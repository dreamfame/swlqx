﻿using UnityEngine;
using System.Collections;
using Assets.Scripts;
using NAudio;
using NAudio.Wave;
using Assets.Scripts.SimHash;

public class main_test : MonoBehaviour {

    public GameObject CharacterModel;

    public bool isPlayed = false;

    public int ret = 0;

    string session_ID;

    public float answer_time = 0f;

    public bool isAnswer = false;

    MicManage mic;

	// Use this for initialization
	void Start () {
        mic = new MicManage(GetComponent<AudioSource>());
        init();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(2)) 
        {
            NAudioRecorder nar = new NAudioRecorder();
            nar.StartRec();
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            FlowManage.M2PMode(1);
        }
        if (isAnswer) 
        {
            answer_time += Time.deltaTime;
            if (answer_time >= 5) 
            {
                FlowManage.StopAnswer();
                answer_time = 0f;
                isAnswer = false;
                /*int questionNo = FlowManage.curNo;
                questionNo++;
                if (questionNo <= 3)
                {
                    FlowManage.StopAnswer();
                    answer_time = 0f;
                    isAnswer = false;
                    FlowManage.M2PMode(questionNo);
                }
                else
                {
                    answer_time = 0f;
                    isAnswer = false;
                    FlowManage.StopAnswer();
                    FlowManage.P2MMode();
                }*/
            }
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
        FlowManage.EnterStandBy(CharacterModel);
    }

    void OnApplicationQuit()
    {
        NAudioRecorder nar = new NAudioRecorder();
        if (nar.waveSource != null)
        {
            nar.StopRec();
        }
        if (VoiceManage.waveOutDevice != null)
        {
            VoiceManage.waveOutDevice.Stop();
        }
        if (VoiceManage.waveOutDevice != null)
        {
            VoiceManage.waveOutDevice.Dispose();
            VoiceManage.waveOutDevice = null;
        }
    }  
}
