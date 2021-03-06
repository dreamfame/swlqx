﻿using UnityEngine;
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

public class main : MonoBehaviour
{

    public GameObject Movie;

    public GameObject CharacterModel;

    public bool isPlayed = false;

    public int ret = 0;

    string session_ID;

    public float answer_time = 0f;

    public bool isAnswer = false;

    public static bool isFinishedCompose = false;

    public static string ComposeWavPath = "";

    epStatus ep_status = epStatus.MSP_EP_NULL;
    rsltStatus rec_status = rsltStatus.MSP_REC_STATUS_SUCCESS;

    MicManage mic;
    audioStatus audio_stat = audioStatus.MSP_AUDIO_SAMPLE_FIRST;

    private class msp_login
    {
        public static string APPID = "appid = 5ae7ea3a";
        public static string Account = "390378816@qq.com";
        public static string Passwd = "ai910125.0";
    }

    void Start()
    {
        mic = new MicManage(GetComponent<AudioSource>());
        //init();
    }

    void Update()
    {
        if (isFinishedCompose && File.Exists(Application.dataPath + "/Resources/"+ComposeWavPath+".wav")) 
        {
            StartPlayClip();
            isFinishedCompose = false;
            ComposeWavPath = "";
        }

        /*
        if (isAnswer)
        {
            AnswerResult ar = null;
            VoiceManage vm = new VoiceManage();
            answer_time += Time.deltaTime;
            ///调用语音识别方法，将语音转换为文字内容，然后与答案匹配，根据匹配程度判断回答正误。
            string voice_string = VoiceManage.VoiceDistinguish();
            if (voice_string != string.Empty) 
            {
                ar = AIUI.HttpPost(AIUI.TEXT_SEMANTIC_API, "text=" + Utils.Encode(voice_string));
            }
            ///正确则返回语音回答正确，错误则返回问题分析。无论正误，返回结果后都进入下一题。
            if (ar != null && ar.code == "00000")
            { 
                vm.PlayVoice("你真棒！回答正确，答案是:" + ar.data.answer.text, "answer" + FlowManage.curNo, "Voice");
            }
            else 
            {
                vm.PlayVoice("很抱歉，回答错误，正确答案是:" + ar.data.answer.text, "answer" + FlowManage.curNo, "Voice");
            }
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
        */
        if (Input.GetKeyDown(KeyCode.F))
        {
            //SpVoiceClass sp = new SpVoiceClass();
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
        UIObject u = Camera.main.GetComponent<UIObject>();
        if (u.M2P_Answer_Panel == null) 
        {
            Debug.Log("加载沙勿略问我界面失败");
            return;
        }
        u.M2P_Answer_Panel.SetActive(false);
        CharacterModel.GetComponent<Animation>().Stop();
        //FlowManage.EnterStandBy(CharacterModel);
    }

    public void CloseMovie()
    {
        Movie.SetActive(false);
    }

    public void RecordAnswerTime()
    {
        isAnswer = true;
    }

    public void StartPlayClip() 
    {
        StartCoroutine(goPlay());
    }

    private IEnumerator goPlay()
    {
        string url = "file:///"+Environment.CurrentDirectory+"/Assets/Resources/"+ComposeWavPath+".wav";
        WWW www = new WWW(url);
        var ac = www.GetAudioClip(false);
        var aud = Camera.main.GetComponent<AudioSource>();
        aud.clip = ac;
        yield return www;
        if (aud.clip != null)
        {
            aud.Play();
        }
    } 
}
