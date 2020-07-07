using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Program
    {
        static readonly Regex beginRegex = new Regex(@"<begin name=.*; sa=[0|1]>");
        static readonly Regex endRegex = new Regex(@"<end>");

        static string GetNextQuestion(StreamReader input)
        {
            while (input.EndOfStream == false)
            {
                string curString = input.ReadLine();
                if (beginRegex.IsMatch(curString))
                    break;
            }
            var question = new StringBuilder();
            while (input.EndOfStream == false)
            {
                string curString = input.ReadLine();
                if (endRegex.IsMatch(curString) == false)
                    question.Append(curString);
                else break;
            }
            return question.ToString();
        }
        static void Main(string[] args)
        {
            var input = new StreamReader("test.txt");
            var output = new StreamWriter("output.xml");

            Console.WriteLine(GetNextQuestion(input));
            Console.WriteLine(GetNextQuestion(input));
        }
    }
}
