﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using LitJson;

namespace Assets.Scripts
{
    class AIUI
    {
        public static string AIUI_BASE_URL = "http://api.xfyun.cn/";
        public static string TEXT_SEMANTIC_API = "v1/aiui/v1/text_semantic";//文本语义接口

        /// <summary>
        /// request头部参数
        /// </summary>
        private static string APPID = "5accf546";
        private static string APIKey = "879c98332be0467e81b31fdf5b861faf";
        private static string CurTime;
        private static string Param;
        private static string CheckSum;

        public string Answer = string.Empty;

        public static void HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            Debug.Log(retString);
        }
        public static AnswerResult HttpPost(string url,string url_params)
        {
            AnswerResult ar = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AIUI_BASE_URL+url);
            CurTime = Utils.CurrentTimeMillis();
            Param = Utils.Encode("{\"userid\":\"test001\",\"scene\":\"main\"}");
            CheckSum = Utils.ToMD5(string.Format("{0}{1}{2}{3}", APIKey, CurTime, Param, url_params));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            request.Headers.Add("X-Appid", APPID);
            request.Headers.Add("X-CurTime",CurTime);
            request.Headers.Add("X-Param", Param);
            request.Headers.Add("X-CheckSum", CheckSum);

            using (StreamWriter dataStream = new StreamWriter(request.GetRequestStream()))
            {
                dataStream.Write(url_params);
                dataStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码    
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            if (retString != "")
            {
                ar = JsonMapper.ToObject<AnswerResult>(retString);
                if (ar.data.answer != null)
                {
                    Debug.Log(ar.data.answer.text);
                }
                else 
                {
                    Debug.Log("抱歉我还不知道这道问题的答案");
                }
            }
            return ar;
        }

        
    }
}