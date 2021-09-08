using System.Collections.Generic;
using C5;
using CIExam.Structure;

namespace CIExam.CI
{
    
    public class BlockFullA
    {
        // 压缩 + 暴力   class Solution {
        //     int[] heights;
        //
        //     public int query(int L, int R) {
        //         int ans = 0;
        //         for (int i = L; i <= R; i++) {
        //             ans = Math.max(ans, heights[i]);
        //         }
        //         return ans;
        //     }
        //
        //     public void update(int L, int R, int h) {
        //         for (int i = L; i <= R; i++) {
        //             heights[i] = Math.max(heights[i], h);
        //         }
        //     }
        //
        //     public List<Integer> fallingSquares(int[][] positions) {
        //         //Coordinate Compression
        //         //HashMap<Integer, Integer> index = ...;
        //         //int t = ...;
        //
        //         heights = new int[t];
        //         int best = 0;
        //         List<Integer> ans = new ArrayList();
        //
        //         for (int[] pos: positions) {
        //             int L = index.get(pos[0]);
        //             int R = index.get(pos[0] + pos[1] - 1);
        //             int h = query(L, R) + pos[1];
        //             update(L, R, h);
        //             best = Math.max(best, h);
        //             ans.add(best);
        //         }
        //         return ans;
        //     }
        // }

       
        
        public class Node
        {
            //左右高
            public int L;
            public int R;
            public int H;
            public Node Left;
            public Node Right;

            public Node(int l, int r, int h, Node left, Node right)
            {
                L = l;
                R = r;
                H = h;
                Left = left;
                Right = right;
            }
        }
        
        //p[0]方块左边界 p[1]方块宽
        public List<int> FallingSquares(int[][] positions)
        {

            // 创建返回值
            var res = new List<int>();
            // 根节点，默认为零
            Node root = null;
            // 目前最高的高度
            var maxH = 0;

            foreach (var position in positions) {
                //新进入一个方块
                var l = position[0]; // 左横坐标
                var r = position[0] + position[1]; // 右横坐标
                var e = position[1]; // 边长
                
                //我们现在想在 横坐标l,r区间内加一个高度为e的方块。
                //需要知道当前区间最高才能决定放到哪。
                var curH = Query(root, l, r); // 目前区间的最高的高度
                root = Insert(root, l, r, curH + e); //更新线段树
                maxH = System.Math.Max(maxH, curH + e); //更新最高高度
                res.Add(maxH);
            }
            return res;
        }

        private static Node Insert(Node root, int l, int r, int h) {
            if (root == null)//如果还不存在节点。那就直接创建一个根节点。 
                return new Node(l, r, h, null, null);
            if (l <= root.L) //按照左边界大小左小右大地建树。
                root.Left = Insert(root.Left, l, r, h);
            else
                root.Right = Insert(root.Right, l, r, h);
            return root; // 返回建立好的节点。
        }

        private static int Query(Node root, int l, int r) {
            if (root == null) return 0;
            
            // 高度
            var curH = 0;
            if (!(r <= root.L || root.R <= l)) // 是否跟这个节点相交 如果非不相交的话就记录高
                curH = root.H;
            // 未剪枝，递归地获取最大高度
            // curH = System.Math.Max(curH, Query(root.Left, l, r));
            // curH = System.Math.Max(curH, Query(root.Right, l, r));
            
            // 剪枝
            curH = System.Math.Max(curH, Query(root.Left, l, r));
            if (r > root.L)
                curH = System.Math.Max(curH, Query(root.Right, l, r));

           
            return curH;
        }
    }
}