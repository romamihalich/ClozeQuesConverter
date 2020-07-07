using System;
using System.Collections.Generic;
using System.Text;

namespace ClozeQuesConverter
{
    class Question
    {
        public string Name { get; }
        public string Body { get; }
        public bool ShuffleAnswers { get; }

        public Question(string name, string body, bool shuffleAnswers)
        {
            Name = name;
            Body = body;
            ShuffleAnswers = shuffleAnswers;
        }

        public override string ToString()
        {
            return string.Format("\nName:{0} sa:{1}\n{2}",
                Name, ShuffleAnswers ? 1 : 0, Body);
        }
    }
}
