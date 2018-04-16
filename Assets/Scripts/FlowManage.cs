using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LitJson;
using Assets.Scripts.SimHash;

namespace Assets.Scripts
{

    /// <summary>
    /// 流程管理类，管理整个项目流程。
    /// </summary>
	public class FlowManage
	{
        private static UIObject u = Camera.main.GetComponent<UIObject>();

        private static GameObject characterModel;//神父人物模型对象

        private static Animation characterAnimation;//神父人物模型动画对象

        public static NAudioRecorder nar = new NAudioRecorder();

        public static int curNo;

        private static List<Answer> tempAnswer = new List<Answer>();

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
                characterAnimation.Play(name);
                //VoiceManage.VoiceWakeUp();//调用语音唤醒接口
            }
        }

        /// <summary>
        /// 沙勿略问我模式
        /// </summary>
        /// <param name="no">题号</param>
        public static void M2PMode(int no) 
        {
            u.ShowM2PAnswerPanel();
            curNo = no;
            AskQuestion aq = new AskQuestion();
            tempAnswer = aq.GetQuestions();
            if (tempAnswer == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else//机器读出并在界面显示问题内容
            {
                Debug.Log(tempAnswer[no - 1].title);
                VoiceManage vm = new VoiceManage();
                vm.PlayVoice(tempAnswer[no - 1].title, "subject" + no,Application.dataPath+"/Resources/Voice");
                u.M2P_Answer_Panel.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = tempAnswer[no - 1].title;
                Camera.main.GetComponent<main_test>().isAnswer = true;
                nar.StartRec();
            }
        }

        /// <summary>
        /// 我问沙勿略模式
        /// </summary>
        public static void P2MMode() 
        {
            Debug.Log("进入我问沙勿略模式");
        }

        /// <summary>
        /// 停止回答（沙勿略问我）
        /// </summary>
        public static void StopAnswer() 
        {
            if (nar.waveSource != null)
            {
                Debug.Log("停止录音");
                string retString = nar.StopRec();
                if (retString != "")
                {
                    AnswerResult ar = JsonMapper.ToObject<AnswerResult>(retString);
                    if (ar == null || ar.data == null)
                    {
                        Debug.Log("抱歉，您刚才说了什么，我没有听清");
                    }
                    else
                    {
                        if (ar.data.answer != null)
                        {
                            Debug.Log(ar.data.answer.text);
                            string HayStack = "德哈所";
                            string Needle = tempAnswer[curNo - 1].CorrectAnswer;
                            IAnalyser analyser = new SimHashAnalyser();
                            var likeness = analyser.GetLikenessValue(Needle, HayStack);
                            Debug.Log("相似度为：" + likeness*100);
                            if ((likeness * 100) > 50)
                            {
                                Debug.Log("回答正确");
                            }
                            else 
                            {
                                Debug.Log("回答错误");
                            }
                        }
                        else
                        {
                            Debug.Log("抱歉我还不知道这道问题的答案");
                        }
                    }
                }
            }
        }
	}
}
