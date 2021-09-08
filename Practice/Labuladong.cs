using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.FunctionExtension;
using CIExam.Math;

namespace CIExam.Praticle
{
    public class Labuladong
    {
        /*
         * class Solution {
    //dijstra算法
    class Node{
        int col;
        int row;
        int val;
    }
    
    public int maximumMinimumPath(int[][] A) {
        int m = A.length;
        int n = A[0].length;
        PriorityQueue<Node> heap = new PriorityQueue<Node>((Node o1,Node o2)->(o2.val-o1.val));
        boolean[][] mark = new boolean[m][n];
        int[][] direction = {{-1,0},{1,0},{0,-1},{0,1}};
        // 最短路径,每次取
        Node n0 = new Node();
        n0.col = 0;n0.row = 0;n0.val = A[0][0];
        heap.add(n0);
        mark[0][0] = true;
        int res = A[0][0];
        while(!heap.isEmpty()){
            Node cur = heap.poll();
           // System.out.println(cur.val);
            res = Math.min(cur.val,res);
            if(cur.col==n-1&&cur.row==m-1){return res;}
            for(int k=0;k<4;k++){
                int nx = cur.row+direction[k][0];
                int ny = cur.col+direction[k][1];
                if(nx>=0&&ny>=0&&nx<m&&ny<n&&!mark[nx][ny]){
                    Node n1 = new Node();
                    n1.row = nx;
                    n1.col = ny;
                    n1.val = A[nx][ny];
                    heap.add(n1);
                    mark[nx][ny] = true;
                }
            }
        }
        return -1;
    }
}
         */
        //bfs 寻找迷宫最短路径（各cost相等。
        
        
        public class Solution {
            // public int Lcs(string text1, string text2) {
            //     var n1 = text1.Length;
            //     var n2 = text2.Length;
            //     var dp = new int[n2 + 1];
            //
            //     for(var i = 1; i <= n1; i++){
            //         var pre = dp[0];
            //
            //         for(var j = 1; j <= n2; j++){
            //             var tmp = dp[j];
            //             if(text1[i - 1] == text2[j - 1]){
            //                 dp[j] = pre + 1;
            //             }else{
            //                 dp[j] = System.Math.Max(dp[j], dp[j - 1]);
            //             }
            //             pre = tmp;
            //         }
            //     }
            //     return dp[n2];
            // }
        }
        public int NthSuperUglyNumber(int n, int[] primes) {
            IntervalHeap<long> queue=new ();
            long res=1;
            for(var i=1;i<n;i++){
                foreach(int prime in primes){
                    queue.Add(prime * res);
                }
                res = queue.DeleteMin();
                while(queue.Any() && res == queue.FindMin()) queue.DeleteMin();
            }
            return (int)res;
        }
        public List<(int, int)> ShortestPath(char[,] matrix, int n, int m, (int sx, int sy) start, (int x, int y) target)
        {
            var offset = new[] {-1, 0, 1};
            var dist = 
                new int[n * m].Select(e => int.MaxValue).ToMatrix(n, m);
            var prev = 
                new (int, int)[n * m].Select(e => (-1, -1)).ToMatrix(n, m);
            var queue = new Queue<(int, int)>();
            dist[start.sx, start.sy] = 0;
            prev[start.sx, start.sy] = (start.sx, start.sy);
            queue.Enqueue(start);
            while (queue.Any())
            {
                var firstNode = queue.Dequeue();
                if (firstNode.Item1 == target.x && firstNode.Item2 == target.y)
                {
                    var stack = new Stack<(int, int)>();
                    stack.Push(firstNode);
                    var t = firstNode;
                    while (t.Item1 != start.sx || t.Item2 != start.sy)
                    {
                        t = prev[firstNode.Item1, firstNode.Item2];
                        stack.Push(t);
                    }

                    var ans = stack.Select(e => e).ToList();
                    return ans;
                }
                else
                {
                    //没有访问过，而且能够访问就访问，然后放到队列里。
                    foreach (var offsetX in offset)
                    {
                        foreach (var offsetY in offset)
                        {
                            if(offsetX == offsetY && offsetX == 0)
                                continue;
                            var nx = firstNode.Item1 + offsetX;
                            var ny = firstNode.Item2 + offsetY;
                            if(nx < 0 || ny < 0 || nx >= n || ny >= m || dist[nx, ny] == int.MaxValue)
                                continue;
                            queue.Enqueue((nx, ny));
                            dist[nx, ny] = dist[firstNode.Item1, firstNode.Item2] + 1;
                            prev[nx, ny] = firstNode;
                        }
                    }
                }
                
            }
            
            
            return null;

        }
    }



}