using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.os.Cache
{
    public static class OsAlgorithm
    {
        public static List<int> BankerAlgorithm(IEnumerable<List<int>> processMaxNeeds, IEnumerable<List<int>> processAllocated, int[] resourceRest)
        {
            var ans = new List<int>();
            var allocated = processAllocated.ToMatrix2D();
            var maxNeed = processMaxNeeds.ToMatrix2D();
            var rest = resourceRest.ToList().ToVector();
            var n = allocated.RowsCount; //进程数
            var finish = new bool[n];
            while (finish.Any(e => !e))
            {
                var t = maxNeed - allocated;
                var f = t.FindFirst((l, i) => l.All(e => (int) e == 0) && !finish[i]);
                if (f.index >= 0)
                {
                    finish[f.index] = true;
                    ans.Add(f.index);
                }
                else
                {
                    var rest1 = rest;
                    var (index, data) = t.FindFirst((l, i) =>
                        (rest1 - l.Cast(e => (int) e).ToVector()).All(e => (int) e >= 0) && !finish[i]
                    );
                    if (index < 0)
                        return null; //不安全
                    //分配
                    var diff = data.Cast(e => (int)e);
                    rest += (dynamic)allocated[f.index].ToVector();
                    allocated[f.index] = maxNeed[f.index];
                }
            }
            return ans;
        }

        public static int GetWorksetMissTimes(IEnumerable<int> pageIndex, int winSize)
        {
            var enumerable = pageIndex as int[] ?? pageIndex.ToArray();
            if (winSize > enumerable.Length)
                return pageIndex.Distinct().Count();
            var cnt = winSize;
            var q = new Queue<int>(enumerable.Take(winSize).Distinct());
            for (var r = winSize; r < enumerable.Length; r++)
            {
                if (q.Contains(enumerable[r]))
                {
                    q.Dequeue();
                    q.Enqueue(enumerable[r]);
                    continue;
                }
                
                cnt++;
                q.Dequeue();
                q.Enqueue(enumerable[r]);
               
                
            }
            return cnt;
        }
        public static int GetMaxWorkSetLength(IEnumerable<int> pageIndex, int delta)
        {
            return pageIndex.WindowSelect(delta, win => win.Distinct().Count()).Max();
            
            // var maxLen = pageIndex.Distinct().Count();
            // var l = 0;
            // var r = maxLen;
            // while (l <= r)
            // {
            //     var m = ((r - l) >> 1) + l;
            //     var winR = m - 1;
            //     var winL = 0;
            //     var rest = m - pageIndex.Take(m).Distinct().Count();
            //     var q = new Queue<int>(pageIndex.Take(m).Distinct());
            //     for (var i = 0; i < winR; i++)
            //     {
            //         if (q.Contains(pageIndex[i]))
            //         {
            //             winL++;
            //             winR++;
            //             q.Dequeue();
            //             q.Enqueue(pageIndex[i]);
            //         }
            //         else
            //         {
            //             if (rest == 0)
            //             {
            //                 l = m + 1;
            //                 break;
            //             }
            //
            //             winR++;
            //             winL++;
            //             var f = q.Dequeue();
            //             q.Enqueue(pageIndex[i]);
            //         }
            //     }
            //
            //     r = m - 1;
            // }
            //
            // return l + 1;

        }
    }
}