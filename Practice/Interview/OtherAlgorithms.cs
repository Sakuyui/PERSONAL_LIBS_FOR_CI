using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.FunctionExtension;
using CIExam.OS.Memory;
using CIExam.Structure;
using CIExam.Structure.Graph;

namespace CIExam.Practice.Interview
{
    public class OtherAlgorithms
    {
        
        public class SortAlgorithm
        {
            public static List<int> KMergeSort(List<List<int>> list)
            {
                var k = list.Count;
                var finish = new bool[k];
                var heapList = Enumerable.Repeat(0, k).Select(_ => new IntervalHeap<int>());
                var loadTimes = 0;
                const int bufferSize = 5;
                var buffer =  Enumerable.Repeat(0, k).Select(_ => (buffer: new List<int>(), rest: k)).ToArray();

                bool LoadData(int index)
                {
                    if (finish[index])
                        return false;
                    var l = list[index];
                    if (l.Any())
                    {
                        var r = buffer[index].rest;
                        var begin = bufferSize - r;
                        if (r >= l.Count)
                        {
                            for (var i = 0; i < l.Count; i++)
                            {
                                buffer[index].buffer[begin + i] = l[i];
                            }
                            l.Clear();
                            buffer[index].rest = 0;
                            loadTimes++;
                            finish[index] = true;
                            return true;
                        }

                        for (var i = 0; i < r; i++)
                        {
                            buffer[index].buffer[begin + i] = l[i];
                        }
                        l.RemoveRange(0, r);
                        loadTimes++;
                        buffer[index].rest = 0;
                        return true;

                    }

                    finish[index] = true;
                    return false;
                }

                var output = new List<int>();
                while (finish.Any(e => !e))
                {
                    //首先看看有没有空的缓冲
                    Enumerable.Range(0, k).ElementInvoke(index =>
                    {
                        if (buffer[index].rest == 0) LoadData(index);
                    });
                    
                    //接下来是归并过程，可以用排序，也可以用锦标赛树
                    //锦标赛树
                    var curLayer = Enumerable.Range(0, k)
                        .Select((e, i) => (val: finish[i] ? int.MaxValue : e, index: i)).ToArray();
                    while (curLayer.Length > 1)
                    {
                        curLayer = curLayer.GroupByCount(2)
                            .Select(l => l.Count == 1? l[0] : l[0].val < l[1].val ? l[0] : l[1]).ToArray();
                    }
                    output.Add(curLayer.ToArray()[0].val);
                    var selIndex = curLayer.ToArray()[0].index;
                    buffer[selIndex].rest++;
                    buffer[selIndex].buffer.RemoveAt(0);
                    
                }

               

                void HeapSortModel()
                {
                    var heap = new IntervalHeap<(int val, int index)>(new CustomerComparer<(int val, int index)>
                        ((t1, t2) => t1.val.CompareTo(t2.val)));
                    while (finish.Any(e => !e))
                    {
                        while (heap.Any())
                        {
                            var (val, index) = heap.DeleteMin();
                            buffer[index].rest++;
                            buffer[index].buffer.RemoveAt(0);
                            output.Add(val);
                        }
                        Enumerable.Range(0, k).ElementInvoke(e => LoadData(e));
                        for (var i = 0; i < k; i++)
                        {
                            if (!finish[i])
                            {
                                heap.Add((buffer[i].buffer.First(), i));
                            }
                        }
                    }
                }
                return output;
            }
        }
        public static int GenerateRand1To7()
        {
        
            int GetRand() => new Random().Next(1, 5);
            return ((GetRand() - 1) * 5 + (GetRand() - 1)) % 7;
        }
        
        /*
         *  public int countSubstrings(String s) {
        int n = s.length();
        StringBuffer t = new StringBuffer("$#");
        for (int i = 0; i < n; ++i) {
            t.append(s.charAt(i));
            t.append('#');
        }
        n = t.length();
        t.append('!');

        int[] f = new int[n];
        int iMax = 0, rMax = 0, ans = 0;
        for (int i = 1; i < n; ++i) {
            // 初始化 f[i]
            f[i] = i <= rMax ? Math.min(rMax - i + 1, f[2 * iMax - i]) : 1;
            // 中心拓展
            while (t.charAt(i + f[i]) == t.charAt(i - f[i])) {
                ++f[i];
            }
            // 动态维护 iMax 和 rMax
            if (i + f[i] - 1 > rMax) {
                iMax = i;
                rMax = i + f[i] - 1;
            }
            // 统计答案, 当前贡献为 (f[i] - 1) / 2 上取整
            ans += f[i] / 2;
        }

        return ans;
    }

         */
        
        
        public static int ManchesterPalindrome(string s)
        {
            var ps = "$#";
            foreach(var c in s)
            {
                ps += c;
                ps += "#";
            }
            ps += '\0';

            var p = new List<int>(ps.Length);
            var id = 0;
            var mx = 0;
            var maxLength = 0;

            for(var i = 1 ; i < ps.Length ; i++)
            {
                p[i] = mx > i ? System.Math.Min(p[2 * id - i], mx - i) : 1;
                while(ps[i + p[i]] == ps[i - p[i]])
                    p[i]++;

                if(i + p[i] > mx)
                {
                    id = i;
                    mx = i + p[i];
                }

                if(p[i] - 1 > maxLength)
                {
                    maxLength = p[i] -1;
                }
            }
            return maxLength;
        }

        
        //divide surge problem
        //input score
        //regular: 1.每人至少一个 2.相邻的分多糖果一定多 3.相邻的得分一样，一定有相同糖果

        public int[] DivideSurge(int[] score)
        {
            if (score.Length == 1)
                return new []{1};
            var pt = 0;
            var divide = new int[score.Length];
            while (pt < score.Length - 1)
            {
                 //引入左坡与右坡
                (int s, int e) leftP = (pt, pt);
                (int s, int e) rightP = (pt, pt - 1);
                var distinctLeft = 1;
                var prev = score[pt];
                for (var i = 1; i < score.Length; i++)
                {
                    if (score[i] >= prev)
                    {
                        leftP.e++;
                        pt++;
                        if (score[i] != prev)
                            distinctLeft++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (pt + 1 < score.Length)
                {
                    rightP.s = pt + 1;
                    rightP.e = pt + 1;
                    prev = score[pt + 1];
                    for (var i = pt + 2; i < score.Length; i++)
                    {
                        if (score[i] < prev)
                        {
                            pt++;
                            rightP.e++;
                        }
                    }
                }
                //divide
                var curSurge = System.Math.Max(distinctLeft, rightP.e - rightP.s + 1);
                divide[leftP.e] = curSurge--;
                prev = score[leftP.e];
                for (var i = leftP.e - 1; i >= 0; i--)
                {
                    divide[i] = curSurge;
                    if (score[i] == prev) continue;
                    prev = score[i];
                    curSurge--;
                }
                //right
                if (rightP.e >= rightP.s)
                {
                    curSurge = System.Math.Max(distinctLeft, rightP.e - rightP.s + 1) - 1;
                    divide[rightP.s] = curSurge--;
                    prev = score[rightP.s];
                    for (var i = rightP.s + 1; i <= rightP.e; i++)
                    {
                        divide[i] = curSurge;
                        if (score[i] == prev) continue;
                        prev = score[i];
                        curSurge--;
                    }
                }
            }
           

            return divide;

        }
        
        
    }
}