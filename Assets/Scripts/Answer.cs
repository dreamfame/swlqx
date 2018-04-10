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

    public string CorrectAnswer { get; set; } // 正确答案

    public string Analyis { get; set; } //答案解析

    /// <summary>
    /// 加载xml文件内的题目信息
    /// </summary>
    /// <param name="type">题目类型</param>
    /// <returns>返回当前题目类型中的所有题目信息，为Answer对象集合</returns>
    public static List<Answer> LoadQuestions(int category,int type)
    {
        string path = "";
        switch (category) 
        {
            case 1:
                path = "Question/questions1.xml";
                break;
            case 2:
                path = "Question/questions2.xml";
                break;
        }
        string t = "";
        switch (type)
        {
            case 1:
                t = "前半生";
                break;
            case 2:
                t = "后半生";
                break;
        }
        List<Answer> listAnswer = new List<Answer>();
        questionXml = new XmlDocument();
        XmlUtil xmlUtil = new XmlUtil(path, true);
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
                a.Analyis = xl1.GetAttribute("解析");
                listAnswer.Add(a);
            }
        }
        return listAnswer;
    }
}