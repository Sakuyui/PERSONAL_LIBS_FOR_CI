using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CIExam.FunctionExtension;
using CIExam.Math;

namespace CIExam.Practice
{
  
    
    
    public class Knapsack
    {
        public bool CanPartition(int[] nums) {
            var n = nums.Length;
            var s = nums.Sum();
            if(s % 2 != 0)
                return false;
            var c = s / 2;
            var dp = new bool[c + 1];
            dp[0] = true;
            for(var i = 0; i < n; i++){
                for(var j = c; j >= nums[i]; j--){
                    dp[j] = dp[j] | dp[j - nums[i]];
                }
            }
            return dp[c];
        }

        
        public static bool IsCanGetSum(IEnumerable<int> input, int sum)
        {
            var data = input.ToArray();
            var n = data.Length;
            var dp = new int[n + 1, sum + 1]; //[i, j] => 用前i种相加如果能得到j的话，第i个最多能剩多少
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j < sum; j++)
                {
                    if (j < data[i - 1] || dp[i, j - data[i - 1]] <= 0)
                        dp[i, j] = -1;
                    else if (dp[i - 1, j] > 0) //前面就够用
                        dp[i, j] = data[i - 1];
                    else
                        dp[i, j] = dp[i, j - data[i - 1]] - 1; //使用一个数
                }
            }

            return dp[n, sum] > 0;
        }
        public static List<int> PartitionTwoSimilarSumArray(int[] array) {
            var sum2 = array.Sum();
            var result = new int[array.Length + 1].Select(_ => new int[array.Sum() / 2 + 1]).ToArray();
            var len = array.Length;
            sum2 >>= 1;
            var res = new List<int>();
            int j;
            for (var i = 1; i < len; i++) {
                for(j = 0; j <= sum2; j++){
                    if(j >= array[i - 1]){
                        result[i][j] = System.Math.Max(result[i - 1][j], result[i - 1][j - array[i - 1]] + array[i - 1]);
                    }else
                        result[i][j] = result[i - 1][j];
                }
            }
            j = sum2;
            for(var i= len - 1; i > 0; i--)
            {
                if (result[i][j] <= result[i - 1][j]) 
                    continue; // 找到第一个接近 sum/2 的，然后与 它上面的比较，如果大于，则代表当前 i 被选中
                res.Add(array[i]);
                j -= array[i];
            }

            return res;
        }

        /*
         *int rotating_calipers(point pnt[],int n){
    int q=1,ans=0,i;
    for(i=0;i<n;i++){
        while( (pnt[(i+1)%n]-pnt[i])*(pnt[(q+1)%n]-pnt[i])>(pnt[(i+1)%n]-pnt[i])*(pnt[q]-pnt[i]))q=(q+1)%n;
        ans=f_max(ans , f_max((pnt[i]-pnt[q]).len2(),(pnt[(i+1)%n]-pnt[(q+1)%n]).len2()));
    }
    return ans;
}
         * 
         */
        //分割等和子集，已测试
        public IEnumerable<IEnumerable<int>> PartitionTwoEqualSubset_OneDim(int[] nums) {
            var n = nums.Length;
            var s = nums.Sum();
            if(s % 2 != 0)
                return ArraySegment<IEnumerable<int>>.Empty;
            var c = s / 2;
            var dp = new bool[c + 1];
            var sel = new int[c + 1];
            dp[0] = true;
            for(var i = 0; i < n; i++){
                for(var j = c; j >= nums[i]; j--){
                    dp[j] = dp[j] | dp[j - nums[i]];
                    if (dp[j - nums[i]])
                        sel[j] = nums[i];
                }
            }
            if(!dp[c])
                return ArraySegment<IEnumerable<int>>.Empty;
            var res = new List<int>();
            while (c != 0)
            {
                res.Add(sel[c]);
                c -= sel[c];
            }

            var res2 = new List<int>(nums);
            res.ElementInvoke(e => res2.Remove(e));
            return new []{res, res2};
        }

        /*common 01*/
        /*状态压缩 去掉第1维的解法*/
        public int KnapsackCommon01_dim1_01(int[] w, int[] v, int c)
        {
            var n = w.Length;
            var dp = new int[c + 1];
            for (var i = 0; i < n; i++)
            {
                for (var j = c; j >= w[i]; j--) //重点，从大到小
                {
                    dp[j] = System.Math.Max(dp[j], dp[j - w[i]] + v[i]);
                }
            }

            return dp[c];
        }
       
        
        /*状正常方法*/
        public int KnapsackCommon01_dim2(int[] w, int[] v, int c)
        {
            var n = w.Length;
            var dp = new int[n + 1, c + 1];
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= c; j++)
                {
                    if (j - v[i] >= 0)
                        dp[i, j] = dp[i - 1, j];
                    else
                        dp[i, j] = System.Math.Max(dp[i - 1, j], dp[i, j - w[i - 1]] + v[i - 1]);
                    
                }
            }
            return dp[n, c];
        }
        
        /*完全背包*/ /*部分背包的话解法也差不多*/
        public int KnapsackCommonFull_dim2(int[] w, int[] v, int c)
        {
            var n = w.Length;
            var dp = new int[n + 1, c + 1];
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= c; j++)
                {
                    dp[i, j] = dp[i - 1, j];
                    //多一层循环，物品能够任意取
                    for (var k = c - v[i]; k > 0; k -= w[i])
                    {
                        dp[i, j] = System.Math.Max(dp[i, k] + v[i - 1], dp[i, j]);
                    }

                }
            }
            return dp[n, c];
        }
        
        public int KnapsackCommonFull_dim2_2(int[] w, int[] v, int c)
        {
            var n = w.Length;
            var dp = new int[c + 1];
            for (var i = 0; i < n; i++)
            {
                for (var j = w[i]; j <= c; j++)
                {
                    dp[j] = System.Math.Max(dp[j], dp[j - w[i]] + v[i] );

                }
            }
            return dp[c];
        }
        
        
        //换零钱的最少数量
        public int CoinChange(int[] coins, int target) {
            var dp = new int[target + 1].Select(_ => int.MaxValue).ToArray();
            dp[0] = 0;
        
            for(var i = 1 ; i < target + 1; i++){
                foreach(var x in coins){
                    if(i - x >= 0 && dp[i - x] != int.MaxValue){
                        dp[i] = System.Math.Min(dp[i], dp[i - x] + 1);
                    }
                }
            }
            return dp[target] == int.MaxValue ? -1 : dp[target];
        }
        
        //能换出某个零钱的不同组合数。。这回需要注重顺序
        public int CoinChange2(int[] coins, int amount) {
            var dp = new int[amount + 1]; //铁板
            dp[0] = 1; //注意初始条件。能换出0的就1种。
            
            
            //注意，注重顺序的，物品遍历要在外层。否则一般都是物品在里层
            foreach (var coin in coins) {
                for(var x = coin; x < amount + 1; ++x) {
                    dp[x] += dp[x - coin];
                }
            }

            return dp[amount];
        }
        
        //划分数组和
        public IEnumerable<int> DivideArrayHalfSum(int[] nums)
        {
            var sum = nums.Sum();
            var n = nums.Length;
            var c = sum / 2 + (nums.Max() > sum / 2 ? sum : sum / 2 + nums.Max());
            var dp = new bool[n + 1].Select(e => new bool[c + 1]).ToArray();
            
            for (var i = 0; i <= n; i++)
                dp[i][0] = true;
            
            for (var i = 1; i <= n; i++)
            {
                for (var j = 0; j <= c; j++)
                {
                    if (j - nums[i - 1] >= 0)
                    {
                        dp[i][j] = dp[i - 1][j] ||  dp[i - 1][j - nums[i - 1]];
                    }
                }
            }
            BoolMatrix.FromMatrix(dp.ToMatrix2D()).PrintToConsole();
            var l = sum / 2;
            var r = sum / 2;
            while (l >= 0 && r <= c)
            {
                $"{l}, {r}".PrintToConsole();
                if (dp.Select(e => e[l]).Any(e => e))
                {
                    return dp.Select(e => e[l]).Select((e, i) => (e, i)).Where(e => e.e).Select(e => e.i);
                }

                if (dp.Select(e => e[r]).Any(e => e))
                {
                    return dp.Select(e => e[r]).Select((e, i) => (e, i)).Where(e => e.e).Select(e => e.i);
                }

                l--;
                r++;
            }

            return new List<int>();

        }

        /* 装箱问题，给定某个K和B。是否能用K个容量为B的箱子装下所有物品*/ /*可以贪心*/
        public static bool CanFillBox(int[] box, int[] goods)
        {
            return CheckBoxDfs(box, goods, 0);
        }

        public static bool CheckBoxDfs(int[] box, int[] goods, int @from)
        {
            var len = goods.Length;
            if (from == len)
                return true;
            for (var i = 0; i < box.Length; i++)
            {
                if (box[i] < goods[@from]) 
                    continue;
                box[i] -= goods[@from];
                if (CheckBoxDfs(box, goods, @from + 1))
                {
                    return true;
                }

                box[i] += goods[@from];
            }

            return false;
        }
       
        public static bool KSetSearch(List<int>[] groups, int[] groupCapacity, int row, int[] nums, int target) {
            //叶子
            if (row < 0)
            {
               
                return true;
            }
            //取物品
            var v = nums[row--];
    
            //分支
            for (var i = 0; i < groups.Length; i++) {
                //可否分支
                if (groupCapacity[i] + v <= target) {
                    groups[i].Add(v);
                    groupCapacity[i] += v;
                    //dfs
                    if (KSetSearch(groups, groupCapacity, row, nums, target)) return true;
                    groupCapacity[i] -= v;
                    groups[i].RemoveAt(groups[i].Count - 1);
                }
                //剪枝。= 0 表示，第i个组无法装下该元素。否则怎样都会 > 0
                if (groupCapacity[i] == 0) 
                    break;
            }
            return false;
        }

      

        
        /*K相等子集*/
        public IEnumerable<IEnumerable<int>> CanPartitionKSubsets(int[] nums, int k)
        {
            var sum = nums.Sum();
            if (sum % k > 0) return ArraySegment<IEnumerable<int>>.Empty; //剪枝，无法整除k。
            var target = sum / k;

            nums = nums.OrderBy(e => e).ToArray();
            //从大到小塞
            var row = nums.Length - 1; //从后
            if (nums[row] > target) 
                return ArraySegment<IEnumerable<int>>.Empty; //剪枝，最大项比目标和还大

            var res = new List<List<int>>();
            //剪枝，排除元素恰好等于目标和的情况（一个元素恰好塞满一个背包），进行下一个元素，容器也减1
            while (row >= 0 && nums[row] == target) {
                row--;
                k--;
                res.Add(new List<int>{target});
            }

            var groups = new int[k].Select(_ => new List<int>()).ToArray();
            var b = KSetSearch(groups, new int[k], row, nums, target);
            
            //dfs判断， 提供目前还剩的容器数量大小的数组，目前在物品上的指针到达了哪（下次该从哪拿），数组，目标
            return res.Concat(groups);
        }
    }
}