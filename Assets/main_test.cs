using UnityEngine;
using System.Collections;
using Assets.Scripts;
using NAudio;
using NAudio.Wave;
using Assets.Scripts.SimHash;
using System.IO;
using System;
using System.Collections.Generic;

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

    public bool canPlay = false;

    public float wait_time = 0f;

    public bool isFinished = false;

    public bool isTransit = false;

    private int curMode = 0;

    NAudioRecorder nar = new NAudioRecorder();

    SingleNAudioRecorder singleNar = new SingleNAudioRecorder();

    private static UIObject u;

    public float once_ask_time = 0f;

    public string voice_path = "";

    public List<Answer> BeforeAskList = new List<Answer>();

    public List<Answer> AfterAskList = new List<Answer>();

    public List<Answer> AnswerList = new List<Answer>();

    public bool ProjectStart = false;

	// Use this for initialization
	void Start () {
        if (VoiceManage.MSCLogin() != (int)ErrorCode.MSP_SUCCESS)
        { Debug.Log("登陆失败!" + ret); MSC.MSPLogout(); return; }
        u = Camera.main.GetComponent<UIObject>();
        BeforeAskList = Answer.LoadQuestions(2, 1);
        AfterAskList = Answer.LoadQuestions(2, 2);
        AnswerList = Answer.LoadQuestions(1, 1); 
        init();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.R))
        {
            VoiceManage vm = new VoiceManage();
            vm.VoiceDistinguish();
        }

        if (canPlay) //语音合成完毕并生成音频后播放
        {
            canPlay = false;
            FlowManage.waveOutDevice = new WaveOutEvent();
            //waveOutDevice.PlaybackStopped += waveOutDevice_PlaybackStopped; 
            FlowManage.audioFileReader = new AudioFileReader(voice_path + "/" + FlowManage.voicename + ".wav");
            FlowManage.waveOutDevice.Init(FlowManage.audioFileReader);
            FlowManage.waveOutDevice.Play();
            FlowManage.PlayModeAnimation();
            if (curMode == 1 && UserStartAnswer) 
            {
                FlowManage.ShowQuestionInfo();
            }
        }
        if (FlowManage.waveOutDevice != null)//音频播放完毕后开始答题
        {
            if (UserStartAnswer)//用户开始回答问题
            {
                if (FlowManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    FlowManage.animName = AnimationControl.GetAnimationClipName(CharacterAction.Looking);
                    FlowManage.PlayModeAnimation();
                    UserStartAnswer = false;
                    Debug.Log("开始答题");
                    isAnswer = true;
                    if (FlowManage.waveOutDevice != null)
                    {
                        FlowManage.waveOutDevice.Dispose();
                        FlowManage.waveOutDevice = null;
                    }
                    if (FlowManage.audioFileReader != null)
                    {
                        FlowManage.audioFileReader.Close();
                        FlowManage.audioFileReader = null;
                    }
                    singleNar.StartRec();
                }
            }
            if (AnswerAnalysis) //沙勿略问我模式答题后给出答案解析
            {
                if (FlowManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (FlowManage.waveOutDevice != null)
                    {
                        FlowManage.waveOutDevice.Dispose();
                        FlowManage.waveOutDevice = null;
                    }
                    if (FlowManage.audioFileReader != null)
                    {
                        FlowManage.audioFileReader.Close();
                        FlowManage.audioFileReader = null;
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
                        curMode = 2;
                        FlowManage.PlayTransitVoice(2, "下面进入我问沙勿略环节。");                
                    }
                }
            }
            if (isTransit) //流程过渡播放声音
            {
                if (FlowManage.waveOutDevice != null)
                {
                    if (FlowManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                    {
                        FlowManage.animName = AnimationControl.GetAnimationClipName(CharacterAction.Looking);
                        FlowManage.PlayModeAnimation();
                        if (FlowManage.waveOutDevice != null)
                        {
                            FlowManage.waveOutDevice.Dispose();
                            FlowManage.waveOutDevice = null;
                        }
                        if (FlowManage.audioFileReader != null)
                        {
                            FlowManage.audioFileReader.Close();
                            FlowManage.audioFileReader = null;
                        }
                        isTransit = false;
                        Debug.Log("完成过渡");
                        flow_change = true;
                    }
                }
            }
            if (FinishedAnswer) //我问沙勿略环节回答完毕
            {
                if (FlowManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (FlowManage.waveOutDevice != null)
                    {
                        FlowManage.waveOutDevice.Dispose();
                        FlowManage.waveOutDevice = null;
                    }
                    if (FlowManage.audioFileReader != null)
                    {
                        FlowManage.audioFileReader.Close();
                        FlowManage.audioFileReader = null;
                    }
                    FinishedAnswer = false;
                    FlowManage.canDistinguish = true;
                }
            }
            if (isFinished) //我问沙勿略环节结束
            {
                if (FlowManage.waveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (FlowManage.waveOutDevice != null)
                    {
                        FlowManage.waveOutDevice.Dispose();
                        FlowManage.waveOutDevice = null;
                    }
                    if (FlowManage.audioFileReader != null)
                    {
                        FlowManage.audioFileReader.Close();
                        FlowManage.audioFileReader = null;
                    }
                    isFinished = false;
                    FlowManage.canDistinguish = true;
                    ProjectStart = false;
                    init();
                }
            }
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            //StartCoroutine(EnterM2MMode());
            if (!ProjectStart)
            {
                ProjectStart = true;
                curMode = 1;
                FlowManage.PlayTransitVoice(1, "下面进入沙勿略问我环节。");
            }
            //FlowManage.M2PMode(1);
        }
        if (isAnswer)
        {
            answer_time += Time.deltaTime;
            if (answer_time >= 10)
            {
                isAnswer = false;
                answer_time = 0f;
                FlowManage.StopAnswer(singleNar);
            }
        }
        if (flow_change) 
        {
            flow_change = false;
            if (curMode == 1) 
            {
                FlowManage.M2PMode(1);
            }
            else if (curMode == 2) 
            {      
                //AskMode = true;
                u.HideM2PAnswerPanel();
                u.ShowP2MAskPanel();
                FlowManage.canDistinguish = true;
                VoiceManage vm = new VoiceManage();
                vm.VoiceDistinguish();
                //nar.StartRec();
            }
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
	}

    /// <summary>
    /// 系统初始化
    /// </summary>
    public void init()
    {
        curMode = 0;
        isPlayed = false;
        answer_time = 0f;
        UserStartAnswer = false;
        isAnswer = false;
        AnswerAnalysis = false;
        AskMode = false;
        flow_change = false;
        FinishedAnswer = false;
        canPlay = false;
        wait_time = 0f;
        isFinished = false;
        isTransit = false;
        once_ask_time = 0f;
        voice_path = Application.dataPath + "/Resources/Voice";
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
        u.M2P_Answer_Panel.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<UILabel>().text = "";
        u.M2P_Answer_Panel.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<UILabel>().text = "";
        u.M2P_Answer_Panel.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "";
        u.M2P_Answer_Panel.transform.GetChild(1).transform.GetChild(2).gameObject.GetComponent<UILabel>().text = "";
        u.M2P_Answer_Panel.transform.GetChild(2).gameObject.GetComponent<UILabel>().text = "";
        u.P2M_Ask_Panel.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        u.P2M_Ask_Panel.transform.GetChild(0).transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "";
        u.P2M_Ask_Panel.transform.GetChild(1).transform.GetChild(1).gameObject.SetActive(false);
        u.P2M_Ask_Panel.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "";
        CharacterModel.GetComponent<Animation>().Stop();
        FlowManage.EnterStandBy(CharacterModel);
    }

    void OnApplicationQuit()
    {
        VoiceManage.MSCLogout();
        if (nar.waveSource != null)
        {
            nar.StopRec();
        }
        if (singleNar.waveSource != null) 
        {
            singleNar.StopRec();
        }
        if (FlowManage.waveOutDevice != null)
        {
            FlowManage.waveOutDevice.Stop();
        }
        if (FlowManage.waveOutDevice != null)
        {
            FlowManage.waveOutDevice.Dispose();
            FlowManage.waveOutDevice = null;
        }
    }
}
