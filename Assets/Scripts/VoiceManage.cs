using Assets.Scripts;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

public class VoiceManage
{
    public static int ret = 0;

    private static MicManage mic = new MicManage(Camera.main.GetComponent<AudioSource>());

    public static NAudioRecorder nar = new NAudioRecorder();

    private static main_test mt = Camera.main.GetComponent<main_test>();

    private static audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

    public static epStatus ep_status = epStatus.MSP_EP_NULL;
    public static RecogStatus recoStatus = RecogStatus.ISR_REC_NULL;
    public static rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;
    public static SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;

    private static int FRAME_LEN = 640;

    private static int BUFFER_SIZE = 4096;

    private static bool isWakeUp = false;

    public static IWavePlayer waveOutDevice;

    public static AudioFileReader audioFileReader;

    private static string grammar_id;
    private static int waitGrmBuildFlag;

    private static Thread satecheck = null;

    private static string sid = "";

    private static string speech_param = "engine_type=cloud,sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = UTF-8 ,vad_eos = 5000";

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
    public SynthStatus PlayVoice(string text, string name, string path)
    {
        try
        {
            string login_configs = msp_login.APPID+", work_dir = .";//登录参数,自己注册后获取的appid  
            ret = MSC.MSPLogin(string.Empty, string.Empty, login_configs);//第一个参数为用户名，第二个参数为密码，第三个参数是登录参数，用户名和密码需要在http://open.voicecloud.cn  
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!" + ret); return SynthStatus.MSP_TTS_FLAG_CMD_CANCELED; }
            //string @params = "engine_type = cloud,voice_name=nannan,speed=50,volume=50,pitch=50,text_encoding =UTF8,background_sound=1,sample_rate=16000";
            string @params = "engine_type = local, voice_name = xiaoyan, text_encoding = UTF8, tts_res_path = fo|res\\tts\\xiaoyan.jet;fo|res\\tts\\common.jet, sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 1";
            sid = Utils.Ptr2Str(MSC.QTTSSessionBegin(@params, ref ret));
            Debug.Log(string.Format("-->开启一次语音合成[{0}]", sid));
            return SpeechSynthesis(sid,text, name,path);
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
            int retCode = MSC.MSPLogin(null, null, msp_login.APPID + ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
            if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
            Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
            sid = Utils.Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20", ref retCode));
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
        string @params = "engine_type=local,asr_res_path=fo|res/asr/common.jet, sample_rate=16000, grm_build_path=res/asr/GrmBuilld";
        //string @params = "engine_type=cloud,sample_rate=16000";
        string grm_content = File.ReadAllText(@"call.bnf", Encoding.Default);
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
            int ret = MSC.MSPLogin(null, null, msp_login.APPID + ",work_dir = .");
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败:" + ret); MSC.MSPLogout(); return ""; }
            Debug.Log(string.Format("登陆成功,语音识别正在加载..."));
            //离线构建语法网络
            //waitGrmBuildFlag = 0;
            //retCode = GrammarBuild("bnf", "call.bnf");
            //Debug.Log("返回码是：" + retCode);
            //if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("语法构建失败:" + ret); return string.Empty; }
            //while (waitGrmBuildFlag == 0){}//等待语法构建结果
            //语音转文字
            //string session_params = "engine_type=local,asr_threshold=0,asr_denoise=0,local_grammar = " + grammar_id + ",asr_res_path=fo|res/asr/common.jet,grm_build_path=res/asr/GrmBuilld, sample_rate = 16000, result_type = plain, result_encoding = GB2312 ,vad_eos = 5000";//可停止说话5秒保持语音识别状态
            nar.StartRec(false);
            result_string = "";//SpeechRecognition(sid);
            Debug.Log("-->语音识别结果:" + result_string);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        return result_string;
    }

    /// <summary>
    /// 单次识别
    /// </summary>
    /// <returns></returns>
    public string SingleVoiceDistinguish()
    {
        string result_string = "";
        try
        {
            int ret = MSC.MSPLogin(null, null, msp_login.APPID + ",work_dir = .");
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败:" + ret); MSC.MSPLogout(); return ""; }
            Debug.Log(string.Format("登陆成功,语音识别正在加载..."));
            //离线构建语法网络
            //waitGrmBuildFlag = 0;
            //retCode = GrammarBuild("bnf", "call.bnf");
            //Debug.Log("返回码是：" + retCode);
            //if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("语法构建失败:" + ret); return string.Empty; }
            //while (waitGrmBuildFlag == 0){}//等待语法构建结果
            //语音转文字
            //string session_params = "engine_type=local,asr_threshold=0,asr_denoise=0,local_grammar = " + grammar_id + ",asr_res_path=fo|res/asr/common.jet,grm_build_path=res/asr/GrmBuilld, sample_rate = 16000, result_type = plain, result_encoding = GB2312 ,vad_eos = 5000";//可停止说话5秒保持语音识别状态
            nar.StartRec(true);
            result_string = "";//SpeechRecognition(sid);
            Debug.Log("-->语音识别结果:" + result_string);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
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
    /// 语音唤醒方法
    /// </summary>
    /// <param name="sid"></param>
    private static void VoiceArousal(string sid)
    {
        string file = mic.startRecording("hx");
        if (file == string.Empty) { return; }
        //byte[] audio_buffer = GetFileData(file);
        byte[] audio_buffer = Utils.GetFileData(Environment.CurrentDirectory + "/wav/rec.wav");
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
    public static void SpeechRecognition(List<VoiceData> VoiceBuffer)
    {
        string rec_result = String.Empty;
        audio_stat = audioStatus.MSP_AUDIO_SAMPLE_CONTINUE;
        ep_status = epStatus.MSP_EP_LOOKING_FOR_SPEECH;
        recoStatus = RecogStatus.ISR_REC_STATUS_SUCCESS;
        sid = Utils.Ptr2Str(MSC.QISRSessionBegin(string.Empty, speech_param, ref ret));
        Debug.Log(string.Format("-->开启一次语音识别[{0}]", sid));
        if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("加载失败!"); return; }

        for (int i = 0; i < VoiceBuffer.Count(); i++)
        {
            audio_stat = audioStatus.MSP_AUDIO_SAMPLE_CONTINUE;
            if (i == 0)
                audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;
            ret = MSC.QISRAudioWrite(sid, VoiceBuffer[i].data, (uint)VoiceBuffer[i].data.Length, audio_stat, ref ep_status, ref recoStatus);
            if ((int)ErrorCode.MSP_SUCCESS != ret)
            {
                MSC.QISRSessionEnd(sid, null);
            }
        }

        ret = MSC.QISRAudioWrite(sid, null, 0, audioStatus.MSP_AUDIO_SAMPLE_LAST, ref ep_status, ref recoStatus);
        if ((int)ErrorCode.MSP_SUCCESS != ret)
        {
            Debug.Log("\nQISRAudioWrite failed! error code:" + ret);
            return;
        }

        while (RecogStatus.ISR_REC_STATUS_SPEECH_COMPLETE != recoStatus)
        {
            IntPtr rslt = MSC.QISRGetResult(sid, ref recoStatus, 0, ref ret);
            if ((int)ErrorCode.MSP_SUCCESS != ret)
            {
                Debug.Log("\nQISRGetResult failed, error code: " + ret);
                break;
            }
            if (IntPtr.Zero != rslt)
            {
                string tempRes = Utils.Ptr2Str(rslt);

                rec_result = rec_result + tempRes;
                if (rec_result.Length >= BUFFER_SIZE)
                {
                    Debug.Log("\nno enough buffer for rec_result !\n");
                    break;
                }
            }

        }
        int errorcode = MSC.QISRSessionEnd(sid, "正常结束");

        //语音识别结果
        if (rec_result.Length != 0)
        {
            Debug.Log("识别结果是："+rec_result);
        }
    }

    public static void StopSpeech() 
    {
        Debug.Log("停止");
        int ret = MSC.QISRSessionEnd(sid, string.Empty);
        MSC.MSPLogout();
        nar.StopRec();
    }

    /// <summary>
    /// 语音合成
    /// </summary>
    /// <param name="sid"></param>
    /// <returns></returns>
    private SynthStatus SpeechSynthesis(string sid, string text, string name, string path)
    {
        string filename = name+".wav"; //合成的语音文件  
        uint audio_len = 0;
        uint audio_total_len = 0;
        synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
        ret = MSC.QTTSTextPut(sid, text, (uint)Encoding.UTF8.GetByteCount(text), string.Empty);
        if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("写入文本失败!" + ret); return synth_status; }
        MemoryStream memoryStream = new MemoryStream();
        while (synth_status != SynthStatus.MSP_TTS_FLAG_DATA_END)
        {
            IntPtr source = MSC.QTTSAudioGet(sid, ref audio_len, ref synth_status, ref ret);
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("合成失败!" + ret); return synth_status; }
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
        Utils.WAVE_Header wave_Header = Utils.getWave_Header((int)memoryStream.Length+44);
        byte[] audio_header = Utils.StructToBytes(wave_Header);
        memoryStream.Position = 0;
        memoryStream.Write(audio_header, 0, audio_header.Length);
        memoryStream.Position = 0;
        if (filename != null)
        {
            FileStream fileStream = new FileStream(path + "/" + filename, FileMode.Create, FileAccess.Write);
            memoryStream.WriteTo(fileStream);
            memoryStream.Close();
            fileStream.Close();
        }
        return synth_status;
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
}
