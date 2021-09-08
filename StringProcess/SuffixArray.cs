using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;

namespace CIExam.StringProcess
{
    public class SuffixArray
    {
        public static (List<int> sa, int[] rank) ConstructSa(string s)
        {
            var n = s.Length;
            var rank = new int[n + 1];
            var sa = new int[n + 1].ToList();
            var tmp = new int[n + 1];
            var k = 0;
            var k1 = k;
            var cmp = new CustomerComparer<int>(delegate(int i, int j)
            {
                if (rank[i] != rank[j]) return rank[i].CompareTo(rank[j]);
                var ri = i + k1 <= n ? rank[i + k1] : -1;
                var rj = j + k1 <= n ? rank[j + k1] : -1;
                return ri.CompareTo(rj);
            });
            
            for (var i = 0; i <= n; i++)
            {
                sa[i] = i;
                rank[i] = i < n ? s[i] : -1;
            }

            for (k = 1; k <= n; k *= 2)
            {
                sa.Sort(cmp);

                tmp[sa[0]] = 0;
                for (var i = 1; i <= n; i++)
                {
                    tmp[sa[i]] = tmp[sa[i - 1]] + (cmp.Compare(sa[i - 1], sa[i]) != 0 ? 1 : 0);
                }
                for (var i = 0; i <= n; i++)
                {
                    rank[i] = tmp[i];
                }
            }

            return (sa, rank);
        }
    }
}