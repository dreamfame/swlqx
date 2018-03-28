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
        if (Input.GetMouseButtonDown(0)) 
        {
            try
            {
                ///APPID请勿随意改动  
                string login_configs = "appid =5ab8b014 ";//登录参数,自己注册后获取的appid  
                string text = "你好，我是沙勿略，很高兴见到你！";//待合成的文本  
                if (string.IsNullOrEmpty(text.Trim()))
                {
                    text = "请输入合成语音的内容";
                }

                string filename = "Call.wav"; //合成的语音文件  
                uint audio_len = 0;

                SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
                ret = TTS.MSPLogin(string.Empty, string.Empty, login_configs);//第一个参数为用户名，第二个参数为密码，第三个参数是登录参数，用户名和密码需要在http://open.voicecloud.cn  
                //MSPLogin方法返回失败  
                if (ret != (int)ErrorCode.MSP_SUCCESS)
                {
                    return;
                }
                //string parameter = "engine_type = local, voice_name=xiaoyan, tts_res_path =fo|res\\tts\\xiaoyan.jet;fo|res\\tts\\common.jet, sample_rate = 16000";  
                //string _params = "ssm=1,ent=sms16k,voice_name=xiaoyan,spd=medium,aue=speex-wb;7,vol=x-loud,auf=audio/L16;rate=16000";
                string @params = "engine_type = cloud,voice_name=xiaoyan,speed=50,volume=50,pitch=50,rcn=1, text_encoding = UTF8, background_sound=1,sample_rate = 16000";
                session_ID = TTS.QTTSSessionBegin(@params, ref ret);
                //QTTSSessionBegin方法返回失败  
                if (ret != (int)ErrorCode.MSP_SUCCESS)
                {
                    return;
                }
                ret = TTS.QTTSTextPut(Ptr2Str(session_ID), text, (uint)Encoding.Default.GetByteCount(text), string.Empty);
                //QTTSTextPut方法返回失败  
                if (ret != (int)ErrorCode.MSP_SUCCESS)
                {
                    return;
                }

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(new byte[200], 0, 200);
                while (true)
                {
                    IntPtr source = TTS.QTTSAudioGet(Ptr2Str(session_ID), ref audio_len, ref synth_status, ref ret);
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
                WAVE_Header wave_Header = getWave_Header((int)memoryStream.Length - 200);
                byte[] array2 = this.StructToBytes(wave_Header);
                memoryStream.Position = 0L;
                memoryStream.Write(array2, 0, array2.Length);
                memoryStream.Position = 0L;
                if (filename != null)
                {
                    FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    memoryStream.WriteTo(fileStream);
                    memoryStream.Close();
                    fileStream.Close();
                }

            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
            finally
            {
                ret = TTS.QTTSSessionEnd(Ptr2Str(session_ID), "");
                ret = TTS.MSPLogout();//退出登录  
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
