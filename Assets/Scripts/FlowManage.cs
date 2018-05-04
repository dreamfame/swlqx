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
            //u.ShowM2PAnswerPanel();
            curNo = no;
            if (tempAnswer == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else//机器读出并在界面显示问题内容
            {
                u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.SetActive(true);
                u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.GetComponent<UILabel>().text = curNo + "." + tempAnswer[no - 1].title;
                u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).GetComponent<UILabel>().text = "A." + tempAnswer[no - 1].answerA;
                u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).GetComponent<UILabel>().text = "B." + tempAnswer[no - 1].answerB;
                u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).GetComponent<UILabel>().text = "C." + tempAnswer[no - 1].answerC;
                VoiceManage vm = new VoiceManage();
                vm.PlayVoice(tempAnswer[no - 1].title, "subject" + no,Application.dataPath+"/Resources/Voice");
            }
        }

        /// <summary>
        /// 开始用户答题
        /// </summary>
        public static void StartUserAnswer() 
        {
            mt.UserStartAnswer = true;
        }

        /// <summary>
        /// 我问沙勿略模式
        /// </summary>
        public static void P2MMode(NAudioRecorder n) 
        {
            //u.HideM2PAnswerPanel();
            //u.ShowP2MAskPanel();
            Debug.Log("进入我问沙勿略模式");
            string result = n.StopRec();
            Debug.Log(string.Format("-->语音信息:{0}", result));
            if (result == string.Empty || result == null)
            {
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.SetActive(true);
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.GetComponent<UILabel>().text = "对不起，我没有听清您说的话！可以再说一次吗？";
                //mt.isFinished = true;
                mt.flow_change = true;
            }
            else
            {
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.SetActive(true);
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.GetComponent<UILabel>().text = result;
                mt.flow_change = true;
            }
            Debug.Log("小沙正在思考中...");
            result = AIUI.HttpPost(AIUI.TEXT_SEMANTIC_API, "{\"userid\":\"test001\",\"scene\":\"main\"}", "text=" + Utils.Encode(result));
            u.P2M_Ask_Panel.transform.GetChild(6).gameObject.SetActive(true);
            u.P2M_Ask_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = result;
            Debug.Log(string.Format("-->小沙回答:{0}", result));
            //结束界面
            //进入唤醒状态
        }

        /// <summary>
        /// 停止回答（沙勿略问我）
        /// </summary>
        public static void StopAnswer(NAudioRecorder nar) 
        {
            if (nar.waveSource != null)
            {
                VoiceManage vm = new VoiceManage();
                string retString = nar.StopRec();
                if (retString != "")
                {
                    string HayStack = retString;
                    Regex r = new Regex(@"[a-zA-Z]+");
                    Match m = r.Match(HayStack);
                    string answerStr = m.Value.ToUpper().Trim();
                    Debug.Log("回答的是：" + answerStr);
                    string Needle = tempAnswer[curNo - 1].CorrectAnswer;
                    if (answerStr.Equals(Needle) || Needle.Contains(answerStr))
                    {
                        Debug.Log("回答正确");
                        u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答正确";
                        if (Needle == "A") 
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo-1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else if (Needle == "B") 
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else if (Needle == "C") 
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else if (Needle == "A/B/C") 
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        Debug.Log("回答错误");
                        u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答错误";
                        if (Needle == "A")
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                        }
                        else if (Needle == "B")
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                        }
                        else if (Needle == "C")
                        {
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(false);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(curNo - 1).gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        /*IAnalyser analyser = new SimHashAnalyser();
                        var likeness = analyser.GetLikenessValue(Needle, HayStack);
                        Debug.Log("相似度为：" + likeness * 100);
                        if ((likeness * 100) > 50)
                        {
                            Debug.Log("回答正确");
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答正确";
                        }
                        else
                        {
                            Debug.Log("回答错误");
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.SetActive(true);
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答错误";
                        }*/
                    }
                }
                else
                {
                    Debug.Log("抱歉,您说了什么，我没有听清");
                    u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "抱歉,您说了什么，我没有听清";
                }
            }
        }
	}
}
