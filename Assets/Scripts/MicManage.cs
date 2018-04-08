using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class MicManage 
    {
        /// <summary>
        /// 麦克风设置
        /// </summary>
        private int maxRecordTime = 5;
        private int samplingRate = 44100;
        AudioSource source;
        public MicManage(AudioSource source)
        {
            this.source = source;
        }
        public void start()
        {
            if (!Microphone.IsRecording(null))
            {
                source.Stop();
                source.clip = Microphone.Start(null,true, maxRecordTime, samplingRate);
                while (!(Microphone.GetPosition(null) > 0)) { }
                source.Play();
                Debug.Log("开始录音..");
            }
            
        }
        public void stop()
        {
            if (!Microphone.IsRecording(null))
            {
                return;
            }
            Microphone.End(null);
            source.Stop();
        }

        public byte[] getData()
        {
            if (source.clip == null)
            {
                Debug.Log("getData audio.clip is null");
                return null;
            }
            float[] temp = new float[source.clip.samples];
            source.clip.GetData(temp, 0);
            byte[] outData = new byte[temp.Length * 2];
            int reScaleFactor = 32767;

            for (int i = 0; i < temp.Length; i++)
            {
                short tempShort = (short)(temp[i] * reScaleFactor);
                byte[] tempData = System.BitConverter.GetBytes(tempShort);

                outData[i * 2] = tempData[0];
                outData[i * 2 + 1] = tempData[1];
            }
            if (outData == null || outData.Length <= 0)
            {
                Debug.Log("GetClipData intData is null");
                return null;
            }
            return outData;
        }

        public void EndRecording(out int length, out AudioClip outClip)
        {
            int lastPos = Microphone.GetPosition(null);

            if (Microphone.IsRecording(null))
            {
                length = lastPos / samplingRate;
            }
            else
            {
                length = maxRecordTime;
            }

            Microphone.End(null);

            if (length < 1.0f)
            {
                outClip = null;
                return;
            }

            outClip = source.clip;
        }
    }
}
