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
        public List<string> Answers { get; set; }

        public Cloze(int value, string type, List<string> answers)
        {
            Value = value;
            Type = type;
            Answers = answers;
        }

        public override string ToString()
        {
            var ans = Answers.Select(x => x.Replace("<feedback>", "#"));
            return $"{{{Value}:{Type}:{string.Join("~", ans)}}}";
        }
    }
}
