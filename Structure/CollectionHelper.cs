using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam
{
    
    //HashBag: 能够存储的同时记录次数
    public static class CollectionHelper
    {


        public static void Test2()
        {
            var t = Enumerable.Range(0, 10).Select(e =>

                Enumerable.Range(0, 10).Select(e2 => (e, e2)).ToList()
            ).ToList();


            var t1 = Enumerable.Range(0, 10);
            var t2 = Enumerable.Range(0, 5);
            var t3 = (from e in t1 join i in t2 on 1 equals 1 select (e, i)).ToList();
            
            foreach (var x in t)
            {
                x.PrintEnumerationToConsole();
            }
            t3.PrintEnumerationToConsole();
        }
        public static List<List<T>> GetPermutation<T>(IEnumerable<T> enumerable, int k)
        {
            var ans = new List<List<T>>();
            Permutation(enumerable.ToList(), k, new bool[enumerable.Count()], new List<T>(), ans);
            return ans;
        }

        public static ulong SetToBitRepresent(IEnumerable<int> enumerable)
        {
            var ints = enumerable as int[] ?? enumerable.ToArray();
            var n = ints.Length;
            return n > 64 ? 0 : 
                ints.Aggregate((ulong) 0, (current, e) => current | (ulong) 1 << e);
        }
        public static List<List<T>> GetCombination<T>(IEnumerable<T> enumerable, int k)
        {
            var ans = new List<List<T>>();
            Combination(enumerable.ToList(), k, 0, new List<T>(), ans);
            return ans;
        }
        //组合
        public static void Combination<T>(List<T> list, int k, int start, List<T> path, List<List<T>> ans)
        {
            if (start > list.Count || k > list.Count) return;
            //判断叶子
            if (path.Count == k)
            {
                ans.Add(new List<T>(path));
                return;
            }
            
         
            
            //非叶子
            //所有未遍历的节点
            for (var i = start; i < list.Count; i++)
            {
                path.Add(list[i]);
                Combination<T>(list, k, i + 1, path, ans);
                path.RemoveAt(path.Count - 1);
            }

        }
        
        
        //排列
        public static void Permutation<T>(List<T> list, int k, bool[] vis, List<T> path, List<List<T>> ans) 
        {
            if(vis.Length != list.Count) throw new ArithmeticException();
            //判断叶子
            if (path.Count == k) //所有节点已经访问过
            {
                //Output
                ans.Add(new List<T>(path));
                return;
            }
            
            //非叶子
            //所有未遍历的节点
            for (var i = 0; i < vis.Length; i++)
            {
                if(vis[i]) continue;
                path.Add(list[i]);
                vis[i] = true;
                Permutation<T>(list, k, vis, path, ans);
                vis[i] = false;
                path.RemoveAt(path.Count - 1);
            }
        }
        
        public static IEnumerable<IEnumerable<T>> SplitCollection<T>(IEnumerable<T> collection, int sizePerGroup)
        {
            var enumerable = collection as T[] ?? collection.ToArray();
            var size = enumerable.Count();
            if (sizePerGroup >= size) return new[] {enumerable};
            var result = enumerable.Select((c, index) => new {index, c})
                .GroupBy(e => e.index /sizePerGroup)
                .Select(x => x.Select(p => p.c));
           
            return result;
        }
        
        //linq排序  var list = (from c in heap orderby c descending select c).ToList();
        
        public static void Test()
        {
            int[] nums = new []{1,2,3,4,5,6,7,8};
           
            var s = CollectionHelper.SplitCollection(nums,3);
            foreach (var t in s)
            {
                foreach(var e in t)
                {
                    Console.Write(e +" ");
                }
                Console.WriteLine();
            }
            var s2 = 
                from e1 in nums
                join e2 in nums
                    on true equals true
                select (e1, e2);
            s2.Distinct().Count().PrintToConsole();
            s2.Distinct().PrintEnumerationToConsole();
        }
        
        public static IList<string> TopKFrequent(string[] words, int k)
        {
            //写一个比较器函数
            
            
            
            
            //统计字符串频率并取出频率最高的k个字符串
           return  words.GroupBy(e => e) //按字符串分组 
                .Select(e => (name: e.Key, count: e.Count())) //计数
                //排序
                .OrderBy(e => e.name) 
                .ThenBy(e => e.count)
                //仅保留字符串
                .Select(e => e.name)
                //选择前k个
                .Take(k).ToList();
            
            
        }
        
        public static List<T> CreateListWithDefault<T>(int k, T data = default)
        {
            if (k < 0) return null;
            var list = new List<T>();
            for (var i = 0; i < k; i++)
            {
                list.Add(data);
            }
            return list;
        }
        
        

        public static List<List<T>> CreateTwoDimensionList<T>(T[] data, int row, int columns)
        {
            var k = data.Length;
            if(row * columns != k) throw new ArithmeticException();
            var list = new List<List<T>>();
            for (var i = 0; i < row; i++)
            {
                var l = new List<T>();
                for (var j = 0; j < columns; j++)
                {
                    l.Add(data[i * columns + j]);
                }
                list.Add(l);
            }
            return list;
        }
        public static List<List<T>> CreateTowDimensionList<T>(int rows, int columns, T data = default)
        {
            var list = new List<List<T>>();
            if (rows == 0 || columns == 0) return list;
            for (var i = 0; i < rows; i++)
            {
                list.Add(CreateListWithDefault(columns,data));
            }
            return list;
        }
    }
    
    
}