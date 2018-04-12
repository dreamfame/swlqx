using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
	public class AnswerResult
	{
        public string code { get; set; } //结果码

        public string desc { get; set; } //描述

        public AnswerData data { get; set; } //返回数据

        public string sid { get; set; }
	}

    public class AnswerData 
    {
        public string man_intv { get; set; }

        public int rc { get; set; } // 应答码

        public AnswerObject answer { get; set; }

        public int no_nlu_result { get; set; }

        public string service { get; set; }

        public string text { get; set; }

        public string operation { get; set; }

        public string uuid { get; set; }

        public int status { get; set; }

        public string sid { get; set; }
    }

    public class AnswerObject 
    {
        public string topicID { get; set; }

        public string emotion { get; set; }

        public QuestionObject question { get; set; }

        public string answerType { get; set; }

        public string text { get; set; }

        public string type { get; set; }
    }

    public class QuestionObject 
    {
        public string question { get; set; }

        public string question_ws { get; set; }
    }
}
