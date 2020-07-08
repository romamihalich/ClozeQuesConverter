﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Program
    {
        static readonly Regex beginRegex = new Regex(@"^<begin (.*);\s*([0|1])\s*>$");
        static readonly Regex endRegex = new Regex(@"^<end>$");

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
                string currentString = input.ReadLine().Trim();
                if (beginRegex.IsMatch(currentString))
                {
                    //init name and shaffleAnswers 
                    var groups = beginRegex.Match(currentString).Groups;
                    name = groups[1].Value;
                    shuffleAnswers = groups[2].Value == "1" ? true : false;
                    break;
                }
                else if (string.IsNullOrEmpty(currentString) == false)
                    throw new SyntaxErrorException("no <begin>");
            }
            var body = new StringBuilder();
            bool endFlag = true;
            while (input.EndOfStream == false)
            {
                endFlag = false;
                string currentString = input.ReadLine();
                string trimmedCurStr = currentString.Trim();
                if (endRegex.IsMatch(trimmedCurStr) == false)
                    body.Append(currentString);
                else { endFlag = true; break; }
            }
            var bodyStr = body.ToString();
            if (endFlag == false)
                throw new SyntaxErrorException("no <end>");
            if (string.IsNullOrEmpty(bodyStr))
                return null;
            return new Question(name, bodyStr, shuffleAnswers);
        }

        static readonly string questionPattern =
            "<question type=\"cloze\">" +
            "<name><text>\n NAME \n</text></name>" +
            "<questiontext><text><![CDATA[\n BODY \n]]></text></questiontext>" +
            "<generalfeedback>" +
            "<text></text>" +
            "</generalfeedback>" +
            "<shuffleanswers>\n SHUFFLEANSWERS \n</shuffleanswers>" +
            "</question>";

        static string ConvertToXml(Question question)
        {
            var questXml = questionPattern.Replace("NAME", question.Name)
                .Replace("BODY", question.Body)
                .Replace("SHUFFLEANSWERS", question.ShuffleAnswers ? "1" : "0");
            return questXml;
        }

        static readonly string xmlPatternBegin =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><quiz>";
        static readonly string xmlPatternEnd = "</quiz>";

        static void ConvertTxtToXml(string input, string output)
        {
            var inputSR = new StreamReader(input);
            var outputSW = new StreamWriter(output);

            outputSW.WriteLine(xmlPatternBegin);
            var nextQuestion = GetNextQuestion(inputSR);
            while(nextQuestion != null)
            {
                outputSW.WriteLine(ConvertToXml(nextQuestion));
                nextQuestion = GetNextQuestion(inputSR);
            }
            outputSW.WriteLine(xmlPatternEnd);

            inputSR.Close();
            outputSW.Close();
        }

        static void Main(string[] args)
        {
            ConvertTxtToXml("test.txt", "output.xml");
            Console.ReadKey();
        }
    }
}
