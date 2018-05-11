using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using UnityEngine;
using System.Runtime.InteropServices;
using Assets.Scripts.VoiceRecorder;

namespace Assets.Scripts
{
	public class NAudioRecorder
	{
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        private AudioRecorder recorder;
        private string fileName = "";
        private float lastPeak;//说话音量
        float secondsRecorded;
        float totalBufferLength;
        int Ends = 5;
        private const int BUFFER_SIZE = 4096;
        List<VoiceData> VoiceBuffer = new List<VoiceData>();

        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRec()
        {
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
            recorder = new AudioRecorder();
            recorder.BeginMonitoring(-1);
            recorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;
            waveSource.DataAvailable += waveSource_DataAvailable;
            waveSource.RecordingStopped += waveSource_RecordingStopped;
            //fileName = Application.dataPath + "/Resources/Voice/rec.wav";
            //waveFile = new WaveFileWriter(fileName, waveSource.WaveFormat);

            waveSource.StartRecording();
        }

        void OnRecorderMaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample)) * 100;
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public string StopRec()
        {
            recorder.SampleAggregator.MaximumCalculated -= OnRecorderMaximumCalculated;
            waveSource.DataAvailable -= waveSource_DataAvailable;
            waveSource.RecordingStopped -= waveSource_RecordingStopped;
            waveSource.StopRecording();
            // Close Wave(Not needed under synchronous situation)
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
            //string result = new VoiceManage().VoiceDistinguish();
            return "";
        }

        /// <summary>
        /// 录音结束后保存的文件路径
        /// </summary>
        /// <param name="fileName">保存wav文件的路径名</param>
        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            totalBufferLength += e.Buffer.Length;
            secondsRecorded = (float)(totalBufferLength / 32000);

            VoiceData data = new VoiceData();
            for (int i = 0; i < 3200; i++)
            {
                data.data[i] = e.Buffer[i];
            }
            VoiceBuffer.Add(data);

            if (lastPeak < 20)
                Ends = Ends - 1;
            else
                Ends = 5;
            if (Ends == 0)
            {
                Debug.Log(totalBufferLength);
                if (VoiceBuffer.Count() > 5)
                {
                    VoiceManage.SpeechRecognition(VoiceBuffer);//调用语音识别
                }

                VoiceBuffer.Clear();
                Ends = 5;
            }
            /*if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
                
            }*/
        }

        /// <summary>
        /// 录音结束回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_RecordingStopped(object sender, EventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }
    }
}
