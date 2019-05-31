using Iveonik.Stemmers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FuzzySearch
{
    public static class SchemeProcess
    {
        /// <summary>
        /// 获取文本列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GenerateFileList()
        {
            var fileName = "FileList.txt";
            List<string> tempFile = new List<string>();
            string file = string.Empty;
            if (!File.Exists(fileName))
            {
                Console.WriteLine("No correct file name!");
                return null;
            }
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            while ((file = streamReader.ReadLine()) != null)
            {
                if (tempFile.Contains(file))
                {
                    continue;
                }
                tempFile.Add(file);
            }
            return tempFile;
        }

        /// <summary>
        /// 获取获得词干组以及所有的词干关键词
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="stemmedDocs"></param>
        /// <param name="vocabularyThreshold"></param>
        /// <returns></returns>
        public static List<string> GetVocabulary(string[] docs, out List<List<string>> stemmedDocs, int vocabularyThreshold)
        {
            List<string> vocabulary = new List<string>();
            Dictionary<string, int> wordCountList = new Dictionary<string, int>();
            stemmedDocs = new List<List<string>>();

            int docIndex = 0;

            foreach (var doc in docs)
            {
                List<string> stemmedDoc = new List<string>();

                docIndex++;

                if (docIndex % 100 == 0)
                {
                    Console.WriteLine("Processing " + docIndex + "/" + docs.Length);
                }

                string[] parts2 = GenerateKeywordsList(doc);

                List<string> words = new List<string>();
                foreach (string part in parts2)
                {
                    // Strip non-alphanumeric characters.
                    string stripped = Regex.Replace(part, "[^a-zA-Z0-9]", "");

                    //if (!StopWords.stopWordsList.Contains(stripped.ToLower()))
                    //{
                    try
                    {
                        var stemmer = new EnglishStemmer();
                        var stem = stemmer.Stem(stripped);
                        words.Add(stem);

                        if (stem.Length > 0)
                        {
                            // Build the word count list.
                            if (wordCountList.ContainsKey(stem))
                            {
                                wordCountList[stem]++;
                            }
                            else
                            {
                                wordCountList.Add(stem, 0);
                            }

                            stemmedDoc.Add(stem);
                        }
                    }
                    catch
                    {
                    }
                    //}
                }

                stemmedDocs.Add(stemmedDoc);
            }

            // Get the top words.
            var vocabList = wordCountList.Where(w => w.Value >= vocabularyThreshold);
            foreach (var item in vocabList)
            {
                vocabulary.Add(item.Key);
            }

            return vocabulary;
        }

        public static List<string> GetVocabulary(string doc, out List<string> stemmedDoc, int vocabularyThreshold)
        {
            List<string> vocabulary = new List<string>();
            Dictionary<string, int> wordCountList = new Dictionary<string, int>();
            stemmedDoc = new List<string>();

            string[] parts2 = GenerateKeywordsList(doc);

            List<string> words = new List<string>();
            foreach (string part in parts2)
            {
                // Strip non-alphanumeric characters.
                string stripped = Regex.Replace(part, "[^a-zA-Z0-9]", "");

                //if (!StopWords.stopWordsList.Contains(stripped.ToLower()))
                //{
                try
                {
                    var stemmer = new EnglishStemmer();
                    var stem = stemmer.Stem(stripped);
                    words.Add(stem);

                    if (stem.Length > 0)
                    {
                        // Build the word count list.
                        if (wordCountList.ContainsKey(stem))
                        {
                            wordCountList[stem]++;
                        }
                        else
                        {
                            wordCountList.Add(stem, 0);
                        }

                        stemmedDoc.Add(stem);
                    }
                }
                catch
                {
                }
                //}
            }

            // Get the top words.
            var vocabList = wordCountList.Where(w => w.Value >= vocabularyThreshold);
            foreach (var item in vocabList)
            {
                vocabulary.Add(item.Key);
            }

            return vocabulary;
        }

        /// <summary>
        /// 生成关键词集合
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] GenerateKeywordsList(string text)
        {
            // Strip all HTML.
            text = Regex.Replace(text, "<[^<>]+>", "");

            // Strip numbers.
            text = Regex.Replace(text, "[0-9]+", "number");

            // Strip urls.
            text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

            // Strip email addresses.
            text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

            // Strip dollar sign.
            text = Regex.Replace(text, "[$]+", "dollar");

            // Strip usernames.
            text = Regex.Replace(text, @"@[^\s]+", "username");

            // Tokenize and also get rid of any punctuation
            return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
        }

        

        
    }
}
