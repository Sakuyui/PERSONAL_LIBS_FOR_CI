using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.InformaticThoery.Text
{
    public class SuffixTree
    {
        private const int MaxN = 10000;
        int[,] Dp = new int[MaxN, 17];
        public string Str = "";
        private int[] _rank;
        private int[] _sa;

        public bool Contain(string s, string t)
        {
            var a = 0;
            var b = s.Length;
            while (b - a > 1)
            {
                //O(T*log S)
                var c = (a + b) / 2;
                if (string.Compare(s.Substring(_sa[c], t.Length), t, StringComparison.Ordinal) < 0)
                {
                    a = c;
                }
                else
                {
                    b = c;
                }
                
            }
            return string.Compare(s.Substring(_sa[b], t.Length), t, StringComparison.Ordinal) == 0;
        }
        public SuffixTree(string str)
        {
            Str = str;
        }
        
        public void InitRmq()
        {
            var n = Str.Length;
            var h = GenerateH(Str, GenerateSA(Str));
            var l=(int)(System.Math.Log(n * 1.0) / System.Math.Log(2.0));
            _sa = GenerateSA(Str);
            _rank = GenerateRank(_sa);
            h.Length.PrintToConsole();
            
            for(var i = 1; i <= n; i++) 
                Dp[i, 0] = h[i - 1];
            
            for (var j = 1; j <= l; ++j)
            {
                for (var i = 1; i + (1 << j) - 1 <= n; ++i)
                {
                    Dp[i, j] = System.Math.Min(Dp[i, j - 1], Dp[i + (1 << (j - 1)), j - 1]);
                }
            }
        }
        
        public static (int star, int) MostFrequentString(string str)
        {
            var suffixTree = new SuffixTree(str);
            suffixTree.InitRmq();
            var len = str.Length;
            var maxT = 1;
            var star = suffixTree._sa[0];
            var maxLen = 1;
            
            for (var l = 1; l <= len / 2; ++l)
            {
                for (var i = 0; i + l < len; i += l)
                {
                    if (str[i] != str[i + l]) continue;
                    var t = i + l + suffixTree.LcpInquiry(i, i + l) - 1;
                    var j = i;
                    while (j >= 0 && j > i - l && str[j] == str[j + l])
                    {
                        var rp = (t - j + 1) / l;
                        if (rp > maxT || rp == maxT && suffixTree._rank[j] < suffixTree._rank[star])
                        {
                            star = j;
                            maxLen = l;
                            maxT = rp;
                        }
        
                        --j;
                    }
                }
        
            }
        
            return (star, star + maxT * maxLen);
        }
        public int LcpInquiry(int a, int b)
        {
            var i = _rank[a];
            var j = _rank[b];
            void Swap()
            {
                var i1 = i;
                i = j;
                j = i1;
            }
            
            if(i > j) Swap(); ++i;
            var t = (int)(System.Math.Log(j - i + 1.0) / System.Math.Log(2.0));
            return System.Math.Min(Dp[i, t], Dp[j - (1 << t) + 1, t]);
        }
        
        
        private static bool Cmp(int[] y, int a, int b, int l) {
            var tmp = new int[y.Length + 1];
            Enumerable.Range(0, y.Length).ElementInvoke(i => tmp[i] = y[i]);
            return tmp[a] == tmp[b] && tmp[a+l] == tmp[b+l];
        }
        
        public static int[] GenerateSA(string s) {
            var ch = s.ToCharArray();
            var len = ch.Length;
            var m = 256;
            var sa = new int[len];
            var x = new int[len];
            var y = new int[len];
            var ySort = new int[len];
            var bucket = new int[256];
            
            for (var i = 0; i < len; i++) {
                x[i] = ch[i];
                bucket[ch[i]]++;
            }
            for (var i = 1; i < m; i++)
                bucket[i] += bucket[i-1];
            for (var i = len-1; i >= 0; i--)
                sa[--bucket[ch[i]]] = i;
        
            for (int step = 1, p = 0; p < len; step *= 2, m = p) {
                p = 0;
                for (var i = len - step; i < len; i++)
                    y[p++] = i;
                for (var i = 0; i < len; i++) {
                    if (sa[i] >= step)
                        y[p++] = sa[i] - step;
                }
        
                for (var i = 0; i < len; i++)
                    ySort[i] = x[y[i]];
        
                Enumerable.Range(0, bucket.Length).ElementInvoke(i => bucket[i] = 0);
                
                for (var i = 0; i < len; i++)
                    bucket[ySort[i]]++;
                for (var i = 1; i < m; i++)
                    bucket[i] += bucket[i-1];
                for (var i = len - 1; i >= 0; i--)
                    sa[--bucket[ySort[i]]] = y[i];
        
                Enumerable.Range(0, y.Length).ElementInvoke(i => y[i] = 0);
        
                Enumerable.Range(0, len).ElementInvoke(i => y[i] = x[i]);//x -> y
        
                x[sa[0]] = 0;
                p = 1;
                for (var i = 1; i < len; i++)
                    x[sa[i]] = Cmp(y, sa[i], sa[i-1], step) ? p-1 : p++;
            }
            return sa;
        }
        
        public static int[] GenerateRank(int[] sa) {
            var len = sa.Length;
            var rank = new int[len];
            for (var i = 0; i < len; i++)
                rank[sa[i]] = i;
            return rank;
        }
        private static bool IsInBounds(int i, int j, int len) {
            return i < len && j < len;
        }
        public  static int[] GenerateH(string s, int[] sa) {
            var ch = s.ToCharArray();
            var len = sa.Length;
            var h = new int[len];
            var rank = GenerateRank(sa);
        
            var index = 0;
            for (var i = 0; i < len; i++) {
                if (rank[i] == 0)
                    continue;
                for (var j = sa[rank[i]-1]; IsInBounds(i+index, j+index, len) && ch[i+index] == ch[j+index];)
                    index++;
                h[rank[i]] = index;
                if (index > 0)
                    index--;
            }
            return h;
        }
        
         public static IEnumerable<int> TSA(string s)
        {
            s.PrintToConsole();
            var n = s.Length;
            var suffix = Enumerable.Range(0, n).Select(e => s[e..]);
            suffix.PrintCollectionToConsole();
            suffix.Select((e, i) => (e, i)).OrderBy(e => e.e).PrintCollectionToConsole();
            
            
            var dict = s.Distinct().OrderBy(e => e).Select((e, i) => (e, i + 1))
                    .ToDictionary(e => e.e + "", e => e.Item2);
            var nRank = s.Select(e => (e, dict[e + ""])).ToList();
            nRank.PrintEnumerationToConsole();

            var times = 0;
            var tmpN = n;
            while (tmpN > 0)
            {
                times++;
                tmpN >>= 1;
            }
            //merge
            var cur = Enumerable.Range(0, n)
                    .Select(e =>
                        e < n - 1
                            ? new RankTuple(nRank[e].Item2, nRank[e + 1].Item2)
                            : new RankTuple(nRank[e].Item2, 0))
                    .ToList();
                
                cur.PrintEnumerationToConsole();
                
            "".PrintToConsole();

            IEnumerable<int> SortRankTuple(List<RankTuple> tuples)
            {
                "sort".PrintToConsole();
                var r = tuples.Distinct().OrderBy(t => t.Left).ThenBy(t => t.Right)
                    .Select((e, i) => (e, i)).ToDictionary(k => k.e, v => v.i + 1);
                tuples.Select(t => (t,r[t])).PrintEnumerationToConsole();
                return tuples.Select(t => r[t]);
            }

            IEnumerable<RankTuple> Merge(List<int> ranks, int offset)
            {
                "merge".PrintToConsole();
                ranks.PrintEnumerationToConsole();
                var r = Enumerable.Range(0, n)
                    .Select(e => e < n - offset ? new RankTuple(ranks[e], ranks[e + offset]) : new RankTuple(ranks[e], 0));
                r.PrintEnumerationToConsole();
                return r;
            }

            var baseNum = 2;
            for (var i = 2; i < 1 + times; i++)
            {
                var t = SortRankTuple(cur);
                cur = Merge(t.ToList(), baseNum).ToList();
                "".PrintToConsole();
                baseNum *= 2;
            }
            
            //final Sort
            var finalRank = SortRankTuple(cur).ToList();
            "逆接尾辞配列".PrintToConsole();
            finalRank.PrintEnumerationToConsole();
            
            //reverse Rank
            var reRank = new int[finalRank.Count];
            for (var i = 0; i < reRank.Length; i++)
            {
                reRank[finalRank[i] - 1] = i + 1;
            }
            
            "接尾辞配列".PrintToConsole();
            reRank.PrintEnumerationToConsole();
            "接尾辞".PrintToConsole();
            
            foreach (var index in reRank)
            {
                s[(index-1)..].PrintToConsole();
            }
            
            //高さ配列を作る
            var height = new int[reRank.Length];
            return reRank;
        }
    }
    struct RankTuple
        {
            public int Left;
            public int Right;

            public RankTuple(int l, int r)
            {
                Left = l;
                Right = r;
            }

            public override string ToString()
            {
                return $"({Left},{Right})";
            }
        }
       
}