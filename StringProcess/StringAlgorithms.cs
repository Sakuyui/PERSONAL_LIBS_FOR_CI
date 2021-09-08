using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;

using CIExam.FunctionExtension;

namespace CIExam.Structure
{
    public static class StringAlgorithms
    {
        
        
        class MyCompare : IComparer<int>
        {
            public int Compare(int x, int y)
            {
             
                return x == y ? -1 : x.CompareTo(y);
            }
        }
        public static int DijskraNetworkDelayTime(int[][] times, int n, int k) {
            var graph = new int[n].Select(e => new System.Collections.Generic.HashSet<int>()).ToArray();
            var cost = new int[n, n];
            foreach (var e in times)
            {
                cost[e[0] - 1, e[1] - 1] = e[2];
                graph[e[0] - 1].Add(e[1]);
            }
            IEnumerable<int> GetExtension(int u) => graph[u - 1];

            
            int Dijskra(int u, int v)
            {
                $"from {u} -> {v}".PrintToConsole();
                var dist = Enumerable.Repeat(int.MaxValue, n).ToArray();
                var closeSet = new System.Collections.Generic.HashSet<int>();
                var openSet = new SortedList<int, int>(new MyCompare()) {{0, u}};
                dist[u - 1] = 0;
                while (openSet.Any())
                {
                    $"cur dist = {dist.ToEnumerationString()}".PrintToConsole();
                    var first = openSet.First().Value;
                    $"get {first}".PrintToConsole();
                    openSet.RemoveAt(0);
                    if (first == v)
                        return dist[v - 1];
                    closeSet.Add(first);
                    var ext = GetExtension(first).Where(e => !closeSet.Contains(e)).ToArray();
                    $"ext = {ext.ToEnumerationString()}".PrintToConsole();
                    foreach (var node in ext)
                    {
                        var newDist = dist[first - 1] + cost[first - 1, node - 1];
                        if (newDist < dist[node - 1])
                        {
                            dist[node - 1] = newDist;
                            $"add {node} with dist = {dist[node - 1]}".PrintToConsole();
                            openSet.Add(dist[node - 1], node);
                        }
                    }
                }
                "".PrintToConsole();
                return dist[v - 1];
            }

            var max = 0;
            for (var i = 1; i <= n; i++)
            {
                if(i == k)
                    continue;
                var l = Dijskra(k, i);
                $"{k} -> {i} = {l}".PrintToConsole();
                max = System.Math.Max(max, l);
            }

            return max;
        }

        public static long GetStringHash<T>(IEnumerable<T> str)
        {
            return GetRabinKarpHashDesc(str.ToArray(), str.Count()).First();
        }

        public static IEnumerable<int> RabinKarp<T>(this IEnumerable<T> source, IEnumerable<T> pattern, bool takeOnlyFirst = false)
        {
            var p = pattern.ToArray();
            var t = source.ToArray();
            if (p.Length > t.Length)
                yield break;
            var hashes = GetRabinKarpHashDesc(t.ToArray(), p.Length).ToArray();
            
            var hash = GetStringHash(p);
           
            bool IsEqualFrom(int @from)
            {
                for (var i = from; i < from + p.Length; i++)
                {
                    if (!t[i].Equals(p[i - from]))
                        return false;
                }

                return true;
            }
            for (var i = 0; i < t.Length - p.Length + 1; i++)
            {
                if (hashes[i] != hash) 
                    continue;
                if (IsEqualFrom(i))
                {
                    if (takeOnlyFirst)
                    {
                        yield return i;
                        yield break;
                    }

                    yield return i;
                }
            }
        }
        public static IEnumerable<long> GetRabinKarpHashDesc<T>(T[] str, int len)
        {
            
            var n = str.Length;
           
            var bitmask = (long)0L;
            const int ep = (1 << 8) - 1;
            
            for (var start = 0; start < n - len + 1; ++start) {
                // compute bitmask of the current sequence in O(1) time
                if (start != 0) {
                    // left shift to free the last 2 bit
                    if (len >= 8)
                    {
                        bitmask <<= 8;
                        bitmask |= (uint) (str[start + len - 1].GetHashCode() & ep);
                    }
                    else
                    {
                        bitmask <<= 8;
                        bitmask |= (uint) (str[start + len - 1].GetHashCode() & ep);
                        //仅保留最后
                        bitmask &= ~(((~0) << (len * 8)));
                        
                    }
                }
                // compute hash of the first sequence in O(L) time
                else {
                    for(var i = 0; i < len; ++i)
                    {
                        bitmask <<= 8;
                        bitmask |= (uint) (str[i].GetHashCode() & ep);
                    }
                }

               
              
                // update output and hashset of seen sequences
                yield return bitmask;
            }
            
        }


        
    }
}