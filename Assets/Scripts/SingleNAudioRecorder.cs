using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using UnityEngine;
using System.Threading;

namespace Assets.Scripts
{
	public class SingleNAudioRecorder
	{
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        private string fileName = "";
        private static main_test mt;

        public static string result = "";

        public void StartRec() 
        {
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
            waveSource.DataAvailable += waveSource_DataAvailable;
            waveSource.RecordingStopped += waveSource_RecordingStopped;
            fileName = Application.dataPath + "/Resources/Voice/rec.wav";
            waveFile = new WaveFileWriter(fileName, waveSource.WaveFormat);
            waveSource.StartRecording();
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRec()
        {
            mt = Camera.main.GetComponent<main_test>();
            waveSource.StopRecording();
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
            Thread thread = new Thread(new ThreadStart(Distinguish));
            thread.IsBackground = true;
            thread.Start();
        }

        static object obj = new object();

        static void Distinguish()
        {
            lock (obj)
            {
                if (new VoiceManage().SingleVoiceDistinguish() == RecogStatus.ISR_REC_STATUS_SPEECH_COMPLETE) 
                {
                    mt.successDistinguish = true;
                }
            }
        }

        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
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
