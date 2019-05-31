using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Common.Filters;
using FuzzySearch;

namespace BloomTest
{
    [TestClass]
    public  class UnitTest1
    {
        //protected abstract IBloomFilter<String> CreateInstance(int numBits, IEnumerable<Func<String, int>> hashFunctions);
        [TestMethod]
        public void TestMethod1()
        {
            IBloomFilterParameters parameters = BloomUtils.CalculateBloomParameters(100, 0.01);
            Console.WriteLine(parameters.NumberOfBits);
            Console.WriteLine(parameters.NumberOfHashFunctions);
        }

        [TestMethod]
        public void TestSim()
        {
            var lsh = new List<byte[]>(2);
            //for(int i = 0; i < 100; i++)
            //{
            //    lsh.Add(Sim.Hash($"test{i}"));
            //}
            //var can = LSH.Candidates(lsh);
            //int count = 0;
            //foreach(HashSet<int> c in can)
            //{
            //    count++;
            //    Console.WriteLine($"第{count}个");
            //    foreach (int i in c)
            //    {
            //        Console.WriteLine($"第{count}个");
            //        Console.WriteLine(i);
            //    }
            //}
            for(int i = 0; i < lsh[0].Length; i++)
            {
                Console.Write($"{lsh[0][i]}         ");
                Console.WriteLine(lsh[1][i]);
            }
        }

        [TestMethod]
        public void TestBiGram()
        {
            string stemKeyword = "tratra";
            List<string> tempStemKeyword = new List<string>();
            var length = stemKeyword.Length;
            int[] count = new int[length - 1];
            for (var m = 0; m < length - 1; m++)
            {
                count[m] = 1;
            }

            for (var i = 1; i < length - 1; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    if (stemKeyword.Substring(i, 2) == stemKeyword.Substring(j, 2))
                    {
                        count[i]++;
                    }
                }
                tempStemKeyword.Add(stemKeyword.Substring(i, 2) + count[i]);
            }
            foreach(string s in tempStemKeyword)
            {
                Console.WriteLine(s);
            }
        }

        [TestMethod]
        public void TestIntMinHash()
        {
            MinHash _mh = new MinHash(1000, 100);

            double[] bloom = new double[10000];
            int[] count = new int[10000];
            double[] bloom1 = new double[10000];
            int[] count1 = new int[10000];

            //var biList1 = SchemeProcess.TransformKeywordsToBiGram("cat");
            //var index1 = SchemeProcess.GenerateVector(biList1);
            //var res1 = _mh.getMinHashSignatures("ca1");

            int len = 0;

            List<string> stemmedDoc;

            var stemSet = SchemeProcess.GetVocabulary("my name is zjw", out stemmedDoc, 0);
            var stemSet1 = SchemeProcess.GetVocabulary("my name is wrm", out stemmedDoc, 0);

            foreach (string stem in stemSet)
            {
                var biList = MyScheme.TransformKeywordsToBiGram(stem);
                //var index = SchemeProcess.GenerateVector(biList);
                foreach (string s in biList)
                {
                    foreach (int i in _mh.getMinHashSignatures(s))
                    {
                        if (i >= 10000) continue;
                        if (bloom[i] == 0)
                        {
                            bloom[i] = 1;
                            count[i]++;
                        }
                        else
                        {
                            bloom[i] = (bloom[i] * count[i] + 1) / (++count[i]);
                        }
                    }
                }
            }

            foreach (string stem in stemSet1)
            {
                var biList = MyScheme.TransformKeywordsToBiGram(stem);
                //var index = SchemeProcess.GenerateVector(biList);
                foreach (string s in biList)
                {
                    foreach (int i in _mh.getMinHashSignatures(s))
                    {
                        if (i >= 1000) continue;
                        if (bloom1[i] == 0)
                        {
                            bloom1[i] = 1;
                            count1[i]++;
                        }
                        else
                        {
                            bloom1[i] = (bloom1[i] * count1[i] + 1) / (++count1[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < bloom.Length; i++)
            {
                if (bloom[i] == bloom1[i] && bloom[i].Equals(1))
                    len++;
                Console.Write($"{bloom[i]}         ");
                Console.WriteLine(bloom1[i]);
            }
            Console.WriteLine(len);
        }
    }


}
