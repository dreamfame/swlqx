using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Assets.Scripts
{
    class MicManage 
    {
        /// <summary>
        /// 麦克风设置
        /// </summary>
        private string wav_file_dir = Environment.CurrentDirectory +"/wav/";
        private int maxRecordTime = 5;
        private int samplingRate = 16000;
        public Byte[] speech_Byte;
        private bool isRecording = false;
        AudioSource source;
        AudioClip micRecord;
        public MicManage(AudioSource source)
        {
            this.source = source;
        }
        public string startRecording(string fileName)
        {
            if (!Microphone.IsRecording(null))
            {
                Microphone.End(null);
                micRecord = Microphone.Start(null,false, maxRecordTime, samplingRate);
                while (!(Microphone.GetPosition(null) > 0)) { }
                Debug.Log("开始录音..");
                PlayAudioClip();
                Thread.Sleep(maxRecordTime * 1000);
                return saveRecord(fileName);
            }
            return string.Empty;
        }
        public string saveRecord(string fileName)
        {
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);
            }
            if (SaveWav(fileName, micRecord))
            {
                if (!fileName.ToLower().EndsWith(".wav"))
                {
                    fileName += ".wav";
                }
                return wav_file_dir + fileName;
            }
            else return string.Empty;
        }


        public void PlayAudioClip()
        {
            if (micRecord.length > 0 && micRecord != null)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
                Debug.Log("Channel :" + micRecord.channels + " ;Samle :" + micRecord.samples + " ;frequency :" + micRecord.frequency + " ;length :" + micRecord.length);
                source.clip = micRecord;
                source.Play();
            }
        }
        /// <summary>
        /// 音频保存为wav文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        bool SaveWav(string filename, AudioClip clip)
        {
            try
            {
                if (!filename.ToLower().EndsWith(".wav"))
                {
                    filename += ".wav";
                }

                string filePath = wav_file_dir + filename;

                Debug.Log("Record Ok :" + filePath);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (FileStream fileStream = CreateEmpty(filePath))
                {
                    ConvertAndWrite(fileStream, clip);

                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log("error : " + ex);
                return false;
            }
        }
        FileStream CreateEmpty(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < 44; i++)//头文件长度为44
            {
                fileStream.WriteByte(emptyByte);
            }
            return fileStream;
        }
        #region
        /// <summary>
        /// 音频数据转文件流
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="clip"></param>
        void ConvertAndWrite(FileStream fileStream, AudioClip clip)
        {
            float[] samples = new float[clip.samples];

            clip.GetData(samples, 0);

            Int16[] intData = new Int16[samples.Length];

            Byte[] bytesData = new Byte[samples.Length * 2];

            int rescaleFactor = 32767; //to convert float to Int16  

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);

                Byte[] byteArr = new Byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            speech_Byte = bytesData;

            fileStream.Write(bytesData, 0, bytesData.Length);

            WriteHeader(fileStream, clip);
        }
        void WriteHeader(FileStream fileStream, AudioClip clip)
        {

            int hz = clip.frequency;
            int channels = clip.channels;
            int samples = clip.samples;

            fileStream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            UInt16 two = 2;
            UInt16 one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(channels);
            fileStream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(hz);
            fileStream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2    
            fileStream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort)(channels * 2);
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(samples * 2 * channels);
            fileStream.Write(subChunk2, 0, 4);

            fileStream.Close();
        }
        #endregion
    }
}
