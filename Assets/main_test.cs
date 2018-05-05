using UnityEngine;
using System.Collections;
using Assets.Scripts;
using NAudio;
using NAudio.Wave;
using Assets.Scripts.SimHash;
using System.IO;
using System;

public class main_test : MonoBehaviour {

    public GameObject CharacterModel;

    public bool isPlayed = false;

    public int ret = 0;

    string session_ID;

    public float answer_time = 0f;

    public bool UserStartAnswer = false;

    public bool isAnswer = false;

    public bool AnswerAnalysis = false;

    public bool AskMode = false;

    public bool flow_change = false;

    public bool FinishedAnswer = false;

    public float wait_time = 0f;

    public bool isFinished = false;

    public bool isTransit = false;

    MicManage mic;

    NAudioRecorder nar = new NAudioRecorder();

    public float once_ask_time = 0f;

	// Use this for initialization
	void Start () {
        mic = new MicManage(GetComponent<AudioSource>());
        init();
	}
	
	// Update is called once per frame
	void Update () {

        if (VoiceManage.waveOutDevice != null)//音频播放完毕后开始答题
        {
            if (UserStartAnswer)
            {
                if (VoiceManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    UserStartAnswer = false;
                    Debug.Log("开始答题");
                    isAnswer = true;
                    if (VoiceManage.waveOutDevice != null)
                    {
                        VoiceManage.waveOutDevice.Dispose();
                        VoiceManage.waveOutDevice = null;
                    }
                    if (VoiceManage.audioFileReader != null)
                    {
                        VoiceManage.audioFileReader.Close();
                        VoiceManage.audioFileReader = null;
                    }
                    nar.StartRec();
                }
            }
            if (AnswerAnalysis) //沙勿略问我模式答题后给出答案解析
            {
                if (VoiceManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (VoiceManage.waveOutDevice != null)
                    {
                        VoiceManage.waveOutDevice.Dispose();
                        VoiceManage.waveOutDevice = null;
                    }
                    if (VoiceManage.audioFileReader != null)
                    {
                        VoiceManage.audioFileReader.Close();
                        VoiceManage.audioFileReader = null;
                    }
                    Debug.Log("完成答案解析，进入下一题");
                    AnswerAnalysis = false;
                    int questionNo = FlowManage.curNo;
                    questionNo++;
                    if (questionNo <= 3)
                    {
                        FlowManage.M2PMode(questionNo);
                    }
                    else 
                    {
                        FlowManage.PlayTransitVoice(2, "下面进入我问沙勿略环节。");                
                    }
                }
            }
            if (isTransit) //流程过渡播放声音
            {
                if (VoiceManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (VoiceManage.waveOutDevice != null)
                    {
                        VoiceManage.waveOutDevice.Dispose();
                        VoiceManage.waveOutDevice = null;
                    }
                    if (VoiceManage.audioFileReader != null)
                    {
                        VoiceManage.audioFileReader.Close();
                        VoiceManage.audioFileReader = null;
                    }
                    isTransit = false;
                    Debug.Log("完成过渡");
                    flow_change = true;
                }
            }
            if (FinishedAnswer) //我问沙勿略环节回答完毕
            {
                if (VoiceManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (VoiceManage.waveOutDevice != null)
                    {
                        VoiceManage.waveOutDevice.Dispose();
                        VoiceManage.waveOutDevice = null;
                    }
                    if (VoiceManage.audioFileReader != null)
                    {
                        VoiceManage.audioFileReader.Close();
                        VoiceManage.audioFileReader = null;
                    }
                    FinishedAnswer = false;
                    Debug.Log("回答完毕");
                    flow_change = true;
                }
            }
        }
        if (Input.GetMouseButtonDown(2)) 
        {         
            nar.StartRec();
        }
        if (Input.GetMouseButtonUp(2)) 
        {
            nar.StopRec();
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            //StartCoroutine(EnterM2MMode());
            FlowManage.M2PMode(1);
        }
        if (isAnswer)
        {
            answer_time += Time.deltaTime;
            if (answer_time >= 10)
            {
                isAnswer = false;
                answer_time = 0f;
                FlowManage.StopAnswer(nar);
            }
        }
        if (flow_change) 
        {
           flow_change = false;
           AskMode = true;
           nar.StartRec();
        }
        
        if (AskMode) 
        {
            once_ask_time += Time.deltaTime;
            if (once_ask_time >= 10) 
            {
                AskMode = false;
                once_ask_time = 0f;
                FlowManage.P2MMode(nar);
            }
        }

        if (isFinished) 
        {
            isFinished = false;
            init();
        }
	}

    IEnumerator EnterM2MMode() 
    {
        yield return new WaitForSeconds(3);
        FlowManage.M2PMode(1);
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
        if (u.P2M_Ask_Panel == null) 
        {
            Debug.Log("加载我问沙勿略界面失败");
            return;
        }
        u.HideM2PAnswerPanel();
        u.HideP2MAskPanel();
        u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "";
        for (var i = 0; i <= 2; i++)
        {
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(false);
            u.M2P_Answer_Panel.transform.GetChild(i).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(false);
        }
        u.P2M_Ask_Panel.transform.GetChild(4).gameObject.SetActive(false);
        u.P2M_Ask_Panel.transform.GetChild(6).gameObject.SetActive(false);
        CharacterModel.GetComponent<Animation>().Stop();
        FlowManage.EnterStandBy(CharacterModel);
    }

    void OnApplicationQuit()
    {
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
