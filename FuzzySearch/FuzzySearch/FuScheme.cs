using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzySearch
{
    class FuScheme
    {
        /// <summary>
        /// 将关键词用uni-gram表示
        /// </summary>
        /// <param name="stemKeyword"></param>
        /// <returns></returns>
        public static List<string> TransformKeywordsToUniGram(string stemKeyword)
        {
            List<string> tempStemKeyword = new List<string>();
            var length = stemKeyword.Length;
            if (length == 1)
            {
                tempStemKeyword.Add(stemKeyword + "1");
                return tempStemKeyword;
            }
            int[] count = new int[length];
            for (var m = 0; m < length; m++)
            {
                count[m] = 1;
            }
            tempStemKeyword.Add(stemKeyword.Substring(0, 1) + count[0]);
            for (var i = 1; i < length; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    if (stemKeyword.Substring(i, 1).Equals(stemKeyword.Substring(j, 1)))
                    {
                        count[i]++;
                    }
                }
                tempStemKeyword.Add(stemKeyword.Substring(i, 1) + count[i]);
            }
            return tempStemKeyword;
        }

        public static int[] UniGramToVector(List<string> uniList)
        {
            int[] index = new int[uniList.Count];
            int nu = 0;

            foreach (string uni in uniList)
            {
                char[] ch = new char[1] { uni[0] };
                var bytes = Encoding.ASCII.GetBytes(ch);
                var num = (((int)bytes[0]) - 96) * ((int)uni[1] - 48) - 1;
                index[nu++] = num;
            }
            return index;
        }
    }
}
