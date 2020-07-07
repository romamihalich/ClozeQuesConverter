using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Program
    {
        static readonly Regex beginRegex = new Regex(@"<begin name=(.*); sa=([0|1])>");
        static readonly Regex endRegex = new Regex(@"<end>");

        static readonly string xamlPattern =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><quiz>(.*)</quiz>";

        /// <summary>
        /// 1-ая группа - название вопроса (Name)
        /// 2-ая группа - сам вопрос (Body)
        /// 3-яя группа - ShuffleAnswers
        ///</summary>
        static readonly string questPattern =
            "<question type=\"cloze\">" +
            "<name><text>(.*)</text>" +
            "</name>" +
            "<questiontext>" +
            "<text><![CDATA[(.*)]]></text>" +
            "</questiontext>" +
            "<generalfeedback>" +
            "<text></text>" +
            "</generalfeedback>" +
            "<shuffleanswers>(.*)</shuffleanswers>" +
            "</question>";


        /// <summary>
        /// Ищет следующий вопрос в потоке input
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Объект типа Question, если не нашлось, то null</returns>
        static Question GetNextQuestion(StreamReader input)
        {
            string name = string.Empty;
            bool shuffleAnswers = false;
            while (input.EndOfStream == false)
            {
                string curString = input.ReadLine();
                if (beginRegex.IsMatch(curString))
                {
                    //init name and shaffleAnswers 
                    var groups = beginRegex.Match(curString).Groups;
                    name = groups[1].Value;
                    shuffleAnswers = groups[2].Value == "1" ? true : false;
                    break;
                }
            }
            var body = new StringBuilder();
            while (input.EndOfStream == false)
            {
                string curString = input.ReadLine();
                if (endRegex.IsMatch(curString) == false)
                    body.Append(curString);
                else break;
            }
            var bodyStr = body.ToString();
            if (string.IsNullOrEmpty(bodyStr))
                return null;
            return new Question(name, bodyStr, shuffleAnswers);
        }



        static void Main(string[] args)
        {
            var input = new StreamReader("test.txt");
            var output = new StreamWriter("output.xml");

            var questions = new List<Question>();
            while(input.EndOfStream == false)
            {
                var nextQuestion = GetNextQuestion(input);
                if (nextQuestion != null)
                    questions.Add(nextQuestion);
            }

            Console.WriteLine(
                string.Join("\n", questions)
                );

            Console.ReadKey();
        }
    }
}
