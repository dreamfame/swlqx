using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public enum CharacterAction  //人物动作枚举类
    {
        None,
        Introducing,
        Thinking,
        Asking,
        Wrong,
        Right,
        Looking,
        All,
    }


    /// <summary>
    /// 动画控制类
    /// </summary>
    public class AnimationControl
    {
        public AnimationClip ac1;//沙勿略生平传奇动画
        public AnimationClip ac2;//欢迎和自我介绍动画
        public AnimationClip ac3;//祈祷指导动画
        public AnimationClip as4;//结束动画

        
        /// <summary>
        /// 获取动画片段名称
        /// </summary>
        /// <param name="ca">动画片段的枚举类型</param>
        /// <returns>动画片段名称</returns>
        public static string GetAnimationClipName(CharacterAction ca)
        {
            String ActionName = "";
            switch (ca) 
            {
                case CharacterAction.None:
                    ActionName = "QUANBUDONGZUO";
                    break;
                case CharacterAction.Introducing:
                    ActionName = "ziwojiesao";
                    break;
                case CharacterAction.Asking:
                    ActionName = "tiwen";
                    break;
                case CharacterAction.Wrong:
                    ActionName = "huidacuowu";
                    break;
                case CharacterAction.Right:
                    ActionName = "HUIDACUOWU";
                    break;
                case CharacterAction.Looking:
                    ActionName = "KANSHU";
                    break;
                case CharacterAction.Thinking:
                    ActionName = "sikao";
                    break;
                default:
                    ActionName = "";
                    break;
            }
            return ActionName;
        }

    }
}
 