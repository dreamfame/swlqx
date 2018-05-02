using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LitJson;
using Assets.Scripts.SimHash;
using System.Text.RegularExpressions;

namespace Assets.Scripts
{

    /// <summary>
    /// 流程管理类，管理整个项目流程。
    /// </summary>
	public class FlowManage:MonoBehaviour
	{
        private static UIObject u = Camera.main.GetComponent<UIObject>();

        private static GameObject characterModel;//神父人物模型对象

        private static Animation characterAnimation;//神父人物模型动画对象

        public static NAudioRecorder nar = new NAudioRecorder();

        public static int curNo;

        private static List<Answer> tempAnswer = new List<Answer>();

        private static main_test mt = Camera.main.GetComponent<main_test>();

        /// <summary>
        /// 进入待机状态
        /// </summary>
        public static void EnterStandBy(GameObject go)
        {
            characterModel = go;
            characterAnimation = go.GetComponent<Animation>();
            String name = AnimationControl.GetAnimationClipName(CharacterAction.None);
            if (characterAnimation.GetClip(name) == null)
            {
                Debug.Log("名为" + name + "的动画片段不存在于人物模型中");
                return;
            }
            else
            {
                characterAnimation.wrapMode = WrapMode.PingPong;
                characterAnimation.Play(name);
                AskQuestion aq = new AskQuestion();
                tempAnswer = aq.GetQuestions();
                //VoiceManage.VoiceWakeUp();//调用语音唤醒接口
            }
        }

        /// <summary>
        /// 沙勿略问我模式
        /// </summary>
        /// <param name="no">题号</param>
        public static void M2PMode(int no) 
        {
            u.M2P_Answer_Panel.transform.GetChild(6).gameObject.SetActive(false);
            u.ShowM2PAnswerPanel();
            curNo = no;
            if (tempAnswer == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else//机器读出并在界面显示问题内容
            {
                VoiceManage vm = new VoiceManage();
                vm.PlayVoice(tempAnswer[no - 1].title+"；请在二十秒内作出回答。", "subject" + no,Application.dataPath+"/Resources/Voice");
                u.M2P_Answer_Panel.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = curNo+"."+tempAnswer[no - 1].title;
                u.M2P_Answer_Panel.transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "A." + tempAnswer[no - 1].answerA;
                u.M2P_Answer_Panel.transform.GetChild(2).gameObject.GetComponent<UILabel>().text = "B." + tempAnswer[no - 1].answerB;
                u.M2P_Answer_Panel.transform.GetChild(3).gameObject.GetComponent<UILabel>().text = "C." + tempAnswer[no - 1].answerC;
            }
        }

        /// <summary>
        /// 开始用户答题
        /// </summary>
        public static void StartUserAnswer() 
        {
            
            mt.isAnswer = true;
            nar.StartRec();
        }

        /// <summary>
        /// 我问沙勿略模式
        /// </summary>
        public static void P2MMode() 
        {
            u.HideM2PAnswerPanel();
            u.ShowP2MAskPanel();
            Debug.Log("进入我问沙勿略模式");
            string result = VoiceManage.VoiceDistinguish();
            Debug.Log(string.Format("-->语音信息:{0}", result));
            Debug.Log("小沙正在思考中...");
            result = AIUI.HttpPost(AIUI.TEXT_SEMANTIC_API, "{\"userid\":\"test001\",\"scene\":\"main\"}", "text=" + Utils.Encode(result));
            Debug.Log(string.Format("-->小沙回答:{0}", result));
            //结束界面
            //进入唤醒状态
        }

        /// <summary>
        /// 停止回答（沙勿略问我）
        /// </summary>
        public static void StopAnswer() 
        {
            if (nar.waveSource != null)
            {
                VoiceManage vm = new VoiceManage();
                Debug.Log("停止录音");
                string retString = nar.StopRec();
                if (retString != "")
                {
                    string HayStack = retString;
                    Regex r = new Regex(@"[a-zA-Z]+");
                    Match m = r.Match(HayStack);
                    String answerStr = m.Value.ToUpper().Trim();
                    string Needle = tempAnswer[curNo - 1].CorrectAnswer;
                    if (answerStr.Equals(Needle) || Needle.Contains(answerStr))
                    {
                        Debug.Log("回答正确");
                        u.M2P_Answer_Panel.transform.GetChild(6).gameObject.SetActive(true);
                        u.M2P_Answer_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = "回答正确";
                    }
                    else
                    {
                        IAnalyser analyser = new SimHashAnalyser();
                        var likeness = analyser.GetLikenessValue(Needle, HayStack);
                        Debug.Log("相似度为：" + likeness * 100);
                        if ((likeness * 100) > 50)
                        {
                            Debug.Log("回答正确");
                            u.M2P_Answer_Panel.transform.GetChild(6).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = "回答正确";
                        }
                        else
                        {
                            Debug.Log("回答错误");
                            u.M2P_Answer_Panel.transform.GetChild(6).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = "回答错误";
                        }
                    }
                }
                else
                {
                    Debug.Log("抱歉,您说了什么，我没有听清");
                    u.M2P_Answer_Panel.transform.GetChild(6).gameObject.SetActive(true);
                    u.M2P_Answer_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = "抱歉,您说了什么，我没有听清";
                }
            }
        }
	}
}
