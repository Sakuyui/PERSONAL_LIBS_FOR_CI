using System.Collections.Generic;
using CIExam.Geometry;
using CIExam.Math;

namespace CIExam.Praticle.TextBook
{
    public class Search
    {
        public int BinarySearch(int[] nums, int t)
        {
            var l = 0;
            var r = nums.Length - 1;
            while (l <= r)
            {
                var mid = (l + r) >> 1;
                if (nums[mid] == t)
                    return mid;
                if (nums[mid] < t)
                {
                    l = mid + 1;
                }
                else
                {
                    r = mid - 1;
                }
            }
            return -1;
        }



       
        
        
        public int BinarySearchLeftEdge(int[] nums, int t)
        {
            var l = 0;
            var r = nums.Length;
            while (l < r)
            {
                var mid = (l + r) >> 1;
                if (nums[mid] >= t)
                {
                    r = mid;
                }
                else if (nums[mid] < t)
                {
                    l = mid + 1;
                }
            }
            return l;
        }

        public int BinarySearchRightEdge(int[] nums, int t)
        {
            var l = 0;
            var r = nums.Length;
            while (l < r)
            {
                var mid = (l + r) >> 1;
                if (nums[mid] <= t)
                {
                    r = mid + 1;
                }
                else
                {
                    r = mid - 1;
                }
            }
            return r - 1;
        }
        
        public List<List<int>> KSum(List<int> nums, int target, int k) {
            if (nums == null || nums.Count < k) return new List<List<int>>();
            nums.Sort();
            List<List<int>> res = new ();
            List<int> ans = new ();
            
            void Help2Sum (int targetNum, int cur) {
                var lo = cur;
                var hi = nums.Count - 1;
                while (lo < hi) {
                    var sum = nums[lo] + nums[hi];
                    if (sum < targetNum) {
                        lo++;
                    } else if (sum > targetNum) {
                        hi--;
                    } else {
                        ans.Add(nums[lo]); ans.Add(nums[hi]);
                        res.Add(new List<int>(ans));
                        ans.RemoveAt(ans.Count - 1); 
                        ans.RemoveAt(ans.Count - 1);
                        while (lo < hi && nums[lo + 1] == nums[lo++]) ;
                        while (lo < hi && nums[hi - 1] == nums[hi--]) ;
                    }
                }
            }
            void Helper(int targetNum, int cur, int kSum) {
                if (targetNum < kSum * nums[cur] || targetNum > kSum * nums[^1]) return ;  // 不可能存在相应的kSum数组
                if (kSum == 2) Help2Sum( targetNum, cur);
                else {
                    for (int i = cur, len = nums.Count; i <= len - kSum; i++) {
                        if (i > cur && nums[i - 1] == nums[i]) continue;  // 去重
                        ans.Add(nums[i]);
                        Helper(targetNum - nums[i],  i + 1, kSum - 1);  // 回溯
                        ans.RemoveAt(ans.Count - 1);
                    }
                }
            }
            
            
            Helper(target, 0, k);  
            return res;
        }
        
    

        
        
        
        //是否存在合为m的子集。回溯
        public void FindSumKSubset(int[] nums, int k,int d = 0, List<int> path = null, List<bool> used = null)
        {
            if (k == 0 || d == nums.Length)
                return;
            if (path == null)
                path = new List<int>();
            if (used == null)
                used = new List<bool>();
            var n = nums.Length;
            for (var i = d; i < n && !used[i]; i++)
            {
                path.Add(i);
                used[i] = true;
                FindSumKSubset(nums, d + 1,k - nums[i], path, used);
                used[i] = false;
                path.Remove(path.Count - 1);
            }
        }
        
        //科赫曲线
        public void Koch(int n, Point2D p1, Point2D p2)
        {
            if(n == 0)
                return;
            //找出3个点
            Point2D s1, s2, s3;
            s1 = p1 + (p2 - p1) / 3;
            s2 = p1 + (p2 - p1) * 2 / 3;
            s3 = s1 + ((s2 - s1).Insert(1) * TransFormUtil.Rotation2D(60));

            //output
            Koch(n - 1, p1, s1);
            //output
            Koch(n - 1, s1, s3);
            //output
            Koch(n - 1, s3, s2);
            //output
            Koch(n - 1,s2 ,p2);
        }
    }
}