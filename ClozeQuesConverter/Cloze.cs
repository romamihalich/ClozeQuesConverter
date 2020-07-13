using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Cloze
    {
        public int Value { get; }
        public string Type { get; }
        public List<Answer> Answers { get; set; }

        public Cloze(int value, string type, List<Answer> answers)
        {
            Value = value;
            Type = type;
            Answers = answers;
        }

        public override string ToString()
        {
            return $"{{{Value}:{Type}:{string.Join("~", Answers)}}}";
        }
    }
}
