using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LitJson;
using Assets.Scripts.SimHash;
using System.Text.RegularExpressions;
using Assets.UI;
using System.Xml;
using System.Threading;
using NAudio.Wave;

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

        private static VoiceManage vm = new VoiceManage();

        public static IWavePlayer waveOutDevice;

        public static AudioFileReader audioFileReader;

        public static bool canDistinguish = true;

        public static string content = "";
        public static string voicename = "";

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
                //vm.PlayVoice(tempAnswer[curNo - 1].title, "subject" + curNo, "Assets/Resources/Voice");
                content = "第"+curNo+"题."+tempAnswer[curNo - 1].title;
                voicename = "subject" + curNo;
                Thread thread_question = new Thread(new ThreadStart(playVoice));
                thread_question.IsBackground = true;
                thread_question.Start();
                FlowManage.StartUserAnswer();
            }
        }

        static object obj = new object();

        static void playVoice() 
        {
            lock (obj)
            {
                Debug.Log("内容是：" + content + "=====文件名是：" + voicename);
                if (vm.PlayVoice(content, voicename, mt.voice_path) == SynthStatus.MSP_TTS_FLAG_DATA_END)
                {
                    canDistinguish = true;
                    mt.canPlay = true;
                }
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
            canDistinguish = false;
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
            //u.HideM2PAnswerPanel();
            //u.ShowP2MAskPanel();
            Debug.Log("进入我问沙勿略模式");
            //string result = n.StopRec();
            //Debug.Log(string.Format("-->语音信息:{0}", result));
            if (VoiceManage.ask_rec_result == string.Empty || VoiceManage.ask_rec_result == null)
            {
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.SetActive(true);
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.GetComponent<UILabel>().text = "对不起，我没有听清您说的话！可以再说一次吗？";
                content = "对不起，我没有听清您说的话！可以再说一次吗？";
                voicename = "answer";
                mt.FinishedAnswer = true;
                //mt.isFinished = true;
            }
            else
            {
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.SetActive(true);
                u.P2M_Ask_Panel.transform.GetChild(4).gameObject.GetComponent<UILabel>().text = VoiceManage.ask_rec_result;
                Debug.Log("小沙正在思考中...");
                string answer_result = AIUI.HttpPost(AIUI.TEXT_SEMANTIC_API, "{\"userid\":\"test001\",\"scene\":\"main\"}", "text=" + Utils.Encode(VoiceManage.ask_rec_result));
                u.P2M_Ask_Panel.transform.GetChild(6).gameObject.SetActive(true);
                u.P2M_Ask_Panel.transform.GetChild(6).gameObject.GetComponent<UILabel>().text = answer_result;
                content = answer_result;
                voicename = "answer";
                Debug.Log("答案：" + answer_result);
                mt.FinishedAnswer = true;
                if (answer_result.Equals("抱歉，这个问题我还不知道，问答结束！")) 
                {
                    DateTime dateTime = new DateTime();
                    dateTime = DateTime.Now;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(Application.dataPath + "/Resources/Question/record.xml");
                    XmlNode root = xmlDoc.SelectSingleNode("root");
                    XmlElement xe1 = xmlDoc.CreateElement("问题");
                    xe1.SetAttribute("内容", VoiceManage.ask_rec_result);
                    xe1.SetAttribute("时间", dateTime.ToString());
                    root.AppendChild(xe1);
                    xmlDoc.Save(Application.dataPath + "/Resources/Question/record.xml");
                    mt.FinishedAnswer = false;
                    VoiceManage.StopSpeech();
                }
                Debug.Log(string.Format("-->小沙回答:{0}", VoiceManage.ask_rec_result));
            }
            Thread thread_answer = new Thread(new ThreadStart(playVoice));
            thread_answer.IsBackground = true;
            thread_answer.Start();
            //结束界面
            //进入唤醒状态
        }

        public static void PlayTransitVoice(int Mode,string txt) 
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
            if (Mode == 1) //播放进入沙勿略问我模式语音
            {
                content = txt;
                voicename = "transit1";
            }
            else if (Mode == 2) //播放进入我问沙勿略模式语音
            {
                content = txt;
                voicename = "transit2";
            }
            Thread thread_transit = new Thread(new ThreadStart(playVoice));
            thread_transit.IsBackground = true;
            thread_transit.Start();
            mt.isTransit = true;
        }

        /// <summary>
        /// 停止回答（沙勿略问我）
        /// </summary>
        public static void StopAnswer(SingleNAudioRecorder nar) 
        {
            if (nar.waveSource != null)
            {
                string retString = nar.StopRec();
                Debug.Log("回答的是："+retString);
                mt.AnswerAnalysis = true;
                if (retString != "")
                {
                    string HayStack = retString;
                    Regex r = new Regex(@"[a-zA-Z]+");
                    Match m = r.Match(HayStack);
                    string answerStr = m.Value.ToUpper().Trim();
                    string Needle = tempAnswer[curNo - 1].CorrectAnswer;
                    if (answerStr.Equals(Needle) || Needle.Contains(answerStr)&&answerStr!="")
                    {
                        content = "恭喜你，回答正确";
                        voicename = "correct";
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
                        content = "很遗憾，回答错误，正确答案是" + Needle;
                        voicename = "wrong";
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
                    content = "抱歉,您说了什么，我没有听清";
                    voicename = "sorry"; 
                    u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "抱歉,您说了什么，我没有听清";
                }
                Thread thread_analysis = new Thread(new ThreadStart(playVoice));
                thread_analysis.IsBackground = true;
                thread_analysis.Start();
            }
        }
	}
}
