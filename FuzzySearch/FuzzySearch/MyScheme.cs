using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzySearch
{
    public class MyScheme
    {
        /// <summary>
        /// 将关键词变为用bi-gram表示
        /// </summary>
        /// <param name="stemKeyword"></param>
        /// <returns></returns>
        public static List<string> TransformKeywordsToBiGram(string stemKeyword)
        {
            List<string> tempStemKeyword = new List<string>();
            var length = stemKeyword.Length;
            if (length == 1)
            {
                tempStemKeyword.Add(stemKeyword + "1");
                return tempStemKeyword;
            }
            int[] count = new int[length - 1];
            for (var m = 0; m < length - 1; m++)
            {
                count[m] = 1;
            }
            tempStemKeyword.Add(stemKeyword.Substring(0, 2) + count[0]);
            for (var i = 1; i < length - 1; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    if (stemKeyword.Substring(i, 2).Equals(stemKeyword.Substring(j, 2)))
                    {
                        count[i]++;
                    }
                }
                tempStemKeyword.Add(stemKeyword.Substring(i, 2) + count[i]);
            }
            return tempStemKeyword;
        }

        public static int[] BiGramToVector(List<string> biList)
        {
            int[] index = new int[biList.Count];
            int nu = 0;

            foreach (string bi in biList)
            {
                if (bi.Length != 3) continue;
                char[] ch = new char[2] { bi[0], bi[1] };
                var bytes = Encoding.ASCII.GetBytes(ch);
                var num = (((int)bytes[0] - 97) * 26 + (((int)bytes[1]) - 96)) * ((int)bi[2] - 48) - 1;
                index[nu++] = num;
            }
            return index;
        }

        

    }
}
