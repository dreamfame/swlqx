using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        public static int curNo;

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
            var temp = aq.GetQuestions();
            if (temp == null)
            {
                Debug.Log("题库读取数据失败..");
            }
            else//机器读出并在界面显示问题内容
            {
                Debug.Log(temp[no - 1].title);
                VoiceManage vm = new VoiceManage();
                vm.PlayVoice(temp[no - 1].title, "subject" + no,Application.dataPath+"/Resources/Voice");
                u.M2P_Answer_Panel.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = temp[no - 1].title;
                Camera.main.GetComponent<main_test>().isAnswer = true;
            }
        }

        /// <summary>
        /// 我问沙勿略模式
        /// </summary>
        public static void P2MMode() 
        {
            Debug.Log("进入我问沙勿略模式");
        }
	}
}
