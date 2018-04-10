using UnityEngine;
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

public class main : MonoBehaviour {

    public GameObject Movie;

    public bool isPlayed = false;

    public int ret = 0;

    epStatus ep_status = epStatus.MSP_EP_NULL;
    rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    MicManage mic;
    audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;
    private static int FRAME_LEN = 640;
    private class msp_login{
       public static string APPID = "appid = 5ab8b014";
       public static string Account = "390378816@qq.com";
       public static string Passwd = "ai910125.0";
       public static string voice_cache = "voice_cache";
    }
     
    //string asr_res_path = string.Format("access_type1|file_info1|[offset1]|[length1];access_type2|file_info2|[offset2]|[length2]");
    //string grm_build_path = "H:\voice _cache";

    int registerCallback(string sessionID, msgProcCb msg, int param1, int param2, IntPtr info, IntPtr userData)
    {
        Debug.Log(string.Format("唤醒返回状态{0}",msg));
        if (msgProcCb.MSP_IVW_MSG_ERROR == msg) //唤醒出错消息
        {
            Debug.Log(string.Format("{1} 唤醒失败({0})!", param1, DateTime.Now.Ticks));
        }
        else if (msgProcCb.MSP_IVW_MSG_WAKEUP == msg) //唤醒成功消息
        {
            Debug.Log(string.Format("唤醒成功({0})!", info));
        }
        return 0;
    }
    // Use this for initialization
    void Start() {
        // Movie.SetActive(true);
        mic = new MicManage(GetComponent<AudioSource>());
    }
    // Update is called once per frame
    void Update () {
        /*if (!isPlayed)
        {
            isPlayed = true;
            MovieManage m = Movie.GetComponent<MovieManage>();
            if (m.videoNo == 0)
            {
                m.PlayMovie("a1");
            }
            else if (m.videoNo == 1) 
            {
                Movie.SetActive(true);
                m.PlayMovie("a2");
            }
        }*/
        
        if (Input.GetKeyUp(KeyCode.A))
        {
            mic.saveRecord("temp");
            Debug.Log("停止录音!");
        }
        //语音唤醒
        if (Input.GetKeyUp(KeyCode.B))
        {
            try
            {
                int retCode = MSC.MSPLogin(null, null, msp_login.APPID+ ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
                Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
                string sid = Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20", ref retCode));
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("开启失败!"); return; }
                Debug.Log(string.Format("{1} 开启成功[{0}],正在注册..", sid, DateTime.Now.Ticks));
                retCode = MSC.QIVWRegisterNotify(sid, registerCallback,new IntPtr());
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
        if (Input.GetMouseButtonDown(2))
        {
            try
            {
                int retCode = MSC.MSPLogin(null, null, msp_login.APPID+ ",work_dir = .");
                Debug.Log(string.Format("-->移动语音终端登陆结果:retCode[{1}],login success:{0}\n", (retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
                if(retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
                string session_params = "engine_type=cloud,sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = UTF-8";
                string sid = Ptr2Str(MSC.QISRSessionBegin(string.Empty, session_params,ref retCode));
                Debug.Log(string.Format("-->开启一次语音识别[{0}]", sid));
                SpeechRecognition(sid);
                MSC.QISRSessionEnd(sid, string.Empty);
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
        if (Input.GetMouseButtonDown(0)) 
        {
            VoiceManage vm = new VoiceManage();
            string path = vm.PlayVoice("你好，我是沙勿略。很高兴见到你！","welcome","Assets/Resources/voice");
            var ac = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
            var aud = Camera.main.GetComponent<AudioSource>();
            aud.clip = ac;
            aud.Play();
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            AskQuestion aq = new AskQuestion();
            var temp = aq.GetQuestions();
            if (temp == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else
            {
                foreach (var t in temp)
                {
                    Debug.Log(t.title);
                }
            }
        }
    }
    /// <summary>
    /// 语音唤醒方法
    /// </summary>
    /// <param name="sid"></param>
    private void VoiceArousal(string sid)
    {
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
    }
    /// <summary>
    /// 语音识别方法
    /// </summary>
    /// <param name="sid"></param>
    private void SpeechRecognition(string sid)
    {
        string file = mic.startRecording("rec");
        if (file == string.Empty) { return; }
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
            if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("读取音频失败:{0}!", ret)); return; }
            audio_count += (long)len;
            audio_size -= (long)len;
            if (rec_status == rsltStatus.MSP_REC_STATUS_SUCCESS)
            {
                IntPtr p = MSC.QISRGetResult(sid, ref rec_status, 0, ref ret);
                if (ret != (int)ErrorCode.MSP_SUCCESS) { Debug.Log(string.Format("无法识别:{0}!", ret)); return; }
                if (p != IntPtr.Zero)
                {
                    Debug.Log(string.Format("-->语音信息:{0}", Ptr2Str(p)));
                }
            }
            Thread.Sleep(200);
        }
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
        return Encoding.Default.GetString(lb.ToArray());
    }
    /// <summary>
    /// 对象转指针
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static IntPtr Obj2Ptr(System.Object obj)
    {
        if (obj == null)
        {
            return new IntPtr();
        }
        int size = Marshal.SizeOf(obj);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            //将托管对象拷贝到非托管内存
            Marshal.StructureToPtr(obj, buffer, false);
        }
        finally{ 
            //释放非托管内存
            Marshal.FreeHGlobal(buffer);
        }
        return buffer;
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
    protected byte[] GetFileData(string fileUrl)
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
    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }
}
