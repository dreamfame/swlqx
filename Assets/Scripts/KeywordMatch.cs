using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
	public class KeywordMatch
	{
        private static bool containK1 = false;

        private static bool containK2 = false;

        private static bool containK3 = false;

        private static bool containK4 = false;

        private static int HitTimes = 0;

        private static Dictionary<Answer,int> answerHitList = new Dictionary<Answer,int>();

        public static string GetAnswerByKeywordMatch(string speechStr,List<Answer> QuestionLibrary) 
        {
            string MatchedAnswer = "";
            foreach (var a in QuestionLibrary) 
            {
                if (a.keyword1[0] != "")
                {
                    foreach (var s1 in a.keyword1)
                    {
                        if (speechStr.Contains(s1))
                        {
                            containK1 = true;
                            HitTimes++;
                        }
                    }
                }
                else 
                {
                    break;
                }
                if (containK1) 
                {
                    if (a.keyword2[0] != "")
                    {
                        foreach (var s2 in a.keyword2)
                        {
                            if (speechStr.Contains(s2))
                            {
                                containK2 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else 
                    {
                        containK2 = true;
                    }
                }
                if (containK2)
                {
                    if (a.keyword3[0] != "")
                    {
                        foreach (var s3 in a.keyword3)
                        {
                            if (speechStr.Contains(s3))
                            {
                                containK3 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else 
                    {
                        containK3 = true;
                    }
                }
                if (containK3)
                {
                    if (a.keyword4[0] != "")
                    {
                        foreach (var s4 in a.keyword4)
                        {
                            if (speechStr.Contains(s4))
                            {
                                containK4 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else 
                    {
                        containK4 = true;
                    }
                }
                if (containK1 && containK2 && containK3 && containK4) 
                {
                    answerHitList.Add(a,HitTimes);
                    containK1 = containK2 = containK3 = containK4 = false;
                    HitTimes = 0;
                }
            }
            if (answerHitList.Count > 0)
            {
                var MaxHitAnswer = answerHitList.First(a => a.Value == answerHitList.Values.Max());
                MatchedAnswer = MaxHitAnswer.Key.CorrectAnswer;
            }
            if (MatchedAnswer == "") 
            {
                MatchedAnswer = "抱歉，这个问题我还不知道，问答结束！";
            }
            return MatchedAnswer;
        }

        public static bool GetResultByKeywordMatch(string speechStr,List<Answer> AnswerLibrary)
        {
            bool isRight = false;
            foreach (var a in AnswerLibrary)
            {
                if (a.keyword1[0] != "")
                {
                    foreach (var s1 in a.keyword1)
                    {
                        if (speechStr.Contains(s1))
                        {
                            containK1 = true;
                            HitTimes++;
                        }
                    }
                }
                else
                {
                    break;
                }
                if (containK1)
                {
                    if (a.keyword2[0] != "")
                    {
                        foreach (var s2 in a.keyword2)
                        {
                            if (speechStr.Contains(s2))
                            {
                                containK2 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else
                    {
                        containK2 = true;
                    }
                }
                if (containK2)
                {
                    if (a.keyword3[0] != "")
                    {
                        foreach (var s3 in a.keyword3)
                        {
                            if (speechStr.Contains(s3))
                            {
                                containK3 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else
                    {
                        containK3 = true;
                    }
                }
                if (containK3)
                {
                    if (a.keyword4[0] != "")
                    {
                        foreach (var s4 in a.keyword4)
                        {
                            if (speechStr.Contains(s4))
                            {
                                containK4 = true;
                                HitTimes++;
                            }
                        }
                    }
                    else
                    {
                        containK4 = true;
                    }
                }
                if (containK1 && containK2 && containK3 && containK4)
                {
                    isRight = true;
                    containK1 = containK2 = containK3 = containK4 = false;
                    HitTimes = 0;
                }
            }
            return isRight;
        }
    }
}
