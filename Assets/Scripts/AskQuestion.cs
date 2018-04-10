using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AskQuestion{

    //用来取三个问题的标识
    public int flag = 0;

    public List<int> TagList = new List<int>();

    /// <summary>
    /// 获取问题集合下标（随机，不重复）
    /// </summary>
    public List<Answer> GetQuestions() 
    {
        TagList.Clear();
        var listAnswers = Answer.LoadQuestions(1,1);
        if (listAnswers.Count == 0) return null;
        System.Random random = new System.Random();
        while (TagList.Distinct().ToList().Count < 3) 
        {
            int index = random.Next(0, listAnswers.Count-1);
            TagList.Add(index);
            flag++;
        }
        return GetQuestionsByNo(listAnswers,TagList.Distinct().ToList());
    }

    /// <summary>
    /// 通过下标获取三个问题
    /// </summary>
    /// <returns></returns>
    public List<Answer> GetQuestionsByNo(List<Answer> answers,List<int> nolist) 
    {
        List<Answer> newAnswers = new List<Answer>();
        if (nolist.Count == 0) return null;
        foreach (var t in nolist) 
        {
            newAnswers.Add(answers[t]);
        }
        return newAnswers;
    }
}
