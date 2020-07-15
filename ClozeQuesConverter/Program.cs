using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
                string currentLine = input.ReadLine().Trim();
                lineCount++;
                if (beginRegex.IsMatch(currentLine))
                {
                    //init name and shaffleAnswers 
                    var groups = beginRegex.Match(currentLine).Groups;
                    name = groups[1].Value;
                    shuffleAnswers = groups[2].Value == "1" ? true : false;
                    break;
                }
                else if (string.IsNullOrEmpty(currentLine) == false)
                    throw new SyntaxErrorException($"area outside of question must be empty or white space\nline: {lineCount}");
            }
            var body = new StringBuilder();
            bool endFlag = true;
            int clozeCount = 0;
            while (input.EndOfStream == false)
            {
                endFlag = false;
                string currentLine = input.ReadLine();
                lineCount++;
                string trimmedCurLine = currentLine.Trim();
                if (beginRegex.IsMatch(trimmedCurLine))
                    throw new SyntaxErrorException($"trying to create new question inside question({name})\nline: {lineCount}");
                if (endClozeRegex.IsMatch(trimmedCurLine))
                    throw new SyntaxErrorException($"trying to end cloze question, that doesn't exist\nline: {lineCount}");
                if (endRegex.IsMatch(trimmedCurLine) == false)
                {
                    if (beginClozeRegex.IsMatch(trimmedCurLine))
                    {
                        body.Append(GetNextCloze(input, trimmedCurLine, ref lineCount));
                        clozeCount++;
                    }
                    else body.Append(currentLine);
                }
                else { endFlag = true; break; }
            }
            var bodyStr = body.ToString();
            if (endFlag == false)
                throw new SyntaxErrorException($"couldn't find end in question({name})\nline: {lineCount}");
            if (string.IsNullOrEmpty(bodyStr))
                return null;
            if (clozeCount == 0)
                throw new SyntaxErrorException($"question ({name}) must contain at least one cloze question\nline: {lineCount}");
            return new Question(name, bodyStr, shuffleAnswers);
        }


        static readonly Regex beginClozeRegex = new Regex(@"^<cloze\s+(?<type>[A-Z_]+)\s*;\s*(?<value>\d+)\s*>$");
        static readonly Regex endClozeRegex = new Regex(@"^</cloze>$");
        
        static Cloze GetNextCloze(StreamReader input, string first, ref int lineCount)
        {
            var groups = beginClozeRegex.Match(first).Groups;
            var type = groups["type"].Value;
            var value = int.Parse(groups["value"].Value);

            var result = new Cloze(value, type, new List<Answer>());
            if (result.IsTypeValid() == false)
                throw new SyntaxErrorException($"this type doesn't exist\nline: {lineCount}");

            bool endClozeFlag = true;
            while (input.EndOfStream == false)
            {
                endClozeFlag = false;
                var currentLine = input.ReadLine().Trim();
                lineCount++;
                if (beginClozeRegex.IsMatch(currentLine))
                    throw new SyntaxErrorException($"trying to create new cloze question inside cloze question\nline: {lineCount}");
                if (endClozeRegex.IsMatch(currentLine) == false)
                {
                    var currentAnswer = new Answer(currentLine);
                    if (currentAnswer.Percentage > 100 || currentAnswer.Percentage < -100)
                        throw new SyntaxErrorException($"percentage must be in range [-100,100]\nline: {lineCount}");
                    //TODO: add answers check
                    // check body and feedback
                    if (result.IsNumerical())
                    {
                        if (CheckNumericalAnswer(currentAnswer.Body) == false)
                            throw new SyntaxErrorException($"wrong format\nline: {lineCount}");
                    } else 
                    { 
                        SearchingMistakes(currentAnswer.Body, ref lineCount); 
                        if (string.IsNullOrEmpty(currentAnswer.Feedback) == false)
                            SearchingMistakes(currentAnswer.Feedback, ref lineCount); 
                    }
                    result.Answers.Add(currentAnswer);
                }
                else { endClozeFlag = true; break; }
            }
            if (endClozeFlag == false)
                throw new SyntaxErrorException($"couldn't find end in cloze question\nline: {lineCount}");
            if (result.Answers.Count == 0)
                throw new SyntaxErrorException($"cloze question must contain at least one answer\nline: {lineCount}");
            if (result.IsMultiresponse() == false) 
                if (result.Answers.Where(x => (x.Percentage == 100)).Count() == 0)
                    throw new SyntaxErrorException($"cloze question must contain at least one answer with 100%\nline: {lineCount}");
            return result;
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
            var inputSR = new StreamReader(input, Encoding.UTF8);
            var outputSW = new StreamWriter(output, false, Encoding.UTF8);

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

        static readonly Regex numericalRegex =
            new Regex(@"^(?<answer>.*?)(:(?<range>.*))?$");
        static bool CheckNumericalAnswer(string str)
        {
            var groups = numericalRegex.Match(str).Groups;

            if (groups["answer"].Success == false
                || groups["answer"].Value != groups["answer"].Value.Trim()
                || double.TryParse(groups["answer"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double answer) == false)
                return false;

            if (groups["range"].Success == false)
                return true;
            if (groups["range"].Value != groups["range"].Value.Trim()
                || double.TryParse(groups["range"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double range) == false)
                return false;

            return true;
        }

        static void SearchingMistakes(string s, ref int lineCount)
        {
            int i = 0;
            while (i <= s.Length - 1)
            {
                if ((s[i] == '}') || (s[i] == '#') || (s[i] == '~') || (s[i] == '/') || (s[i] == '\"') || (s[i] == '\"'))
                    Console.WriteLine("Обратите внимание на "+i+"-ый символ в "+lineCount+"-ой строке!("+s[i]+")Так как он может нести двойное значение.");
                i++;
            }
        }

        static readonly Regex txtRegex = new Regex(@".txt$");
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Введите имя файла (Расширение .txt)");

                var input = GetFileConsole();
                var output = new FileInfo(Path.Combine(input.DirectoryName, txtRegex.Replace(input.Name, ".xml")));

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
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("Проблемы с доступом к файлу");
                    Console.WriteLine(ex.Message + "\n");
                    continue;
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message + "\n");
                    continue;
                }

                Console.WriteLine($"Файл {output.Name} успешно сохранен в {output.DirectoryName}\n");

            }
        }
    }
}
