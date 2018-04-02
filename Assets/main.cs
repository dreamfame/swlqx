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
    public GameObject CharacterModel;
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
        init();
        FlowManage.EnterStandBy(CharacterModel);
	}
	
	// Update is called once per frame
    void Update()
    {
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
                Debug.Log(string.Format("msp retCode[{1}],login success:{0}\n", (retCode == (int)ErrorCode.MSP_SUCCESS), retCode));
                if (retCode != (int)ErrorCode.MSP_SUCCESS) { return; }
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
            string text = "你好，我是沙勿略，很高兴见到你！";
            string path = vm.PlayVoice(text, "welcome", "Assets/Resources/voice");
            var ac = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
            var aud = Camera.main.GetComponent<AudioSource>();
            aud.clip = ac;
            aud.Play();
        }
        if (Input.GetMouseButtonDown(1))
        {
            VoiceManage vm = new VoiceManage();
            AskQuestion aq = new AskQuestion();
            var temp = aq.GetQuestions();
            if (temp == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else
            {
                Debug.Log(temp[0].title);
                string path = vm.PlayVoice(temp[0].title, "1", "Assets/Resources/voice");
                var ac = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
                var aud = Camera.main.GetComponent<AudioSource>();
                aud.clip = ac;
                aud.Play();
            }
        }
    }

    /// <summary>
    /// 程序初始化
    /// </summary>
    public void init() 
    {
        if (CharacterModel == null) 
        {
            Debug.Log("加载人物模型失败");
            return;
        }
        CharacterModel.GetComponent<Animation>().Stop();
    }

    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }
}
