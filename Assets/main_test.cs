using UnityEngine;
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

    public bool AskMode = false;

    MicManage mic;

    NAudioRecorder nar = new NAudioRecorder();

	// Use this for initialization
	void Start () {
        mic = new MicManage(GetComponent<AudioSource>());
        init();
	}
	
	// Update is called once per frame
	void Update () {
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
            FlowManage.M2PMode(1);
        }
        if (isAnswer)
        {
            answer_time += Time.deltaTime;
            if (answer_time >= 10)
            {
                int questionNo = FlowManage.curNo;
                questionNo++;
                if (questionNo <= 3)
                {
                    isAnswer = false;
                    answer_time = 0f;
                    FlowManage.StopAnswer();
                    FlowManage.M2PMode(questionNo);
                }
                else
                {
                    answer_time = 0f;
                    isAnswer = false;
                    FlowManage.StopAnswer();
                    FlowManage.P2MMode();
                }
            }
        }
        
        if (AskMode) 
        {

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
        if (u.P2M_Ask_Panel == null) 
        {
            Debug.Log("加载我问沙勿略界面失败");
            return;
        }
        u.HideM2PAnswerPanel();
        u.HideP2MAskPanel();
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
