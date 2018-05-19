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

    public GameObject CharacterModel;  //人物模型对象

    public int ret = 0;  //  调用MSC接口返回值

    public float answer_time = 0f;  //沙勿略问我答题时间控制变量（10秒答题时间）

    public bool UserStartAnswer = false;  //开始答题（当为true时用户可开始答题）

    public bool isAnswer = false;   // 是否正在答题（当为true时开始答题计时）

    public bool AnswerAnalysis = false;  //  答完题后分析答案（当为true时分析完毕）

    public bool flow_change = false;   // 流程切换标识（当为true时切换流程）

    public bool FinishedAnswer = false;   // 完成回答标识（我问沙勿略环节，机器给出答案，当为true时标志完成）

    public bool canPlay = false;  // 合成语音音频可播放标识（当为true时标志语音合成完成，并播放音频）

    public bool isFinished = false; // 我问沙勿略环节结束标识（当为true时整个流程结束，回到初始状态）

    public bool isTransit = false;  // 环节间过渡标识（当为true时完成过渡）

    public bool successDistinguish = false;  //成功识别标识（沙勿略问我环节，用户回答完进行识别，当为true时标识识别完成，并在界面给出相应结果）

    private int curMode = 0;  // 当前环节变量（当为0时为待机状态，当为1时为沙勿略问我环节，当为2时为我问沙勿略环节）

    NAudioRecorder nar = new NAudioRecorder();

    SingleNAudioRecorder singleNar = new SingleNAudioRecorder();

    private static UIObject u;

    public string voice_path = ""; //音频存放目录

    public List<Answer> BeforeAskList = new List<Answer>();   // 我问沙勿略前半生题库

    public List<Answer> AfterAskList = new List<Answer>();    // 我问沙勿略后半生题库

    public List<Answer> AnswerList = new List<Answer>();      // 沙勿略问我题库

    public M1101Ctrl mc = null;

    public bool FlowStart = false;   //流程开始标识

	// Use this for initialization
	void Start () {
        if (VoiceManage.MSCLogin() != (int)ErrorCode.MSP_SUCCESS)
        { Debug.Log("登陆失败!" + ret); MSC.MSPLogout(); return; }
        mc = new M1101Ctrl();
        mc.mResultAction = FlowStartAction;
        u = Camera.main.GetComponent<UIObject>();
        BeforeAskList = Answer.LoadQuestions(2, 1);
        AfterAskList = Answer.LoadQuestions(2, 2);
        AnswerList = Answer.LoadQuestions(1, 1); 
        init();
	}

    void FlowStartAction(bool a) 
    {
        FlowStart = a;
    }
	
	// Update is called once per frame
	void Update () {

        if (FlowStart) //监听流程是否开始
        {
            FlowStart = false;
            curMode = 1;
            FlowManage.PlayTransitVoice(1, "下面进入沙勿略问我环节。");
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
                    FlowStart = false;
                    init();
                }
            }
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            if (!FlowStart)
            {
                FlowStart = true;
                curMode = 1;
                FlowManage.PlayTransitVoice(1, "下面进入沙勿略问我环节。");
            }
        }

        if (isAnswer) //答题计时
        {
            answer_time += Time.deltaTime;
            if (answer_time >= 10)
            {
                isAnswer = false;
                answer_time = 0f;
                singleNar.StopRec();
            }
        }
        if (curMode == 1&&successDistinguish) 
        {
            FlowManage.StopAnswer();
            successDistinguish = false;
        }
        if (flow_change) //切换流程
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
	}

    /// <summary>
    /// 系统初始化
    /// </summary>
    public void init()
    {
        curMode = 0;
        answer_time = 0f;
        UserStartAnswer = false;
        isAnswer = false;
        AnswerAnalysis = false;
        flow_change = false;
        FinishedAnswer = false;
        canPlay = false;
        isFinished = false;
        isTransit = false;
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
        u.M2P_Answer_Panel.transform.GetChild(3).gameObject.GetComponent<UILabel>().text = "";
        u.M2P_Answer_Panel.transform.GetChild(2).gameObject.GetComponent<UILabel>().text = "";
        u.P2M_Ask_Panel.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        u.P2M_Ask_Panel.transform.GetChild(0).transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "";
        u.P2M_Ask_Panel.transform.GetChild(1).transform.GetChild(1).gameObject.SetActive(false);
        u.P2M_Ask_Panel.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "";
        CharacterModel.GetComponent<Animation>().Stop();
        FlowManage.EnterStandBy(CharacterModel);
    }


    /// <summary>
    /// 程序退出
    /// </summary>
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
