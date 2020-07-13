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

        static readonly string[] shortanswers =
            new[] { "SHORTANSWER", "SA", "MW", "SHORTANSWER_C", "SAC", "MWC" };
        public bool IsShortanswer()
            => shortanswers.Contains(Type);

        static readonly string[] numericals =
            new[] { "NUMERICAL", "NM" };
        public bool IsNumerical()
            => numericals.Contains(Type);

        static readonly string[] multichoices =
            new[] { "MULTICHOICE", "MC", "MULTICHOICE_V", "MCV", "MULTICHOICE_H", "MCH",
            "MULTICHOICE_S", "MCS", "MULTICHOICE_VS", "MCVS", "MULTICHOICE_HS", "MCHS"};
        public bool IsMultichoice()
            => multichoices.Contains(Type);

        static readonly string[] multiresponses =
            new[] { "MULTIRESPONSE", "MR", "MULTIRESPONSE_H", "MRH",
            "MULTIRESPONSE_S", "MRS", "MULTIRESPONSE_HS", "MRHS"};
        public bool IsMultiresponse()
            => multiresponses.Contains(Type);

        public bool IsTypeValid()
            => IsShortanswer() || IsNumerical() || IsMultichoice() || IsMultiresponse();

        public override string ToString()
        {
            return $"{{{Value}:{Type}:{string.Join("~", Answers)}}}";
        }
    }
}
