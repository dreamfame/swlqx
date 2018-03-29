using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Assets.UI;

public class Answer{
    private static XmlDocument questionXml;

    public int Type { get; set; } //题目类型

    public int No { get; set; }  //题号

    public string title { get; set; }  //题目

    public string answerA { get; set; }  // 选项A

    public string answerB { get; set; }  // 选项B

    public string answerC { get; set; }  // 选项C

    public string CorrectAnswer { get; set; } // 正确答案

    /// <summary>
    /// 加载xml文件内的题目信息
    /// </summary>
    /// <param name="type">题目类型</param>
    /// <returns>返回当前题目类型中的所有题目信息，为Answer对象集合</returns>
    public static List<Answer> LoadQuestions(int type)
    {
        string t = "";
        switch (type)
        {
            case 1:
                t = "选择";
                break;
            case 2:
                t = "问答";
                break;
        }
        List<Answer> listAnswer = new List<Answer>();
        questionXml = new XmlDocument();
        XmlUtil xmlUtil = new XmlUtil("Question/questions.xml", true);
        questionXml = xmlUtil.getXmlDocument();
        XmlNodeList xmlNodeList = questionXml.SelectSingleNode("data").ChildNodes;
        foreach (XmlElement xl1 in xmlNodeList)
        {
            Answer a = new Answer();
            if (xl1.GetAttribute("类型").Equals(t))
            {
                a.Type = 1;
                a.No = int.Parse(xl1.GetAttribute("题号"));
                a.title = xl1.GetAttribute("题目");
                a.CorrectAnswer = xl1.GetAttribute("答案");
                if (xl1.HasChildNodes)
                {
                    foreach (XmlElement xl2 in xl1.ChildNodes)
                    {
                        if (xl2.GetAttribute("名称").Equals("A"))
                        {
                            a.answerA = xl2.GetAttribute("内容");
                        }
                        else if (xl2.GetAttribute("名称").Equals("B"))
                        {
                            a.answerB = xl2.GetAttribute("内容");
                        }
                        else if (xl2.GetAttribute("名称").Equals("C"))
                        {
                            a.answerC = xl2.GetAttribute("内容");
                        }
                    }
                }
                listAnswer.Add(a);
            }
        }
        return listAnswer;
    }
}
