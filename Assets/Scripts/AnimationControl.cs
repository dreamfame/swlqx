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
        Walk,
        Run,
        Talk,
        Jump,
        Hand,
        Leg,
        Foot,
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
                    ActionName = "Take 001";
                    break;
                case CharacterAction.Walk:
                    ActionName = "walk";
                    break;
                case CharacterAction.Run:
                    ActionName = "run";
                    break;
                case CharacterAction.Talk:
                    ActionName = "talk";
                    break;
                case CharacterAction.Jump:
                    ActionName = "jump";
                    break;
                case CharacterAction.Hand:
                    ActionName = "hand";
                    break;
                case CharacterAction.Foot:
                    ActionName = "foot";
                    break;
                case CharacterAction.Leg:
                    ActionName = "leg";
                    break;
                default:
                    ActionName = "";
                    break;
            }
            return ActionName;
        }

    }
}
 