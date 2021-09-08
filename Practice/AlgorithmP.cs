using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.Structure;

//实现堆，优先队列

namespace CIExam
{
    public class AlgorithmP
    {

        
        public bool DecompressRLElist(string s)
        {
            var t = s.Trim().Replace(" ", "").ToUpper();
            return t.SequenceEqual(t.Reverse());
           
        }
        public static T[] GetTopKFreq<T>(IEnumerable<T> enumerable, int k)
        {
            var ints = enumerable as T[] ?? enumerable.ToArray();
            var n = ints.Count();
            if (k > n)
                return null;
            return ints
                .GroupBy(e => e)
                .Select(g => (element: g.Key, count: g.Count()))
                .OrderByDescending(e => e.count)
                .ThenBy(e => e.element)
                .Select(e => e.element)
                .Take(k)
                .ToArray();
        }
        
        public static void GetTopK<T>(IEnumerable<T> arr, int k) where T:IComparable
        {
            var heap = new IntervalHeap<T>();
            
            foreach (var e in arr)
            {
                if (heap.Count < k)
                {
                    heap.Add(e);
                }
                else
                {
                    if (e.CompareTo(heap.FindMin()) > 0)
                    {
                        heap.DeleteMin();
                        heap.Add(e);
                    }
                }
            }

            //Linq排序~!!!!
            var list = (from c in heap orderby c descending select c).ToList();
            Console.WriteLine(Utils.ListToString(list));
            
        }

        

        
        
        
   
        
        //背包问题的三种解法

        public static int BagDynamicProgramming (int[] w, int[] v, int capacity)
        {
            //dp[i][w]表示前i个物品在w的情况下能够获得的最大价值
            var dp = new int[w.Length+ 1, capacity + 1];
            //dp[i,j] = max{dp[i-1, j], dp[j, j - w[i]] + v[i]}
            var ans = Int32.MinValue;
            for (var i = 1; i <= w.Length; i++)
            {
                for (var j = 1; j <= capacity; j++){
                    dp[i, j] = ( j - w[i] < 0 ? dp[i - 1, j] : 
                        System.Math.Max(dp[i - 1, j], dp[i, j - w[i]] + v[i]));
                    ans = (dp[i, j] > ans ? dp[i, j] : ans);
                }
            }

            return ans;
        }

        public static int BagReturn(int[] w, int[] v, int capacity, bool[] vis, int curMax)
        {
            //回溯法第一步：判断是否到叶子节点
            var max = curMax;
            var flag = w.Where((t, i) => !vis[i] && t < capacity).Any();  //如果还存在未遍历过的
            
            if (!flag) return max; //到达叶子,不存在任何没遍历过的了
            
            //遍历未遍历的子树
            for (var i = 0; i < w.Length; i++)
            {
                if (vis[i] || w[i] > capacity) continue; //找到一个未访问过的元素
                //找到一个未遍历的子节点
                vis[i] = true; //标记
                //开始向下搜索惹
                max =  System.Math.Max(BagReturn(w, v, capacity - w[i], vis, max), max);
                vis[i] = false; //清除标记
            }

            return max;
        }
        
        public static int BagBranchLimit(int[] w, int[] v, int capacity, int curMax)
        {
            int[] indexes = new int[w.Length];
            
            for (int i = 0; i < w.Length; i++)
            {
                //对于每个物品有选择或者不选择
                
            }

            return 0;
        }


        
        //二分搜索模板
        public static int BinarySearch(int[] nums, int target)
        {
            var left = 0;
            var right = nums.Length - 1;
            while (left <= right)
            {
                var mid = (right - left) / 2 + left;
                if (nums[mid] == target)
                {
                    return mid;
                }
                else if(nums[mid] < target)
                {
                    left = mid + 1;
                }else if (nums[mid] > target)
                {
                    right = mid - 1;
                }
            }

            return -1;
        }

        public static int BoundLeftBinarySearch(int[] nums, int target)
        {
            var left = 0;
            var right = nums.Length; //使用开区间
            while (left < right) //右闭区间需要用<
            {
                var mid = left + (right - left) / 2;
                if (nums[mid] == target)
                {
                    right = mid;
                }else if (nums[mid] < target)
                {
                    left = mid + 1;
                }else if (nums[mid] > target)
                {
                    right = mid;
                }
            }

            return left;
        }



       
    }
}