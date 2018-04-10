﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Assets.Scripts;
using UnityEngine;

public class VoiceManage {

    public int ret = 0;

    public IntPtr session_ID;

    public static string session_Id;

    private int i = 0;

    private string voice_path = "";

    private static MicManage mic;

    private static audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

    public static epStatus ep_status = epStatus.MSP_EP_NULL;
    public static rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    private class msp_login
    {
        public static string APPID = "appid = 5ab8b014";
        public static string Account = "390378816@qq.com";
        public static string Passwd = "ai910125.0";
        public static string voice_cache = "voice_cache";
    }

    /// <summary>
    /// 播放合成语音
    /// </summary>
    /// <param name="content">语音内容</param>
    /// <param name="name">文件名</param>
    public string PlayVoice(string content,string name,string path)
    {
        try
        {
            ///APPID请勿随意改动  
            string login_configs = "appid =5ab8b014 ";//登录参数,自己注册后获取的appid  
            string text = content;//待合成的文本  
            string filename = name+".wav"; //合成的语音文件  
            uint audio_len = 0;
            voice_path = path + "/" + filename;
            SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
            ret = MSC.MSPLogin(string.Empty, string.Empty, login_configs);//第一个参数为用户名，第二个参数为密码，第三个参数是登录参数，用户名和密码需要在http://open.voicecloud.cn  
            //MSPLogin方法返回失败  
            if (ret != (int)ErrorCode.MSP_SUCCESS)
            {
                return "";
            }
            //string parameter = "engine_type = local, voice_name=xiaoyan, tts_res_path =fo|res\\tts\\xiaoyan.jet;fo|res\\tts\\common.jet, sample_rate = 16000";  
            string _params = "ssm=1,ent=sms16k,vcn=xiaoyan,spd=medium,aue=speex-wb;7,vol=x-loud,auf=audio/L16;rate=16000";
            string @params = "engine_type = cloud,voice_name=xiaoyan,speed=50,volume=50,pitch=50,text_encoding = UTF8,background_sound=1,sample_rate = 16000";
            session_ID = MSC.QTTSSessionBegin(@params, ref ret);
            //QTTSSessionBegin方法返回失败  
            if (ret != (int)ErrorCode.MSP_SUCCESS)
            {
                return "";
            }
            ret = MSC.QTTSTextPut(Ptr2Str(session_ID), text, (uint)Encoding.Default.GetByteCount(text), string.Empty);
            //QTTSTextPut方法返回失败  
            if (ret != (int)ErrorCode.MSP_SUCCESS)
            {
                return "";
            }

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(new byte[44], 0, 44);
            while (true)
            {
                IntPtr source = MSC.QTTSAudioGet(Ptr2Str(session_ID), ref audio_len, ref synth_status, ref ret);
                i++;
                byte[] array = new byte[(int)audio_len];
                if (audio_len > 0)
                {
                    Marshal.Copy(source, array, 0, (int)audio_len);
                }
                memoryStream.Write(array, 0, array.Length);
                Thread.Sleep(1000);
                if (synth_status == SynthStatus.MSP_TTS_FLAG_DATA_END || ret != 0)
                    break;
            }
            Debug.Log(i);
            WAVE_Header wave_Header = getWave_Header((int)memoryStream.Length - 44);
            byte[] array2 = this.StructToBytes(wave_Header);
            memoryStream.Position = 0L;
            memoryStream.Write(array2, 0, array2.Length);
            memoryStream.Position = 0L;
            if (filename != null)
            {
                FileStream fileStream = new FileStream(path+"/"+filename, FileMode.Create, FileAccess.Write);
                memoryStream.WriteTo(fileStream);
                memoryStream.Close();
                fileStream.Close();
            }
            
        }
        catch (Exception)
        {
        }
        finally
        {

            ret = MSC.QTTSSessionEnd(Ptr2Str(session_ID), "");
            ret = MSC.MSPLogout();//退出登录  
        }
        return voice_path;
    }

    /// <summary>
    /// 结构体转字符串
    /// </summary>
    /// <param name="structure"></param>
    /// <returns></returns>
    private byte[] StructToBytes(object structure)
    {
        int num = Marshal.SizeOf(structure);
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        byte[] result;
        try
        {
            Marshal.StructureToPtr(structure, intPtr, false);
            byte[] array = new byte[num];
            Marshal.Copy(intPtr, array, 0, num);
            result = array;
        }
        finally
        {
            Marshal.FreeHGlobal(intPtr);
        }
        return result;
    }
    /// <summary>
    /// 结构体初始化赋值
    /// </summary>
    /// <param name="data_len"></param>
    /// <returns></returns>
    private WAVE_Header getWave_Header(int data_len)
    {
        return new WAVE_Header
        {
            RIFF_ID = 1179011410,
            File_Size = data_len + 36,
            RIFF_Type = 1163280727,
            FMT_ID = 544501094,
            FMT_Size = 16,
            FMT_Tag = 1,
            FMT_Channel = 1,
            FMT_SamplesPerSec = 16000,
            AvgBytesPerSec = 32000,
            BlockAlign = 2,
            BitsPerSample = 16,
            DATA_ID = 1635017060,
            DATA_Size = data_len
        };
    }

    /// <summary>  
    /// 语音音频头  
    /// </summary>  
    private struct WAVE_Header
    {
        public int RIFF_ID;
        public int File_Size;
        public int RIFF_Type;
        public int FMT_ID;
        public int FMT_Size;
        public short FMT_Tag;
        public ushort FMT_Channel;
        public int FMT_SamplesPerSec;
        public int AvgBytesPerSec;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public int DATA_ID;
        public int DATA_Size;
    }
    /// 指针转字符串  
    /// </summary>  
    /// <param name="p">指向非托管代码字符串的指针</param>  
    /// <returns>返回指针指向的字符串</returns>  
    public static string Ptr2Str(IntPtr p)
    {
        List<byte> lb = new List<byte>();
        while (Marshal.ReadByte(p) != 0)
        {
            lb.Add(Marshal.ReadByte(p));
            if (IntPtr.Size == 4)
            {
                p = (IntPtr)(p.ToInt32() + 1);
            }
            else
            {
                p = (IntPtr)(p.ToInt64() + 1);
            }
        }
        byte[] bs = lb.ToArray();
        return Encoding.Default.GetString(lb.ToArray());
    }

    /// <summary>
    /// 语音唤醒
    /// </summary>
    public static void VoiceWakeUp() 
    {
        try
        {
            mic = new MicManage(Camera.main.GetComponent<AudioSource>());
            Debug.Log(Environment.CurrentDirectory);
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
            Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
            session_Id = Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .", ref retCode));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("开启失败!"); return; }
            Debug.Log(string.Format("{1} 开启成功[{0}],正在注册..", session_Id, DateTime.Now.Ticks));
            retCode = MSC.QIVWRegisterNotify(session_Id, registerCallback, new IntPtr());
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("注册失败!"); return; }
            Debug.Log(string.Format("{1} 注册成功,语音唤醒[{0}]正在初始化...", session_Id, DateTime.Now.Ticks));
            //while (!mic_flag)
            //{
            mic.start();

            //Thread.Sleep(5 * 1000);
            if (mic.getData() == null)
            {
                Debug.LogError("未接受有效语音数据！");
                return;
            }
            byte[] recRet = mic.getData();

            Debug.Log(string.Format("{1} 音频长度[{0}]", (uint)mic.getData().Length, DateTime.Now.Ticks));
            //AudioSource audio = GetComponent<AudioSource>();
            //audio.clip.SetData(mic.getData())
            int errCode = MSC.QIVWAudioWrite(session_Id, Byte2Ptr(mic.getData()), (uint)mic.getData().Length, audio_stat);
            Debug.Log(string.Format("QIVWAudioWrite returned: {0}", errCode));
            //}
            MSC.QIVWSessionEnd(session_Id, string.Empty);
        }
        finally
        {
            MSC.MSPLogout();
        }
    }

    /// <summary>
    /// 语音识别
    /// </summary>
    public static void VoiceDistinguish() 
    {
        try
        {
            Debug.Log(string.Format("登陆移动语音终端..."));
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID);
            Debug.Log(string.Format("-->移动语音终端登陆结果:retCode[{1}],login success:{0}\n", (retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
            //生成语法ID
            //string grammar_params = string.Format("engine_type = local, asr_res_path = {0}, grm_build_path = {1}", asr_res_path, grm_build_path);
            //int grammar_ID = MSC.QISRBuildGrammar("bnf","",10,grammar_params,,);
            //string session_params = string.Format("engine_type = {0}, asr_res_path = {1}, grm_build_path = {2}, local_grammar = {3}, result_type = json, result_encoding = UTF-8","local",asr_res_path,grm_build_path,grammar_ID);

            string session_params = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
            session_Id = Ptr2Str(MSC.QISRSessionBegin(string.Empty, session_params, ref retCode));
            Debug.Log(string.Format("-->开启一次语音识别[{0}]", session_Id));
            retCode = MSC.QISRAudioWrite(session_Id, IntPtr.Zero, 0, audioStatus.MSP_AUDIO_SAMPLE_LAST, ref ep_status, ref rec_status);
            Debug.Log(string.Format("-->语音读写中({0},{1})...", ep_status, rec_status));
            //if (ep_status == epStatus.MSP_EP_AFTER_SPEECH || ep_status == epStatus.MSP_EP_TIMEOUT)
            //{
            int count = 0;
            while (rec_status != rsltStatus.MSP_REC_STATUS_COMPLETE)
            {

                IntPtr p = MSC.QISRGetResult(session_Id, ref rec_status, 0, ref retCode);
                if (retCode == (int)ErrorCode.MSP_SUCCESS)
                {
                    Debug.Log("已获取到音频信息,正在识别...");
                    if (p != IntPtr.Zero)
                    {
                        Debug.Log(string.Format("-->语音信息:{0}", Ptr2Str(p)));
                    }
                    else
                    {
                        count++;
                        Thread.Sleep(100);
                        Debug.Log(string.Format("第{0}次抓取语音信息...", count));
                    }
                }
                else
                {
                    Debug.Log("语音识别失败!");
                    return;
                }
            }
            if (count > 20)
            {

            }
            else
            {
                MSC.QISRSessionEnd(session_Id, string.Empty);
            }
            //}
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            MSC.MSPLogout();
        }
    }

    /// <summary>
    /// byte[]转指针
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static IntPtr Byte2Ptr(byte[] bytes)
    {
        int size = bytes.Length;
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, buffer, size);
            return buffer;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }


    /// <summary>
    /// 语音唤醒回调函数
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="msg"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="info"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    static int registerCallback(string sessionID, msgProcCb msg, int param1, int param2, IntPtr info, IntPtr userData)
    {
        if (msgProcCb.MSP_IVW_MSG_ERROR == msg) //唤醒出错消息
        {

            Debug.Log(string.Format("{1} 唤醒失败({0})!", param1, DateTime.Now.Ticks));
        }
        else //唤醒成功消息
        {
            FlowManage.M2PMode(1);
            mic.stop();
            Debug.Log(string.Format("唤醒成功({0})!", info));
        }
        return 0;
    }


}
