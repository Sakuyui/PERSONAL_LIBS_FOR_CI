using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.ImageProcess;
using CIExam.Math;
using CIExam.Structure;
using JJ.Framework.Collections;
using KdTree;
using CIExam.FunctionExtension;


namespace CIExam.Geometry
{
    public static class GeometryUtil
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
        public static List<(int, int)> LeastCostPath(GreyImage greyImage, int sx, int sy, int tx, int ty)
        {
            var closeSet = new HashSet<ValueTupleSlim>();
            var openSet = new Structure.PriorityQueue<double,ValueTupleSlim >();
            var row = greyImage.Height;
            var col = greyImage.Width;
            openSet.EnQueue(greyImage.PixelMatrix[sx,sy],new ValueTupleSlim(sx, sy));
            
            var offset = new (int x, int y)[] {(-1, 0), (1, 0), (0, -1), (0, 1)};

            double Heuristic(int startX, int startY) =>
                System.Math.Sqrt((ty - startY) * (ty - startY) + (tx - startX) * (tx - startX));
            int GetOneDim(ValueTupleSlim t) => t[0] * row +  t[1];
            (int sx, int y) ToTwoDim(int c) => (c / row, c % row);
            
            var pre = new int[row * col].Select(_ => -1).ToArray();
            var dist = new double[row * col].Select(_ => double.MaxValue).ToArray();
            dist[0] = greyImage[0, 0].Color;

            var res = new List<(int,int)> ();
            
            while (openSet.Any())
            {
                var first = openSet.DeQueue();
                var ext = offset.Select(e => new ValueTupleSlim(e.x + first.item[0], e.y + first.item[1]))
                    .Where(c => c[0] >= 0 && c[0] < row && c[1] >= 0 && c[1] < col && !closeSet.Contains(c) &&
                                !openSet.Values.Contains(c));
                
                //$"Get {first.item}".PrintToConsole();
                closeSet.Add(first.item);
                if (first.item[0] == tx && first.item[1] == ty)
                {
                    var cur = GetOneDim(first.item);
                    while (pre[cur] != -1)
                    {
                        res.Add(ToTwoDim(cur));
                        cur = pre[cur];
                    }
                    res.Add((sx,sy));
                    res.Reverse();
                    return res;
                }
                foreach (var e in ext)
                {
                    var oneDim = GetOneDim(e);
                    var newDist = dist[GetOneDim(first.item)] + (double)greyImage.PixelMatrix[e[0], e[1]];

                    //$"{oneDim},{e}".PrintToConsole();
                    if (newDist > dist[oneDim]) 
                        continue;
                   
                    dist[GetOneDim(e)] = newDist;
                    pre[GetOneDim(e)] = GetOneDim(first.item);
                    openSet.EnQueue(newDist + (double)Heuristic(e[0], e[1]), e);
                }
            }

            return null;
        }
        
        
        public static List<double> GetCrossPoint(int[] start1, int[] end1, int[] start2, int[] end2) {
            int x1 = start1[0], y1 = start1[1];
            int x2 = end1[0], y2 = end1[1];
            int x3 = start2[0], y3 = start2[1];
            int x4 = end2[0], y4 = end2[1];
            
            var ans = new List<double>();
            bool IsInside(int x1, int y1, int x2, int y2, int xk, int yk) {
                // 若与 x 轴平行，只需要判断 x 的部分
                // 若与 y 轴平行，只需要判断 y 的部分
                // 若为普通线段，则都要判断
                return (x1 == x2 || System.Math.Min(x1, x2) <= xk && xk <= System.Math.Max(x1, x2)) && 
                       (y1 == y2 || System.Math.Min(y1, y2) <= yk && yk <= System.Math.Max(y1, y2));
            }
            void Update(double xk, double yk) {
                // 将一个交点与当前 ans 中的结果进行比较
                // 若更优则替换
                if (!ans.Any() || xk < ans[0] || (System.Math.Abs(xk - ans[0]) < double.Epsilon && yk < ans[1])) {
                    ans = new[]{xk, yk}.ToList();
                }
            }
            
            // 判断 (x1, y1)~(x2, y2) 和 (x3, y3)~(x4, y4) 是否平行
            if ((y4 - y3) * (x2 - x1) == (y2 - y1) * (x4 - x3)) {
                // 若平行，则判断 (x3, y3) 是否在「直线」(x1, y1)~(x2, y2) 上
                if ((y2 - y1) * (x3 - x1) == (y3 - y1) * (x2 - x1)) {
                    // 判断 (x3, y3) 是否在「线段」(x1, y1)~(x2, y2) 上
                    if (IsInside(x1, y1, x2, y2, x3, y3)) {
                        Update(x3, y3);
                    }
                    // 判断 (x4, y4) 是否在「线段」(x1, y1)~(x2, y2) 上
                    if (IsInside(x1, y1, x2, y2, x4, y4)) {
                        Update(x4, y4);
                    }
                    // 判断 (x1, y1) 是否在「线段」(x3, y3)~(x4, y4) 上
                    if (IsInside(x3, y3, x4, y4, x1, y1)) {
                        Update(x1, y1);
                    }
                    // 判断 (x2, y2) 是否在「线段」(x3, y3)~(x4, y4) 上
                    if (IsInside(x3, y3, x4, y4, x2, y2)) {
                        Update(x2, y2);
                    }
                }
                // 在平行时，其余的所有情况都不会有交点
            } else { 
                // 联立方程得到 t1 和 t2 的值
                var t1 = (double) (x3 * (y4 - y3) + y1 * (x4 - x3) - y3 * (x4 - x3) - x1 * (y4 - y3)) / ((x2 - x1) * (y4 - y3) - (x4 - x3) * (y2 - y1));
                var t2 = (double) (x1 * (y2 - y1) + y3 * (x2 - x1) - y1 * (x2 - x1) - x3 * (y2 - y1)) / ((x4 - x3) * (y2 - y1) - (x2 - x1) * (y4 - y3));
                // 判断 t1 和 t2 是否均在 [0, 1] 之间
                if (t1 is >= 0.0 and <= 1.0 && t2 is >= 0.0 and <= 1.0) {
                    ans = new []{x1 + t1 * (x2 - x1), y1 + t1 * (y2 - y1)}.ToList();
                }
            }
            return ans.ToList();
        }

   
    
        public static Point2D GetLineCross(Point2D p1, Point2D p2, Point2D p3, Point2D p4)
        {
            var vec1 =(Vector<double>) (p2 - p1);
            var vec2 = (Vector<double>) (p4 - p3);
            var a11 = vec1[0];
            var a12 = -vec1[1];
            var b1 = p1.X * a12 + p1.Y * a11;
            var a21 = vec2[0];
            var a22 = -vec2[1];
            var b2 = p2.X * a22 + p2.Y * a21;

            //a11y + a12x = b1, a21y + a22x = b2
            var d = a11 * a22 - a12 * a21;
            var y = (b1 * a22 - a12 * b2) / d;
            var x = (a11 * b2 - b1 * a21) / d;
            return new Point2D(x, y);
        }
        public static Point2D GetMirrorPoint(Point2D point, Segment2D segment)
        {
            var project = Point2D.Project(segment, point);
            var vec = point + ((project - point) * 2);
            return new Point2D((double) vec[0], (double) vec[1]);
        }
        public static Vector<double> GetReflection(Point2D point, Vector<double> direction, Segment2D segment)
        {
            var p2 = point + direction;
            var cross = GetLineCross(point, new Point2D((double) p2[0], (double) p2[1]), segment.P1, segment.P2);
            var mirror = GetMirrorPoint(point, segment);
            //c1y = c2x + c3;
            return (Vector<double>)(cross - mirror);
        }
        private class Event
        {
            internal enum EventType
            {
                LeftPoint,
                RightPoint,
                CrossPoint
            }
            public readonly EventType Type;
            public readonly Segment2D Segment;
            public readonly Segment2D Segment2;
            public Event(EventType type, double x, double y, Segment2D segment = null, Segment2D segment2 = null)
            {
                Type = type;
                Segment = segment;
                Segment2 = segment2;
            }
        }
        
        public static IEnumerable<Point2D> GetIntersections(IEnumerable<Segment2D> segments)
        {
            var segment2Ds = segments.ToArray();
            var q = new Structure.PriorityQueue<Point2D, Event>
                ((t1, t2) => System.Math.Abs(t1.X - t2.X) < double.Epsilon ? t1.Y.CompareTo(t2.Y) : t1.X.CompareTo(t2.X));
            FunctionExt.ElementInvoke(segments, e => 
            {
                q.EnQueue(e.P1, new Event(Event.EventType.LeftPoint, e.P1.X, e.P1.Y, e));
                q.EnQueue(e.P2, new Event(Event.EventType.RightPoint, e.P2.X, e.P2.Y, e));
            });
            var stateTable = new List<Segment2D>(); //这里要求是左端点y轴坐标升序
            var memo = new HashSet<ValueTupleSlim>();

            void InsertCrossPoint(Point2D cross, Segment2D s1, Segment2D s2)
            {
                if (memo.Contains(new[] {cross.X, cross.X}.ToValueTupleSlim())) return;
                q.EnQueue(cross, new Event(Event.EventType.CrossPoint, cross.X, cross.Y, s1, s2 ));
                memo.Add(new[] {cross.X, cross.Y}.ToValueTupleSlim());
            }
            void ProcessCrossPointsWithState(Segment2D s)
            {
                //计算交点
                
                if(!stateTable.Contains(s))
                    return;
                var index = stateTable.IndexOf(s);
                var pre = index - 1;
                var next = index + 1;
                if (pre >= 0)
                {
                    var cross = CalcGeometry2D.GetCrossPoint(s, stateTable[pre]);
                    InsertCrossPoint(cross, s, stateTable[pre]);
                }

                if (next >= stateTable.Count) return;
                {
                    var cross = CalcGeometry2D.GetCrossPoint(s, stateTable[next]);
                    InsertCrossPoint(cross, s, stateTable[next]);
                }
            }
            while (q.Any())
            {
                var (item, _) = q.DeQueue();
                switch (item.Type)
                {
                    case Event.EventType.LeftPoint:
                    {
                        ProcessCrossPointsWithState(item.Segment);
                        var i =stateTable.BinarySearch(item.Segment,
                            new CustomerComparer<Segment2D>((t1, t2) => t1.P1.Y.CompareTo(t2.P2.Y)));
                        stateTable.Insert(i + 1, item.Segment);
                        break;
                    }
                    case Event.EventType.RightPoint:
                        ProcessCrossPointsWithState(item.Segment);
                        stateTable.Remove(item.Segment);
                        break;
                    //交点的情况
                    case Event.EventType.CrossPoint:
                        break;
                    default:
                    {
                        //交换次序关系
                        //这里应该用二分查找
                        var index1 = stateTable.IndexOf(item.Segment);
                        var index2 = stateTable.IndexOf(item.Segment2);
                        var t = stateTable[index1];
                        stateTable[index1] = stateTable[index2];
                        stateTable[index2] = t;
                        break;
                    }
                }
            }

            return memo.Select(e => new Point2D(e[0], e[1]));
        }
        
        
        public static (Dictionary<int, int> xDict, Dictionary<int, int> yDict) GetDiscretionDict2D(IEnumerable<Point2D> points)
        {
            var enumerable = points as Point2D[] ?? points.ToArray();
            var xOrder = enumerable.Select(e => e.X).OrderBy(e => e);
            var yOrder = enumerable.Select(e => e.Y).OrderBy(e => e);
            var xDict = new Dictionary<int, int>();
            var yDict = new Dictionary<int, int>();
            var c = 0;
            foreach (var x in xOrder)
            {
                xDict[(int) x] = c++;
            }

            c = 0;
            foreach (var y in yOrder)
            {
                xDict[(int) y] = c++;
            }

            return (xDict, yDict);
        }

        
        public static IEnumerator<Point2D> GetMappedPoints(Dictionary<int, int> xDict, Dictionary<int, int> yDict, IEnumerable<Point2D> points)
        {
            return points.Select(p => new Point2D(xDict[(int) p.X], yDict[(int) p.Y])).GetEnumerator();
        }
    }
}