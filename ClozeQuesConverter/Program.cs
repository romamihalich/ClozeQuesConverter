﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ClozeQuesConverter
{
    class Program
    {
        static readonly Regex beginRegex = new Regex(@"^<begin (.*);\s*([01])\s*>$");
        static readonly Regex endRegex = new Regex(@"^<end>$");

        /// <summary>
        /// Ищет следующий вопрос в потоке input
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Объект типа Question, если не нашлось, то null</returns>
        static Question GetNextQuestion(StreamReader input, ref int lineCount)
        {
            string name = string.Empty;
            bool shuffleAnswers = false;
            while (input.EndOfStream == false)
            {
                string currentString = input.ReadLine().Trim();
                lineCount++;
                if (beginRegex.IsMatch(currentString))
                {
                    //init name and shaffleAnswers 
                    var groups = beginRegex.Match(currentString).Groups;
                    name = groups[1].Value;
                    shuffleAnswers = groups[2].Value == "1" ? true : false;
                    break;
                }
                else if (string.IsNullOrEmpty(currentString) == false)
                    throw new SyntaxErrorException($"no <begin>\nline: {lineCount}");
                if (endRegex.IsMatch(currentString))
                    throw new SyntaxErrorException($"no <begin>\nline: {lineCount}");
            }
            var body = new StringBuilder();
            bool endFlag = true;
            while (input.EndOfStream == false)
            {
                endFlag = false;
                string currentString = input.ReadLine();
                lineCount++;
                string trimmedCurStr = currentString.Trim();
                if (endRegex.IsMatch(trimmedCurStr) == false)
                    body.Append(currentString + "<br>"); // br for new line
                else { endFlag = true; break; }
                if (beginRegex.IsMatch(trimmedCurStr))
                    throw new SyntaxErrorException($"no <end>\nline: {lineCount}");
            }
            var bodyStr = body.ToString();
            if (endFlag == false)
                throw new SyntaxErrorException($"no <end>\nline: {lineCount}");
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

            int lineCount = 0;

            outputSW.WriteLine(xmlPatternBegin);
            try
            {
                var nextQuestion = GetNextQuestion(inputSR, ref lineCount);

                while (nextQuestion != null)
                {
                    outputSW.WriteLine(ConvertToXml(nextQuestion));
                    nextQuestion = GetNextQuestion(inputSR, ref lineCount);
                }
            }
            catch (SyntaxErrorException)
            {
                //Console.WriteLine(ex.Message);
                throw;
            }

            outputSW.WriteLine(xmlPatternEnd);

            inputSR.Close();
            outputSW.Close();
        }

        static FileInfo GetFileConsole()
        {
            FileInfo input;
            while (true)
            {
                input = new FileInfo(Console.ReadLine());
                if (input.Exists == false)
                {
                    Console.WriteLine("Файл не существует");
                    continue;
                }
                if (input.Extension != ".txt")
                {
                    Console.WriteLine("Расширение должно быть .txt");
                    continue;
                }
                break;
            }
            return input;
        }
        //


        static readonly string[] shortanswers =
            new[] { "SHORTANSWER", "SA", "MW", "SHORTANSWER_C", "SAC", "MWC" };
        static bool IsShortanswer(string str)
        {
            var regex = new Regex(@"^{\d+:([A-Z_]+):(=|%\d+%)[^~{}#]+(#[^~{}#]+)?(~(=|%\d+%)[^~{}#]+(#[^~{}#]+)?)*}$");
            if (regex.IsMatch(str) == false)
                return false;
            if (shortanswers.Contains(regex.Match(str).Groups[1].Value) == false)
                return false;
            return true;
        }

        static void Check1(string s)
        {
            Regex regex = new Regex(@"{[0-9]{1,}:SHORTANSWER:=.+}");
            Regex regex1 = new Regex(@"{[0-9]{1,}:SHORTANSWER_C:=.+}");
            Regex regex2 = new Regex(@"{[0-9]{1,}:NUMERICAL:=[0-9]{1,}}");
            Regex regex3 = new Regex(@"{[0-9]{1,}:MULTICHOICE:.*=+.+~+.+}");
            Regex regex4 = new Regex(@"{[0-9]{1,}:MULTICHOICE_V:=+.+~+.+}");
            Regex regex5 = new Regex(@"{[0-9]{1,}:MULTICHOICE_H:=+.+~+.+}");
            Regex regex6 = new Regex(@"{[0-9]{1,}:MULTIRESPONSE:=+.+~+.+}");
            Regex regex7 = new Regex(@"{[0-9]{1,}:MULTIRESPONSE_H:=+.+~+.+}");
            Regex regex8 = new Regex(@"{[0-9]{1,}:MULTICHOICE_S:=+.+~+.+}");
            Regex regex9 = new Regex(@"{[0-9]{1,}:MULTICHOICE_VS :=+.+~+.+}");
            Regex regex10 = new Regex(@"{[0-9]{1,}:MULTICHOICE_HS :=+.+~+.+}");
            Regex regex11 = new Regex(@"{[0-9]{1,}:MULTIRESPONSE_S :=+.+~+.+}");
            Regex regex12 = new Regex(@"{[0-9]{1,}:MULTIRESPONSE_HS :=+.+~+.+}");
            MatchCollection matches = regex.Matches(s);
            MatchCollection matches1 = regex1.Matches(s);
            MatchCollection matches2 = regex2.Matches(s);
            MatchCollection matches3 = regex3.Matches(s);
            MatchCollection matches4 = regex4.Matches(s);
            MatchCollection matches5 = regex5.Matches(s);
            MatchCollection matches6 = regex6.Matches(s);
            MatchCollection matches7 = regex7.Matches(s);
            MatchCollection matches8 = regex8.Matches(s);
            MatchCollection matches9 = regex9.Matches(s);
            MatchCollection matches10 = regex10.Matches(s);
            MatchCollection matches11 = regex11.Matches(s);
            MatchCollection matches12 = regex12.Matches(s);
            int sum = matches.Count + matches1.Count + matches2.Count + matches3.Count + matches4.Count + matches5.Count + matches6.Count + matches7.Count + matches8.Count + matches9.Count + matches10.Count + matches11.Count + matches12.Count;
            if (sum > 0)
            {
                Console.WriteLine("Выражение задано правильно");
            }
            else
            {
                Console.WriteLine("Выражение задано неправильно!");
            }
        }
        //
        
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Введите имя файла (Расширение .txt)");

                var input = GetFileConsole();
                var output = new FileInfo(input.FullName.Replace(".txt", ".xml"));

                try
                {
                    ConvertTxtToXml(input.FullName, output.FullName);
                }
                catch (SyntaxErrorException ex)
                {
                    Console.WriteLine("Синтаксическая ошибка");
                    Console.WriteLine(ex.Message + "\n");
                    continue;
                }

                Console.WriteLine("Файл успешно сохранен\n");

            }
        }
    }
}
