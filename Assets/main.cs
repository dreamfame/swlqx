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

    public int ret = 0;

    string session_ID;

    public float answer_time = 0f;

    public bool isAnswer = false;
 
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
            Debug.Log(string.Format("唤醒成功({0})!", info));
        }
        return 0;
    }

    void Start() {
        mic = new MicManage(GetComponent<AudioSource>());
        init();
    }

	void Update () {
        if (isAnswer)
        {
            answer_time += Time.deltaTime;
            ///调用语音识别方法，将语音转换为文字内容，然后与答案匹配，根据匹配程度判断回答正误。
            ///正确则返回语音回答正确，错误则返回问题分析。无论正误，返回结果后都进入下一题。
            if (answer_time >= 20)
            {
                ///如果20秒之内未作出回答视为错误，给出解析后进入下一题。
                isAnswer = false;
                answer_time = 0f;
                int no = FlowManage.curNo + 1;
                if (no <= 3)
                {
                    FlowManage.M2PMode(no);
                }
            }
        }
        //语音合成测试
        if (Input.GetMouseButtonDown(0)) 
        {
            VoiceManage vm = new VoiceManage();
            string path = vm.PlayVoice("你好，我是沙勿略。很高兴见到你！","welcome","Assets/Resources/voice");
            var ac = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
            var aud = Camera.main.GetComponent<AudioSource>();
            aud.clip = ac;
            aud.Play();
        }
    }

    /// <summary>
    /// 系统初始化
    /// </summary>
    public void init()
    {
        if (CharacterModel == null)
        {
            Debug.Log("加载人物模型失败");
            return;
        }
        CharacterModel.GetComponent<Animation>().Stop();
        FlowManage.EnterStandBy(CharacterModel);
    }

    public void CloseMovie() 
    {
        Movie.SetActive(false);
    }

    public void RecordAnswerTime() 
    {
        isAnswer = true;
    }
}
