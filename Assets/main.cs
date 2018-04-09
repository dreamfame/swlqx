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

    string session_ID;
    epStatus ep_status = epStatus.MSP_EP_NULL;
    rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    MicManage mic;
    audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

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
        //mic.start();

        //Debug.Log("录制唤醒词!");
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
            mic.stop();
            Debug.Log("停止录音!");
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            try
            {
                Debug.Log(Environment.CurrentDirectory);
                int retCode = MSC.MSPLogin(null, null, msp_login.APPID+ ",engine_start = ivw,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .");
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
                Debug.Log(string.Format("{0} 登陆成功,正在开启引擎..", DateTime.Now.Ticks));
                session_ID = Ptr2Str(MSC.QIVWSessionBegin(string.Empty, "sst=wakeup,ivw_threshold=0:-20,ivw_res_path =fo|res/ivw/wakeupresource.jet,work_dir = .", ref retCode));
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("开启失败!"); return; }
                Debug.Log(string.Format("{1} 开启成功[{0}],正在注册..", session_ID, DateTime.Now.Ticks));
                retCode = MSC.QIVWRegisterNotify(session_ID,registerCallback,new IntPtr());
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("注册失败!"); return; }
                Debug.Log(string.Format("{1} 注册成功,语音唤醒[{0}]正在初始化...", session_ID, DateTime.Now.Ticks));
                //while (!mic_flag)
                //{
                   mic.start(); 

                //Thread.Sleep(5 * 1000);
                    if (mic.getData()==null)
                    {
                        Debug.LogError("未接受有效语音数据！");
                        return;
                    }
                    byte[] recRet = mic.getData();
                
                    Debug.Log(string.Format("{1} 音频长度[{0}]", (uint)mic.getData().Length, DateTime.Now.Ticks));
                //AudioSource audio = GetComponent<AudioSource>();
                //audio.clip.SetData(mic.getData())
                int errCode = MSC.QIVWAudioWrite(session_ID, Byte2Ptr(mic.getData()), (uint)mic.getData().Length, audio_stat);
                Debug.Log(string.Format("QIVWAudioWrite returned: {0}", errCode));
                //}
                MSC.QIVWSessionEnd(session_ID, string.Empty);
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
                Debug.Log(string.Format("登陆移动语音终端..."));
                int retCode = MSC.MSPLogin(null, null, msp_login.APPID);
                Debug.Log(string.Format("-->移动语音终端登陆结果:retCode[{1}],login success:{0}\n", (retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
                if(retCode != (int)ErrorCode.MSP_SUCCESS) { Debug.Log("登陆失败!"); return; }
                //生成语法ID
                //string grammar_params = string.Format("engine_type = local, asr_res_path = {0}, grm_build_path = {1}", asr_res_path, grm_build_path);
                //int grammar_ID = MSC.QISRBuildGrammar("bnf","",10,grammar_params,,);
                //string session_params = string.Format("engine_type = {0}, asr_res_path = {1}, grm_build_path = {2}, local_grammar = {3}, result_type = json, result_encoding = UTF-8","local",asr_res_path,grm_build_path,grammar_ID);

                string session_params = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
                session_ID = Ptr2Str(MSC.QISRSessionBegin(string.Empty, session_params,ref retCode));
                Debug.Log(string.Format("-->开启一次语音识别[{0}]", session_ID));
                retCode = MSC.QISRAudioWrite(session_ID, IntPtr.Zero,0,audioStatus.MSP_AUDIO_SAMPLE_LAST,ref ep_status,ref rec_status);
                Debug.Log(string.Format("-->语音读写中({0},{1})...", ep_status, rec_status));
                //if (ep_status == epStatus.MSP_EP_AFTER_SPEECH || ep_status == epStatus.MSP_EP_TIMEOUT)
                //{
                    int count = 0;
                    while (rec_status != rsltStatus.MSP_REC_STATUS_COMPLETE)
                    {
                    
                        IntPtr p = MSC.QISRGetResult(session_ID, ref rec_status, 0, ref retCode);
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
                        MSC.QISRSessionEnd(session_ID, string.Empty);
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
    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }
}
