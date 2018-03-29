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
    private int videoNo = 0;
    private float DeltaT = 0f;

    public int ret = 0;

    public IntPtr session_ID;

    private class msp_login{
       public static string Config = "appid = 5ab8b014";
       public static string Account = "390378816@qq.com";
       public static string Passwd = "ai910125.0";
    };

    private class msp_params_sample
    {
        public string QISR_SessionBeginParams()
        {
            return string.Format("engine_type = {0}, asr_res_path = {1} sample_rate = {2} grm_build_path = {3} local_grammar = {4} result_type = {5}, result_encoding = {6}");
        }
    }

    // Use this for initialization
    void Start () {
       // Movie.SetActive(true);
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
        if (Input.GetMouseButtonDown(2))
        {
            try
            {
                int retCode = MSC.MSPLogin(null, null, "appid = 5ab8b014 ");
                Debug.Log(string.Format("msp retCode[{1}],login success:{0}\n",(retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
                if(retCode != (int)ErrorCode.MSP_SUCCESS) { return; }
                string params_str = string.Format("engine_type = {0}, asr_res_path = {1}, grm_build_path = {2}, local_grammar = {3}, result_type = json, result_encoding = UTF-8",
                    "local",
                    "H:\voice _cache",
                    "H:\voice _cache",
                    ""
                    );
                session_ID = MSC.QTTSSessionBegin(params_str, ref retCode);
                Debug.Log(string.Format("-->start a session[{0}]\n", session_ID));
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

    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }
}
