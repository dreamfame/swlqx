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
        private float lastPeak;//说话音量
        float secondsRecorded;
        float totalBufferLength;
        int Ends = 5;
        private const int BUFFER_SIZE = 4096;
        List<VoiceData> VoiceBuffer = new List<VoiceData>();
        private bool singleFlag = false;

        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRec(bool singleFlag)
        {
            this.singleFlag = singleFlag;
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
            recorder = new AudioRecorder();
            recorder.BeginMonitoring(-1);
            recorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;
            this.SetWaveInCallback(waveSource_DataAvailable, waveSource_RecordingStopped);
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
            this.SetWaveInCallback(waveSource_DataAvailable, waveSource_RecordingStopped);
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
            return "";
        }
        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="dataAvailable"></param>
        /// <param name="RecordingStopped"></param>
        private void SetWaveInCallback(EventHandler<WaveInEventArgs> dataAvailable, EventHandler RecordingStopped)
        {
            waveSource.DataAvailable -= dataAvailable;
            waveSource.RecordingStopped -= RecordingStopped;
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
                if (VoiceBuffer.Count() > 5)
                {
                    VoiceManage.SpeechRecognition(VoiceBuffer);//调用语音识别
                }
                VoiceBuffer.Clear();
                if (singleFlag)
                {
                    Ends = 0;
                }
                else Ends = 5;
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
