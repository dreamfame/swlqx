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
using NAudio;
using NAudio.Wave;

public class VoiceManage
{
    public VoiceManage()
    {

    }

    public static int ret = 0;

    private static MicManage mic = new MicManage(Camera.main.GetComponent<AudioSource>());

    private static NAudioRecorder nar = new NAudioRecorder();

    private static audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

    public static epStatus ep_status = epStatus.MSP_EP_NULL;
    public static rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    private static int FRAME_LEN = 640;

    private static int BUFFER_SIZE = 4096;

    private static bool isWakeUp = false;

    public static IWavePlayer waveOutDevice;

    public static AudioFileReader audioFileReader;

    private static string grammar_id;
    private static int waitGrmBuildFlag;

    private class msp_login
    {
        public static string APPID = "appid = 5ae7ea3a";
        public static string Account = "390378816@qq.com";
        public static string Passwd = "ai910125.0";
    }

    /// <summary>
    /// 合成语音
    /// </summary>
    /// <param name="text">语音内容</param>
    /// <param name="name">文件名</param>
    /// <param name="path">音频存放地址</param>
    public void PlayVoice(string text, string name, string path)
    {
        string sid = string.Empty;
        try
        {
            string login_configs = msp_login.APPID+", work_dir = .";//登录参数,自己注册后获取的appid  
            ret = MSC.MSPLogin(string.Empty, string.Empty, login_configs);//第一个参数为用户名，第二个参数为密码，第三个参数是登录参数，用户名和密码需要在http://open.voicecloud.cn  
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!" + ret); return; }
            //string @params = "engine_type = cloud,voice_name=nannan,speed=50,volume=50,pitch=50,text_encoding =UTF8,background_sound=1,sample_rate=16000";
            string @params = "engine_type = local, voice_name = xiaoyan, text_encoding = UTF8, tts_res_path = fo|res\\tts\\xiaoyan.jet;fo|res\\tts\\common.jet, sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 1";
            sid = Ptr2Str(MSC.QTTSSessionBegin(@params, ref ret));
            Debug.Log(string.Format("-->开启一次语音合成[{0}]", sid));
            SpeechSynthesis(sid,text, name,path);
        }
        finally
        {
            MSC.QTTSSessionEnd(sid, string.Empty);
            MSC.MSPLogout();//退出登录
        }
    }

    /// <summary>
    /// 语音唤醒
    /// </summary>
    public static void VoiceWakeUp()
    {
        try
        {
            //Debug.Log(Environment.CurrentDirectory);
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
            Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
            string sid = Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20", ref retCode));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("开启失败!"); return; }
            Debug.Log(string.Format("{1} 开启成功[{0}],正在注册..", sid, DateTime.Now.Ticks));
            retCode = MSC.QIVWRegisterNotify(sid, registerCallback, new IntPtr());
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("注册失败!"); return; }
            Debug.Log(string.Format("{1} 注册成功,语音唤醒[{0}]正在初始化...", sid, DateTime.Now.Ticks));
            VoiceArousal(sid);
            MSC.QIVWSessionEnd(sid, string.Empty);                   
        }
        finally
        {
            MSC.MSPLogout();
        }
    }
    /// <summary>
    /// 构建语法网络
    /// </summary>
    int GrammarBuild(string grm_type,string file)
    {
        string @params = "engine_type=local, sample_rate=16000, asr_res_path=fo|res/asr/common.jet, grm_build_path=res/asr/GrmBuilld";
        //string @params = "engine_type=cloud,sample_rate=16000";
        string grm_content = Utils.FileGetString(file);
        uint grm_cnt_len = (uint)System.Text.Encoding.Default.GetBytes(grm_content).Length;
        QISRUserData userdata = getUserData();
        Debug.Log(grm_content);
        return MSC.QISRBuildGrammar(grm_type, grm_content, grm_cnt_len, @params, grammarCallBack,ref userdata);
    }
    /// <summary>
    /// 构建语法-回调函数
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="info"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    static int grammarCallBack(int errorCode, string info,QISRUserData userData)
    {
        Debug.Log("语法构建结果:" + errorCode);
        grammar_id = info;
        waitGrmBuildFlag = 1;
        return 0;

    }
    /// <summary>
    /// 语音识别
    /// </summary>
    /// <returns>返回识别转换的文字</returns>
    public string VoiceDistinguish()
    {
        string result_string = "";
        try
        {
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",work_dir = .");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败:" + retCode); return ""; }
            Debug.Log(string.Format("登陆成功,语音识别正在加载..."));
            //离线构建语法网络
            waitGrmBuildFlag = 0;
            retCode = GrammarBuild("bnf", "call.bnf");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("语法构建失败:" + retCode); return string.Empty; }
            while (waitGrmBuildFlag == 0){}//等待语法构建结果
            //语音转文字
            //string session_params = "engine_type=cloud,sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = UTF-8 ,vad_eos = 5000";//可停止说话5秒保持语音识别状态
            string session_params = "engine_type=local,asr_threshold=0,asr_denoise=0,local_grammar = " + grammar_id + ",asr_res_path=fo|res/asr/common.jet,grm_build_path=res/asr/GrmBuilld, sample_rate = 16000, result_type = plain, result_encoding = UTF-8 ,vad_eos = 5000";//可停止说话5秒保持语音识别状态
            string sid = Ptr2Str(MSC.QISRSessionBegin(string.Empty, session_params, ref retCode));
            Debug.Log(string.Format("-->开启一次语音识别[{0}]", sid));
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("加载失败!"); return ""; }
            result_string = SpeechRecognition(sid);
            Debug.Log("-->语音识别结果:" + result_string);
            MSC.QISRSessionEnd(sid, string.Empty);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            MSC.MSPLogout();
        }
        return result_string;
    }

    private QISRUserData getUserData()
    {
        return new QISRUserData
        {
            build_fini = 0,
            update_fini = 0,
            errcode = -1,
            grammar_id = ""

        };
    }

    /// <summary>
    /// 获取二进制流音频
    /// </summary>
    /// <returns></returns>
    public static byte[] GetAudioBytes()
    {
        string file = mic.startRecording("rec");
        if (file == string.Empty) { return null; }
        return GetFileData(Application.dataPath + "/Resources/Voice/rec.wav");
    }

    /// <summary>
    /// 语音唤醒方法
    /// </summary>
    /// <param name="sid"></param>
    private static void VoiceArousal(string sid)
    {
        string file = mic.startRecording("hx");
        if (file == string.Empty) { return; }
        //byte[] audio_buffer = GetFileData(file);
        byte[] audio_buffer = GetFileData(Environment.CurrentDirectory + "/wav/rec.wav");
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
    }
    /// <summary>
    /// 语音识别方法
    /// </summary>
    /// <param name="sid"></param>
    private static string SpeechRecognition(string sid)
    {
        Debug.Log("加载成功,正在开启话筒..");
        string file = mic.startRecording("rec");
        if (file == string.Empty) { return ""; }
        byte[] audio_buffer = GetFileData(Application.dataPath + "/Resources/Voice/rec.wav");
        long audio_size = audio_buffer.Length;
        long audio_count = 0;
        string rec_result = string.Empty;
        ep_status = epStatus.MSP_EP_LOOKING_FOR_SPEECH;
        //if (ep_status == epStatus.MSP_EP_AFTER_SPEECH || ep_status == epStatus.MSP_EP_TIMEOUT)
        //{
        while (epStatus.MSP_EP_AFTER_SPEECH != ep_status)
        {
            audio_stat = audioStatus.MSP_AUDIO_SAMPLE_CONTINUE;
            long len = 10 * FRAME_LEN; //16k音频，10帧 （时长200ms）
            if (audio_size < 2 * len) len = (int)audio_size;
            if (len <= 0) break;
            if (0 == audio_count)
                audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;
            ret = MSC.QISRAudioWrite(sid, audio_buffer.Skip((int)audio_count).Take((int)len).ToArray(), (uint)len, audio_stat, ref ep_status, ref rec_status);
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("读取音频失败:{0}!", ret)); return ""; }
            audio_count += len;
            audio_size -= len;
            if (rec_status == rsltStatus.MSP_REC_STATUS_SUCCESS)
            {
                IntPtr p = MSC.QISRGetResult(sid, ref rec_status, 0, ref ret);
                if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("无法识别:{0}!", ret)); return ""; }
                Debug.Log(Ptr2Str(p));
                if (p != IntPtr.Zero)
                {
                    int rslt_len = Ptr2Str(p).Length;
                    rec_result = rec_result + Ptr2Str(p);
                    if (rec_result.Length >= BUFFER_SIZE)
                    {
                        Debug.Log("no enough buffer for rec_result");
                        return "";
                    }
                }
            }
        }
        Debug.Log(string.Format("录制完毕,正在识别中..."));
        ret = MSC.QISRAudioWrite(sid, null, 0, audioStatus.MSP_AUDIO_SAMPLE_LAST, ref ep_status, ref rec_status);
        if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("识别音频失败:{0}!", ret)); return ""; }
        while (rsltStatus.MSP_REC_STATUS_COMPLETE != rec_status)
        {
            IntPtr rslt = MSC.QISRGetResult(sid, ref rec_status, 0, ref ret);
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("音频无法识别:{0}!", ret)); return ""; }

            if (rslt != IntPtr.Zero)
            {
                rec_result = rec_result + Ptr2Str(rslt);
                if (rec_result.Length >= BUFFER_SIZE)
                {
                    Debug.Log("no enough buffer for rec_result");
                    return "";
                }
            }
            Thread.Sleep(150); //防止频繁占用CPU
        }
        return rec_result;
    }
    /// <summary>
    /// 语音合成
    /// </summary>
    /// <param name="sid"></param>
    /// <returns></returns>
    private void SpeechSynthesis(string sid, string text, string name, string path)
    {
        string filename = name+".wav"; //合成的语音文件  
        uint audio_len = 0;
        uint audio_total_len = 0;
        SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
        ret = MSC.QTTSTextPut(sid, text, (uint)Encoding.UTF8.GetByteCount(text), string.Empty);
        if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("写入文本失败!" + ret); return; }
        MemoryStream memoryStream = new MemoryStream();
        while (synth_status != SynthStatus.MSP_TTS_FLAG_DATA_END)
        {
            IntPtr source = MSC.QTTSAudioGet(sid, ref audio_len, ref synth_status, ref ret);
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("合成失败!" + ret); return; }
            byte[] array = new byte[(int)audio_len];
            if (audio_len > 0)
            {
                Marshal.Copy(source, array, 0, (int)audio_len);
                memoryStream.Write(array, 0, array.Length);
                audio_total_len = audio_total_len + audio_len;
            }
            Thread.Sleep(100);
        }
        //Debug.Log(string.Format("音频长度:byte[{0}],memoryStream[{1}]", audio_total_len, memoryStream.Length));
        //添加音频头,否则无法播放
        WAVE_Header wave_Header = getWave_Header((int)memoryStream.Length+44);
        byte[] audio_header = StructToBytes(wave_Header);
        memoryStream.Position = 0;
        memoryStream.Write(audio_header, 0, audio_header.Length);
        memoryStream.Position = 0;
        if (filename != null)
        {
            FileStream fileStream = new FileStream(path + "/" + filename, FileMode.Create, FileAccess.Write);
            memoryStream.WriteTo(fileStream);
            memoryStream.Close();
            fileStream.Close();
            waveOutDevice = new WaveOut();
            //waveOutDevice.PlaybackStopped += waveOutDevice_PlaybackStopped; 
            audioFileReader = new AudioFileReader(Application.dataPath + "/Resources/Voice/" + filename);
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
            FlowManage.StartUserAnswer();
        }
    }

    /// <summary>
    /// 音频播放结束回调函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void waveOutDevice_PlaybackStopped(object sender, EventArgs e)
    {
        if (waveOutDevice != null)
        {
            Debug.Log("开始答题");
            FlowManage.StartUserAnswer();
            waveOutDevice.Dispose();
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
            isWakeUp = false;
            Debug.Log(string.Format("{1} 唤醒失败({0})!", param1, DateTime.Now.Ticks));
        }
        else //唤醒成功消息
        {
            Debug.Log(string.Format("唤醒成功({0})!", info));
            FlowManage.M2PMode(1);
        }
        return 0;
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
            File_Size = data_len -8,
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
            DATA_Size = data_len-44
        };
    }

    /// <summary>  
    /// wav音频头  
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
