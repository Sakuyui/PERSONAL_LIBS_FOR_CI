using System;
using System.Collections.Specialized;
using System.Linq;
using JJ.Framework.Text;
using CIExam.FunctionExtension;

namespace CIExam.Praticle.TextBook
{
    public class StringAlgorithm
    {
        public static int Kmp(string s, string p)
        {
            var next = GetNext(p);
            var n1 = s.Length;
            var n2 = p.Length;
            var i = 0;
            var j = 0;
            while (i < n1)
            {
                if (s[i] == p[j])
                {
                    if (j == n2 - 1)
                        return i - p.Length + 1;
                    i++;
                    j++;
                }
                else if (j <= 0)
                {
                    i++;
                    j = 0;
                }
                else
                {
                    j = next[j];
                }
            }

            return -1;
        }
        public static int[] GetNext(string p)
        {
            var next = new int[p.Length];
            next[0] = -1;
            int pos = 2, cnd = 0;
            
            while (pos < p.Length)
            {
                //case 1
                if (p[pos - 1] == p[cnd])
                    next[pos++] = cnd++ + 1;
                else if (cnd > 0)
                    cnd = next[cnd];
                else
                    next[pos++] = 0;
            }
            
            return next;
        }


        public static int ShiftOrAlgorithm(string t, string p)
        {
            var tn = t.Length;
            var pn = p.Length;
            var dp = new byte[tn + 1].Select(_ => Enumerable.Repeat(1, pn + 1).ToArray()).ToArray();
            //dp[i,j]代表。p的0~j部分是否 == dp的[i-j+1 ~ i]部分。相等才为0
            var b = new byte[26].Select(_ => Enumerable.Repeat(1, pn).ToArray()).ToArray();
            for (var i = 0; i < pn; i++)
            {
                b[p[i] - 'a'][i] = 0; //字母在i位置出现，设为0
            }

            for (var i = 1; i <= tn; i++)
            {
                for (var j = 1; j <= pn; j++)
                {
                    dp[i][j] = dp[i - 1][j - 1] | b[t[i] - 'a'][j - 1];
                }
            }
            
            //在p字符少时，完全可以位运算
            //D[i + 1] = D[i]<<1 v B[T[i+1]]
            var query = 
                Enumerable.Range(pn, tn - pn).Select(i => dp[i].Select((v, j) => (v, i, j)).Where(t => t.v == 0))
                    .SelectMany(t => t);
            return query.Any() ? query.First().j - query.First().i + 1 : -1;

        }
        
    }
}