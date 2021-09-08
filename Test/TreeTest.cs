using System;
using System.Collections.Generic;
using System.Linq;

using IntervalTree;
using JJ.Framework.Collections;
using KdTree;
using KdTree.Math;
using CIExam.FunctionExtension;

namespace CIExam.Structure
{
    public class KdTreeTest
    {
        public static void Test()
        {
            var tree = new KdTree<double, int>(3, new DoubleMath());
            tree.Add(new[] {1.0, 2.0, 3.0}, 4);
            tree.Add(new[] {1.0, 2.0, 3.0}, 7);
            tree.Add(new[] {2.0, 4.0, 3.0}, 5);
            tree.Add(new[] {5.0, 2.0, 3.0}, 4);
            tree.PrintCollectionToConsole();
            tree.GetNearestNeighbours(new[] {1.0, 1.0}, 3);
            //range tree
            //key val
            var intervalTree = new IntervalTree<int, string>()
            {
                { 0, 10, "1" },
                { 20, 30, "2" },
                { 15, 17, "3" },
                { 25, 35, "4" },
            };
            

            var results1 = intervalTree.Query(5);     // 1 item: [0 - 10]
            var results2 = intervalTree.Query(10);    // 1 item: [0 - 10]
            var results3 = intervalTree.Query(29);    // 2 items: [20 - 30], [25 - 35]
            var results4 = intervalTree.Query(5, 15); // 2 items: [0 - 10], [15 - 17]
            
            
        }
    }

    

    
    public class SegmentTreeExample
    {

        public static int FindNumberOfLis(int[] nums)
        {
            // if (nums.Length == 0) return 0;
            // var min = nums.Min();
            // var max = nums.Max();
            // //Lis的长度作为key。个数是val
            // var segmentTree = new SegmentTree<(int len, int count)>(
            //     delegate((int len, int count) t1, (int len, int count) t2)
            //     {
            //         if (t1.len == t2.len)
            //         {
            //             //左边没有找到符合
            //             return t1.len == 0 ? (0, 1) : (t1.len, t1.count + t2.count);
            //             //那结果就是两者相加
            //         }
            //         return t1.len > t2.len ? t1 : t2;
            //     });
            //
            // segmentTree.Init(new SegmentTreeNode<(int len, int count)>(min, max));
            //
            // foreach (var num in nums)
            // {
            //     //找到以[min,num-1]范围为尾节点的子序列的，最大长度L，以及个数count————步骤3
            //     var (len, c) = segmentTree.RightRangeQuery(segmentTree.Root, num - 1);
            //     //那么[min,num]区间的值就为[min,num-1]的长度+1，个数继承，插入到表里去————步骤4
            //     //如果节点本身存在，那么就是更新，不存在，那么就是新增咯————步骤4
            //     segmentTree.Insert(segmentTree.Root, num, (len + 1, c));
            // }
            //
            // //返回[min,max]的区间的最终结果
            // return segmentTree.Root.Val.count;
            throw new NotImplementedException();
        }


        public static void Test2()
        {
            // var set = new HashSet<int>(new[] {2, 4, 5, 6, 24, 11, 9});
            // var root = new SegmentTree<int>(
            //     mergeStrategy: (i, i1) => i + i1
            // );
            // root.Init(new SegmentTreeNode<int>(0, 30));
            // foreach (var num in set)
            // {
            //     root.Insert(root.Root, num, 1);
            // }
            //
            // //这就是求<=26的key个数惹
            //
            // root.RightRangeQuery(root.Root, 26).PrintToConsole();
            // var node = new RedBlackTree<int, int>();
            // node.Insert(1, 2);
            //
            throw new NotImplementedException();
        }
        
    }
}