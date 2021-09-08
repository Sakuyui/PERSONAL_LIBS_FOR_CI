using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CIExam.Structure;

namespace CIExam.FunctionExtension
{
    public static class LinqExt
    {
        public static IEnumerable<(T element, int index)> ExtendIndex<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Select((e, i) => (e, i));
        }
        
        public static IEnumerable<ValueTupleSlim> ExtendIndex2D<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            var d1 = enumerable.SelectMany((e, i) => 
                e.Select((col, j) => new ValueTupleSlim(i, j, col))
                );
            return d1;
        }

        public static string ConvertToString(this IEnumerable<char> c)
        {
            return new(c.ToArray());
        }

        public static void RemoveKeys<TKey, TVal>(this Dictionary<TKey, TVal> dictionary,
            Func<TKey, bool> pairJudge)
        {
            var keys = dictionary.Keys.ToList();
            foreach (var k in keys.Where(pairJudge.Invoke))
            {
                dictionary.Remove(k);
            }
        }

        public static IEnumerable<T> SubSequence<T>(this IEnumerable<T> collection, int from, int count)
        {
            return collection.Skip(from).Take(count);
        }

        public static int FindSubSequence<T>(this IEnumerable<T> collection, IEnumerable<T> sub)
        {
            var enumerable = sub as T[] ?? sub.ToArray();
            var ts = collection as T[] ?? collection.ToArray();
            if (enumerable.Length > ts.Length)
                return -1;
            var ans = ts.FindFirst((e, i) => ts.Skip(i).Take(enumerable.Length).SequenceEqual(enumerable));
            return ans.index;
        }

        public static int LastIndexOf<T>(this IEnumerable<T> s,T t)
        {
            var a = s.ToArray();
            for (var i = a.Length - 2; i >= 0; i--)
            {
                if (a[i].Equals(t))
                    return i;
            }

            return -1;
        }

        private static int[] ComputePrefix<T>(IEnumerable<T> pattern)
        {
            var p = pattern as T[] ?? pattern.ToArray();
            var m = p.Length;
            var pi = new int[m];
            pi[1]=0;
            var k =0;
            for(var q=2 ; q<m ; q++)
            {
                while(k > 0 && !p[k + 1].Equals(p[q]))
                    k = pi[k];
			
                if(p[k+1].Equals(p[q]))
                    k += 1;
                pi[q]=k;
            }
            return pi;
        }

        private const int NoOfChars = 256;

        //记录出现坏字符的位置
        private static Dictionary<T, int> BadTHeuristic<T>(IReadOnlyList<T> pattern, int size)
        {
            // Fill the actual value of last occurrence
            // of a character (indeces of table are ascii and values are index of occurence)
            var badChar = new Dictionary<T, int>();
            for (var i = 0; i < size; i++)
                badChar[pattern[i]] = i;
            return badChar;
        }

     
        public static int BoyerMooreSearch<T>(this IEnumerable<T> source,  T[] pattern)
        {
            var elements = source.ToArray();
            var m = pattern.Length;
            var n = elements.Length;
            
            /* Fill the bad character array by calling
               the preprocessing function badCharHeuristic()
               for given pattern */
            var bad = BadTHeuristic(pattern, m);
            var s = 0;  // s is shift of the pattern with
            // respect to text
            //there are n-m+1 potential allignments
            while(s <= n - m)
            {
                var j = m - 1;
 
                /* Keep reducing index j of pattern while
                   characters of pattern and text are
                   matching at this shift s */
                while(j >= 0 && pattern[j].Equals(elements[s + j]))
                    j--;
 
                /* If the pattern is present at current
                   shift, then index j will become -1 after
                   the above loop */
                if (j < 0)
                {
                    return s;
 
                    /* Shift the pattern so that the next
                       character in text aligns with the last
                       occurrence of it in pattern.
                       The condition s+m < n is necessary for
                       the case when pattern occurs at the end
                       of text */
                    //txt[s+m] is character after the pattern in text
                    //s += (s+m < n)? m-badchar[txt[s+m]] : 1;
 
                }

                s += System.Math.Max(1, j - (bad.ContainsKey(elements[s + j]) ? bad[elements[s + j]] : -1));
            }

            return -1;
        }
        public static string GetMultiDimensionString(this IEnumerable enumerable)
        {
            var s = "[";
            var count = 0;
            foreach (var e in enumerable)
            {
                if (e.GetType().GetInterface("IEnumerable") == null)
                {
                    s += e + "";
                }
                else
                {
                    s += GetMultiDimensionString((IEnumerable)e);
                }

                count++;
                s += " ,";
            }

            if (count != 0)
                s = s.Remove(s.Length - 1);
            s += "]";
            return s;
        }
        public static IEnumerable<string> FindAll(this string str, Regex regex)
        {
            var m = regex.Matches(str);
            return regex.Matches(str).Select(e => e.Value);

        }


        public static IEnumerable<T2> Cast<T, T2>(this IEnumerable<T> enumerable, Func<T, T2> castFunc)
        {
            return enumerable.Select(castFunc.Invoke);
        }
        public static int  BoyerMooreIndexOfSubPattern<T>(this IEnumerable<T> s, IEnumerable<T> p)
        {
            var sArr = s as T[] ?? s.ToArray();
            var pArr = p as T[] ?? p.ToArray();
            var n = sArr.Length; //元字符串长度
            var m = pArr.Length; //模式串长度
            
            //初始化两个坐标
            var i = m - 1; //source pointer
            var j = m - 1; //pattern pointer
            
            //源坐标小于字符串长度
            while (i < n)
            {
                //当前两个指针匹配成功
                if (pArr[j].Equals(sArr[i]))
                {
                    if  (j == 0) //已经匹配到头了，匹配成功，返回	
                        return  i;
                    j--;  i--;	//双指针左移

                }
                else //未匹配情况
                {
                    //寻找模式串中最后一个当前字符的位置
                    var last = pArr.LastIndexOf(sArr[i]);
                    if  (last < 0) 	// 不存在于匹配串的字符，直接跳过整个串
                        i += m;		
                    else
                        i = i + j - last;	// 重设源串指针
                    
                    j = m - 1;	// 重设模式串指针
                }
            }		
                
            return  -1;	//  not matched
        }

        //找出所有连续子序列
        public static List<List<T>> FindAllContinuousSubSequence<T>(this IEnumerable<T> enumerable, 
            Func<List<T>, bool> judgeFunc, Func<List<List<T>>,int, int, bool> terminateCondition = null)
        {
            var arr = enumerable.ToArray();
            var l = 0;
            var r = 0;
            var n = arr.Length;
            var ans = new List<List<T>>();
            var path = new List<T>();
            for (; l < n; l++)
            {
                path.Clear();
                r = l;
                for (var j = r; j < n; j++)
                {
                    if (terminateCondition != null && terminateCondition.Invoke(ans, l, r))
                        return ans;
                    path.Add(arr[r++]);
                    if (judgeFunc.Invoke(path))
                    {
                        ans.Add(new List<T>(path));
                    }
                }
                
            }

            return ans;
        } 
        
        public static List<List<T>> FindAllCombination<T>(this IEnumerable<T> enumerable, 
            Func<List<T>, bool> judgeFunc, Func<int, bool> skip = null)
        {
            var arr = enumerable.ToList();
            
           
            var n = arr.Count;
            var ans = new List<List<T>>();
            
            for (var i = 0; i < n; i++)
            {
                if(skip != null && skip.Invoke(i))
                    continue; ;
                var path = CollectionHelper.GetCombination(arr, i).Where(judgeFunc.Invoke);
                ans.AddRange(path);
                
            }

            return ans;
        } 
        
        
    }
}