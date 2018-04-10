using System.Collections;
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

public class VoiceManage
{

    public static int ret = 0;

    public IntPtr session_ID;

    private int i = 0;

    private string voice_path = "";

    private static MicManage mic = new MicManage(Camera.main.GetComponent<AudioSource>());

    private static audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

    public static epStatus ep_status = epStatus.MSP_EP_NULL;
    public static rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    private static int FRAME_LEN = 640;

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
    public string PlayVoice(string content, string name, string path)
    {
        try
        {
            ///APPID请勿随意改动  
            string login_configs = "appid =5ab8b014 ";//登录参数,自己注册后获取的appid  
            string text = content;//待合成的文本  
            string filename = name + ".wav"; //合成的语音文件  
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
                FileStream fileStream = new FileStream(path + "/" + filename, FileMode.Create, FileAccess.Write);
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
    /// 语音唤醒
    /// </summary>
    public static void VoiceWakeUp()
    {
        try
        {
            Debug.Log(Environment.CurrentDirectory);
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
            Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
            string sid = Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .", ref retCode));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("开启失败!"); return; }
            Debug.Log(string.Format("{1} 开启成功[{0}],正在注册..", sid, DateTime.Now.Ticks));
            retCode = MSC.QIVWRegisterNotify(sid, registerCallback, new IntPtr());
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("注册失败!"); return; }
            Debug.Log(string.Format("{1} 注册成功,语音唤醒[{0}]正在初始化...", sid, DateTime.Now.Ticks));
            string file = mic.startRecording("hx");
            if (file == string.Empty) { return; }
            byte[] audio_buffer = GetFileData(file);
            int audio_size = audio_buffer.Length;
            int audio_count = 0;
            while (audio_stat != audioStatus.MSP_AUDIO_SAMPLE_LAST)
            {
                int len = 10 * FRAME_LEN; //16k音频，10帧 （时长200ms）
                audio_stat = audioStatus.MSP_AUDIO_SAMPLE_CONTINUE;
                if (audio_size <= len)
                {
                    len = audio_size;
                    audio_stat = audioStatus.MSP_AUDIO_SAMPLE_LAST; //最后一块
                }
                if (0 == audio_count)
                {
                    audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;
                }
                //Debug.Log(string.Format("{1} 音频长度[{0}]", len, DateTime.Now.Ticks));
                ret = MSC.QIVWAudioWrite(sid, audio_buffer.Skip(audio_count).Take(len).ToArray(), (uint)len, audio_stat);
                if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("{0} 语音唤醒失败:{1}", DateTime.Now.Ticks, ret)); return; }
                audio_count += len;
                audio_size -= len;
                Thread.Sleep(200);
            }
            MSC.QIVWSessionEnd(sid, string.Empty);
        }
        finally
        {
            MSC.MSPLogout();
        }
    }

    /// <summary>
    /// 语音识别
    /// </summary>
    /// <returns>返回识别转换的文字</returns>
    public static string VoiceDistinguish()
    {
        IntPtr p = IntPtr.Zero ;
        try
        {
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",work_dir = .");
            Debug.Log(string.Format("-->移动语音终端登陆结果:retCode[{1}],login success:{0}\n", (retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return ""; }
            string session_params = "engine_type=cloud,sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = UTF-8";
            string sid = Ptr2Str(MSC.QISRSessionBegin(string.Empty, session_params, ref retCode));
            string file = mic.startRecording("rec");
            if (file == string.Empty) { return ""; }
            byte[] audio_buffer = GetFileData(file);
            long audio_size = audio_buffer.Length;
            long audio_count = 0;
            ep_status = epStatus.MSP_EP_LOOKING_FOR_SPEECH;
            //if (ep_status == epStatus.MSP_EP_AFTER_SPEECH || ep_status == epStatus.MSP_EP_TIMEOUT)
            //{
            while (epStatus.MSP_EP_AFTER_SPEECH == ep_status)
            {
                audio_stat = audioStatus.MSP_AUDIO_SAMPLE_CONTINUE;
                int len = 10 * FRAME_LEN; //16k音频，10帧 （时长200ms）
                if (audio_size < 2 * len)
                    len = (int)audio_size;
                if (len <= 0)
                    break;
                if (0 == audio_count)
                    audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;
                ret = MSC.QISRAudioWrite(sid, audio_buffer, (uint)len, audio_stat, ref ep_status, ref rec_status);
                if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("读取音频失败:{0}!", ret)); return ""; }
                audio_count += (long)len;
                audio_size -= (long)len;
                if (rec_status == rsltStatus.MSP_REC_STATUS_SUCCESS)
                {
                    p = MSC.QISRGetResult(sid, ref rec_status, 0, ref ret);
                    if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("无法识别:{0}!", ret)); return ""; }
                    if (p != IntPtr.Zero)
                    {
                        Debug.Log(string.Format("-->语音信息:{0}", Ptr2Str(p)));
                    }
                }
                Thread.Sleep(200);
            }
            Debug.Log(string.Format("-->开启一次语音识别[{0}]", sid));
            MSC.QISRSessionEnd(sid, string.Empty);
            return Ptr2Str(p);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            MSC.MSPLogout();
        }
        return "";
    }


    #region 通用方法

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
        Debug.Log(string.Format("唤醒返回状态{0}", msg));
        if (msgProcCb.MSP_IVW_MSG_ERROR == msg) //唤醒出错消息
        {

            Debug.Log(string.Format("{1} 唤醒失败({0})!", param1, DateTime.Now.Ticks));
        }
        else //唤醒成功消息
        {
            FlowManage.M2PMode(1);
            Debug.Log(string.Format("唤醒成功({0})!", info));
        }
        return 0;
    }

    /// <summary>
    /// 将文件转为byte[]
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <returns></returns>
    protected static byte[] GetFileData(string fileUrl)
    {
        FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
        try
        {
            byte[] buffur = new byte[fs.Length];
            fs.Read(buffur, 0, (int)fs.Length);
            return buffur;
        }
        catch (Exception ex)
        {
            //MessageBoxHelper.ShowPrompt(ex.Message);
            return null;
        }
        finally
        {
            if (fs != null)
            {
                //关闭资源
                fs.Close();
            }
        }
    }
    #endregion
}
