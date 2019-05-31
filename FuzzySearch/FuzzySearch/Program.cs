using System;
using System.Collections.Generic;
using System.IO;
using Iveonik.Stemmers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FuzzySearch
{
    public class FuzzySearch
    {
        private static string[] FileList;
        private static List<double[]> blooms = new List<double[]>();
        private static List<double[]> bloomsFu = new List<double[]>();
        private static Dictionary<string, int> _wordIdf = new Dictionary<string, int>();
        private static MinHash _mh = new MinHash(1000, 30);
        private static Stopwatch stopwatch = new Stopwatch();
        private static Dictionary<string, double> _myScheme = new Dictionary<string, double>();
        private static Dictionary<string, double> _fuScheme = new Dictionary<string, double>();

        public static void Main(string[] args)
        {
            blooms.Clear();
            bloomsFu.Clear();
            _wordIdf.Clear();
            List<List<string>> stemmedDocs;
            List<string> vocabulary;

            FileList = SchemeProcess.GenerateFileList().ToArray();

            stopwatch.Restart();
            vocabulary = SchemeProcess.GetVocabulary(FileList, out stemmedDocs, 0);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);


            if (_wordIdf.Count == 0)
            {
                // 设置idf的变量，以供生成trapdoor时使用
                foreach (var term in vocabulary)
                {
                    _wordIdf[term] = stemmedDocs.Where(d => d.Contains(term)).Count();
                }
            }


            //my scheme
            stopwatch.Restart();
            foreach (List<string> stemDoc in stemmedDocs)
            {
                double[] bloom = new double[8000];
                int[] count = new int[8000];

                HashSet<string> stemSet = new HashSet<string>(stemDoc);

                foreach (string stem in stemSet)
                {
                    double tf = (double)stemDoc.Where(d => d == stem).Count() / (double)stemDoc.Count;
                    //int[] vector = new int[1352];
                    var biList = MyScheme.TransformKeywordsToBiGram(stem);
                    //var uniList = FuScheme.TransformKeywordsToUniGram(stem);
                    //var index = SchemeProcess.GenerateVector(biList);
                    var index = MyScheme.BiGramToVector(biList);
                    //var s = string.Join("", index);
                    foreach (int i in _mh.getMinHashSignatures(index))
                    {
                        if (i >= 8000) continue;
                        if (bloom[i] == 0)
                        {
                            bloom[i] = tf;
                            count[i]++;
                        }
                        else
                        {
                            bloom[i] = (bloom[i] * count[i] + tf) / (++count[i]);
                        }
                    }
                    //foreach (string s in uniList)
                    //{
                    //    foreach (int i in _mh.getMinHashSignatures(s))
                    //    {
                    //        if (i >= 8000) continue;
                    //        if (bloomFu[i] == 0)
                    //        {
                    //            bloomFu[i] = tf;
                    //            countFu[i]++;
                    //        }
                    //        else
                    //        {
                    //            bloomFu[i] = (bloomFu[i] * countFu[i] + tf) / (++countFu[i]);
                    //        }
                    //    }
                    //}
                }
                blooms.Add(bloom);
                //bloomsFu.Add(bloomFu);
            }
            stopwatch.Stop();
            Console.WriteLine($"Bi-Gram生成索引所需时间为：{stopwatch.Elapsed.TotalMilliseconds}");


            ///Fu's scheme
            stopwatch.Restart();
            foreach (List<string> stemDoc in stemmedDocs)
            {
                double[] bloomFu = new double[8000];
                int[] countFu = new int[8000];

                HashSet<string> stemSet = new HashSet<string>(stemDoc);

                foreach (string stem in stemSet)
                {
                    double tf = (double)stemDoc.Where(d => d == stem).Count() / (double)stemDoc.Count;
                    //int[] vector = new int[1352];
                    //var biList = MyScheme.TransformKeywordsToBiGram(stem);
                    var uniList = FuScheme.TransformKeywordsToUniGram(stem);
                    var index = FuScheme.UniGramToVector(uniList);
                    //var index = SchemeProcess.GenerateVector(biList);
                    //foreach (string s in biList)
                    //{
                    //    foreach (int i in _mh.getMinHashSignatures(s))
                    //    {
                    //        if (i >= 8000) continue;
                    //        if (bloom[i] == 0)
                    //        {
                    //            bloom[i] = tf;
                    //            count[i]++;
                    //        }
                    //        else
                    //        {
                    //            bloom[i] = (bloom[i] * count[i] + tf) / (++count[i]);
                    //        }
                    //    }
                    //}
                    foreach (int i in _mh.getMinHashSignatures(index))
                    {
                        if (i >= 8000) continue;
                        if (bloomFu[i] == 0)
                        {
                            bloomFu[i] = tf;
                            countFu[i]++;
                        }
                        else
                        {
                            bloomFu[i] = (bloomFu[i] * countFu[i] + tf) / (++countFu[i]);
                        }
                    }
                }
                //blooms.Add(bloom);
                bloomsFu.Add(bloomFu);
            }
            stopwatch.Stop();
            Console.WriteLine($"Uni-Gram生成索引所需时间为：{stopwatch.Elapsed.TotalMilliseconds}");
            Console.ReadLine();

            QueryWithMyScheme("Ferewele");
            QueryWithFuScheme("Ferewele");
        }

        /// <summary>
        /// 用本人方案进行检索的结果
        /// </summary>
        /// <param name="s"></param>
        private static void QueryWithMyScheme(string s)
        {
            Console.WriteLine($"当前使用方案为本人的");
            Console.WriteLine($"当前检索文本为{s}");

            List<string> stemmedDoc;
            List<string> vocabulary;

            vocabulary = SchemeProcess.GetVocabulary(s, out stemmedDoc, 0);

            double[] bloom = new double[8000];
            int[] count = new int[8000];


            foreach (string stem in vocabulary)
            {
                int[] vector = new int[8000];
                var biList = MyScheme.TransformKeywordsToBiGram(stem);
                var index = MyScheme.BiGramToVector(biList);
                //string bi = string.Join("", index);
                //var index = SchemeProcess.GenerateVector(biList);
                foreach (int i in _mh.getMinHashSignatures(index))
                {
                    if (i >= 8000) continue;
                    if (bloom[i] == 0)
                    {
                        if (!_wordIdf.ContainsKey(stem))
                        {
                            bloom[i] = 0;
                        }
                        else
                        {
                            bloom[i] = _wordIdf[stem];
                        }
                        bloom[i] = Math.Log((double)FileList.Length / (bloom[i] + 1));
                        count[i]++;
                    }
                    else
                    {
                        double temp = 0;
                        if (!_wordIdf.ContainsKey(stem))
                        {
                            temp = 0;
                        }
                        else
                        {
                            temp = _wordIdf[stem];
                        }
                        temp = Math.Log((double)FileList.Length / (temp + 1));
                        bloom[i] = (bloom[i] * count[i] + temp) / (double)(++count[i]);
                    }
                }
            }

            for (int i = 0; i < blooms.Count; i++)
            {
                double score = 0;
                for (int j = 0; j < 8000; j++)
                {
                    score += blooms[i][j] * bloom[j];
                }
                _myScheme.Add(FileList[i], score);
                //_myScheme[score] = FileList[i];
                //res.Add(score, FileList[i]);
                //Console.WriteLine($"第{i}个文本为{FileList[i]}；");
                //Console.WriteLine($"分数为{score}");
            }

            var dicSort = from objDic in _myScheme orderby objDic.Value descending select objDic;

            int rank = 1;

            foreach(KeyValuePair<string, double> item in dicSort)
            {
                Console.WriteLine($"{rank++}、文本为{item.Key}；");
                Console.WriteLine($"  分数为{item.Value};");
            }

            Console.ReadLine();
        }

        private static void QueryWithFuScheme(string s)
        {
            Console.WriteLine($"当前使用方案为Fu的");
            Console.WriteLine($"当前检索文本为{s}");

            List<string> stemmedDoc;
            List<string> vocabulary;

            vocabulary = SchemeProcess.GetVocabulary(s, out stemmedDoc, 0);

            double[] bloom = new double[8000];
            int[] count = new int[8000];


            foreach (string stem in vocabulary)
            {
                int[] vector = new int[8000];
                var uniList = FuScheme.TransformKeywordsToUniGram(stem);
                var index = FuScheme.UniGramToVector(uniList);
                //var index = SchemeProcess.GenerateVector(biList);
                foreach (int i in _mh.getMinHashSignatures(index))
                {
                    if (i >= 8000) continue;
                    //if (bloom[i] == 0)
                    //{
                    //    //if (!_wordIdf.ContainsKey(stem))
                    //    //{
                    //    //    bloom[i] = 0;
                    //    //}
                    //    //else
                    //    //{
                    //    //    bloom[i] = _wordIdf[stem];
                    //    //}
                    //    //bloom[i] = Math.Log((double)FileList.Length / (bloom[i] + 1));
                    //    bloom[i] = 1;
                    //    count[i]++;
                    //}
                    //else
                    //{
                    //    //double temp = 0;
                    //    //if (!_wordIdf.ContainsKey(stem))
                    //    //{
                    //    //    temp = 0;
                    //    //}
                    //    //else
                    //    //{
                    //    //    temp = _wordIdf[stem];
                    //    //}
                    //    //temp = Math.Log((double)FileList.Length / (temp + 1));
                    //    //bloom[i] = (bloom[i] * count[i] + temp) / (++count[i]);
                    //    bl
                    //}
                    //if (bloom[i] == 0)
                    //{
                    //    bloom[i] = 1;
                    //    count[i] = 1;
                    //}
                    //else
                    //{
                    //    bloom[i] = (bloom[i] * count[i] + 1) / (double)(++count[i]);
                    //}
                    bloom[i] = 1;
                }
            }

            for (int i = 0; i < bloomsFu.Count; i++)
            {
                double score = 0;
                for (int j = 0; j < 8000; j++)
                {
                    score += bloomsFu[i][j] * bloom[j];
                }
                _fuScheme.Add(FileList[i], score);
                //_fuScheme[score] = FileList[i];
                //res.Add(score, FileList[i]);
                //Console.WriteLine($"第{i}个文本为{FileList[i]}；");
                //Console.WriteLine($"分数为{score}");
            }

            var dicSort = from objDic in _fuScheme orderby objDic.Value descending select objDic;

            int rank = 1;

            foreach (KeyValuePair<string, double> item in dicSort)
            {
                Console.WriteLine($"{rank++}、文本为{item.Key}；");
                Console.WriteLine($"  分数为{item.Value};");
            }

            Console.ReadLine();
        }

        private static double CaculateThreshold(string s)
        {
            double res = 0;

            List<string> stemmedDoc;
            List<string> vocabulary;

            vocabulary = SchemeProcess.GetVocabulary(s, out stemmedDoc, 0);

            var ss = GenerateRandomString(s);

            return res;
        }

        private static string[] GenerateRandomString(string s)
        {
            string[] res = new string[10];
            for(int i = 0; i < 10; i++)
            {

            }
            return res;
        }
    }
}
