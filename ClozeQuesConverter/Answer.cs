using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Answer
    {
        public int Percentage { get; }
        public string Body { get; }
        public string Feedback { get; }

        private static readonly Regex answerRegex
            = new Regex(@"^%(?<percentage>-?\d+)%(?<body>.*)(<feedback>(?<feedback>.*))?$");
        public Answer(string str)
        {
            var groups = answerRegex.Match(str).Groups;
            Percentage = int.Parse(groups["percentage"].Value);
            Body = groups["body"].Value.Trim();
            if (groups["feedback"].Success)
                Feedback = groups["feedback"].Value.Trim();
            else Feedback = null;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Feedback))
                return $"%{Percentage}%{Body}";
            return $"%{Percentage}%{Body}#{Feedback}";
        }


    }
}
