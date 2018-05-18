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

        public static string animName = "";

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
            animName = AnimationControl.GetAnimationClipName(CharacterAction.Introducing);
            PlayModeAnimation();
            AskQuestion aq = new AskQuestion();
            tempAnswer = aq.GetQuestions();
        }

        public static void PlayModeAnimation() 
        {
            if (characterAnimation.GetClip(animName) == null)
            {
                Debug.Log("名为" + animName + "的动画片段不存在于人物模型中");
                return;
            }
            else
            {
                Debug.Log("正在播放" + animName + "动画");
                characterAnimation.wrapMode = WrapMode.PingPong;
                characterAnimation.Play(animName);
            }
        }

        /// <summary>
        /// 沙勿略问我模式
        /// </summary>
        /// <param name="no">题号</param>
        public static void M2PMode(int no) 
        {
            animName = AnimationControl.GetAnimationClipName(CharacterAction.Asking);
            u.ShowM2PAnswerPanel();
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
                string answer_result = KeywordMatch.GetAnswerByKeywordMatch(VoiceManage.ask_rec_result, mt.BeforeAskList);//AIUI.HttpPost(AIUI.TEXT_SEMANTIC_API, "{\"userid\":\"test001\",\"scene\":\"main\"}", "text=" + Utils.Encode(VoiceManage.ask_rec_result));
                if (answer_result == string.Empty) {
                    answer_result = KeywordMatch.GetAnswerByKeywordMatch(VoiceManage.ask_rec_result, mt.AfterAskList);
                }
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


        /// <summary>
        /// 流程过渡
        /// </summary>
        /// <param name="Mode"></param>
        /// <param name="txt"></param>
        public static void PlayTransitVoice(int Mode,string txt) 
        {
            animName = AnimationControl.GetAnimationClipName(CharacterAction.Looking);
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
                        animName = AnimationControl.GetAnimationClipName(CharacterAction.Right);
                        content = "恭喜你，回答正确";
                        voicename = "correct";
                        u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答正确";
                    }
                    else
                    {
                        if (KeywordMatch.GetResultByKeywordMatch(HayStack, mt.AnswerList))
                        {
                            animName = AnimationControl.GetAnimationClipName(CharacterAction.Right);
                            content = "恭喜你，回答正确";
                            voicename = "correct";
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答正确";
                        }
                        else
                        {
                            animName = AnimationControl.GetAnimationClipName(CharacterAction.Wrong);
                            content = "很遗憾，回答错误，正确答案是" + Needle;
                            voicename = "wrong";
                            u.M2P_Answer_Panel.transform.GetChild(5).gameObject.GetComponent<UILabel>().text = "回答错误";
                        }
                    }
                }
                else
                {
                    animName = AnimationControl.GetAnimationClipName(CharacterAction.Thinking);
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
