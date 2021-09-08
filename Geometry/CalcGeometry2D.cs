using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using C5;
using CIExam.Math;
using CIExam.Structure;
using JJ.Framework.Collections;
using CIExam.FunctionExtension;

namespace CIExam.Geometry
{
    public enum CcwResult
    {
        CounterClockwise = 1,
        ClockWise = -1,
        OnlineBack = 2,
        OnlineFront = -2, //p2与p1共线，但是比p1长。
        OnSegment = 0 //p2在p0p1上
    }
    public static class CalcGeometryTest{
        public static void Test()
        {
            var s1 = new Segment2D(new Point2D(0, 0), new Point2D(2, 0));
            CalcGeometry2D.Ccw(s1.P1, s1.P2, new Point2D(-1, 1)).PrintToConsole();
            CalcGeometry2D.Ccw(s1.P1, s1.P2, new Point2D(-1, -1)).PrintToConsole();
            CalcGeometry2D.Ccw(s1.P1, s1.P2, new Point2D(-1, 0)).PrintToConsole();
            CalcGeometry2D.Ccw(s1.P1, s1.P2, new Point2D(0, 0)).PrintToConsole();
            CalcGeometry2D.Ccw(s1.P1, s1.P2, new Point2D(3, 0)).PrintToConsole();

            var s2 = new Segment2D(new Point2D(0, 0), new Point2D(3, 4));
            CalcGeometry2D.Project(s2, new Point2D(2, 5)).PrintCollectionToConsole();

            CalcGeometry2D.GetCrossPoint(new Segment2D(0, 0, 2, 0), new Segment2D(1, 1, 1, -1)).PrintToConsole();
            CalcGeometry2D.GetCrossPoint(new Segment2D(0, 0, 1, 1), new Segment2D(0, 1, 1, 0)).PrintToConsole();
            CalcGeometry2D.GetCrossPoint(new Segment2D(0, 0, 1, 1), new Segment2D(1, 0, 0, 1)).PrintToConsole();
        }
    }
    
    
    
    public static class CalcGeometry2D
    {
        public static (double r, Point2D center) GetCircleFromThreePoints(Point2D p1, Point2D p2, Point2D p3)
        {
            var set = new[] {p1, p2, p3};
            if ((p2 - p1).Cross2D(p3 - p2) == (object) 0)
            {
                //共线
                var pPair = (from point1 in set join point2 in set on 1 equals 1 select (point1, point2))
                    .Where(p => !p.point1.Equals(p.point2)).ArgMax(e => (e.point2 - e.point1).Normal()).Item2;
                var c = pPair.point2 - pPair.point1;
                return ((double) (c - pPair.point1).Normal(), c);
            }

            //|i, j||c,d|  id - cj
            
            var s1C = (p1 + p2) / 2;
            var s1Vec = (Math.Vector<double>)(p2 - p1);
            var t = s1Vec[0];
            s1Vec[0] = s1Vec[1];
            s1Vec[1] = -t;
            var s2C = (p2 + p3) / 2;
            var s2Vec = (Math.Vector<double>) (p3 - p2);
            t = s2Vec[0];
            s2Vec[0] = s2Vec[1];
            s2Vec[1] = -t;
            var cross = GeometryUtil.GetLineCross(
                s1C, s1C + s1Vec, s2C, s2C + s2Vec
            );
            return ((double)(cross - p1).Normal() ,cross);
        }

        public static (double r, Point2D center) GetMinimalCircle(IEnumerable<Point2D> points)
        {
            var r = new Random();
            var ps = points.ToList();
            //随机打乱
            ps.Sort( (point2D, point2D1) => r.Next(-1, 1)); //1/2概率交换或不交换
            var ri = AdvanceRandom.GetRandomIndexes(ps.Count).ToArray();
            var p1 = ps[0];
            var p2 = ps[1];
            var circle = GetCircleFromThreePoints(p1, p2, p2);
           
            for (var i = 2; i < ri.Length; i++)
            {
                var pi = ps[i];
                //按顺序添加点
                if ((double) (pi - circle.r).Normal() < circle.r)
                {
                    //在园内
                    continue;
                }
                //新的点一定在圆的边界上
                var ci = GetCircleFromThreePoints(p1, pi, pi);
                var pjs = ps.Take(i).Where(p => (double) (p - ci.r).Normal() < ci.r).ToArray();
                if (!pjs.Any())
                {
                    circle = ci;
                    continue;
                }
                var cj = GetCircleFromThreePoints(pjs.First(), pi, pi);
                var pks= ps.Take(i).Where(p => (double) (p - cj.r).Normal() < cj.r).ToArray();
                if (!pks.Any())
                {
                    circle = cj;
                    continue;
                }

                circle = GetCircleFromThreePoints(pi, pjs.First(), pks.First());
                
            }

            return circle;
        }
        public static IEnumerable<(Segment2D h, Segment2D v)> GetIntersectionVh(System.Collections.Generic.HashSet<Segment2D> vSegments,
            System.Collections.Generic.HashSet<Segment2D> hSegments)
        {
            var dict = vSegments.Union(hSegments).SelectMany(e => new[] {(e, e.P1), (e, e.P2)}).ToDictionary(
                k => k.Item2, v => v.e);
            var queue = new PriorityQueue<double, (Point2D, Segment2D)>();
            hSegments.ElementInvoke(e =>
            {
                queue.EnQueue(e.P1.X, (e.P1, e));
                queue.EnQueue(e.P2.X, (e.P2, e));
            });
            vSegments.ElementInvoke(e =>
            {
                queue.EnQueue(e.P1.X, (e.P1, e));
                //queue.EnQueue(e.P2.X, (e.P2, e));
            });
            var res = new List<(Segment2D, Segment2D)>();
            var v = new SortedSet<Segment2D>();
            bool IsH(Segment2D segment2D) => System.Math.Abs(segment2D.P1.Y - segment2D.P2.Y) < double.Epsilon;
            var rangeTree = new SortedRangeTreeMd<Segment2D>(1);
            while (queue.Any())
            {
                var (item, _) = queue.DeQueue();
                if (IsH(item.Item2))
                {
                    if (System.Math.Abs(item.Item1.X - item.Item2.P2.X) < double.Epsilon
                        && System.Math.Abs(item.Item1.Y - item.Item2.P2.Y) < double.Epsilon)
                    {
                        v.Remove(item.Item2);
                        //rangeTree.Remove(item.Item2);
                    }
                    else if (System.Math.Abs(item.Item1.X - item.Item2.P1.X) < double.Epsilon
                             && System.Math.Abs(item.Item1.Y - item.Item2.P1.Y) < double.Epsilon)
                    {
                        //左端点
                        v.Add(item.Item2);
                        //rangeTree.Add(item.Item2, item.Item1.Y);
                    }
                }
                else
                {
                    //这里是范围查询，可以考虑线段树
                    var seg = v
                        .Where(e => e.P1.Y >= item.Item2.P1.Y && e.P1.Y <= item.Item2.P2.Y).Select(e => (e, item.Item2));
                    res.AddRange(seg);
                    //var s = rangeTree.Query(item.Item2.P1.Y, item.Item2.P2.Y).Select(e => e.val);
                }
            }

            return res;
        }

        //旋转卡壳求直径
        static double RotatingCalipers(IEnumerable<Point2D> ps, int n){
            // 旋转卡壳 求点对最长距离     效率 O(n)
            // ch[] 凸包 为逆时针给出的
            // 计算最长距离的时候，枚举了每一条边 |p p+1| 及 离该边最远的点q 的情况， 每一次计算点到边两端的距离，并更新一下ans（取最大值）
            // 如果遇到下一个点与该线段围成的三角形面积(即叉积) 比这一次变大了，则比较下一个点 （即q++）
            // 如果遇到下一个点与该线段围成的三角形面积(即叉积) 比这一次变小了，则计算两个距离（因为会出现平行的情况）并更新ans
          
            var ch = ps.ToArray();
            var q = 1; double ans = 0;
            ch[n] = ch[0]; // ch[q+1] 可能会访问到
            for(var p = 0; p < n; p++){
                // 此处遍历的线段为 |p->p+1| 点为 q 
                // 所以叉积的意义为 线段|p->p+1| 和 点q 围成的三角形的有向面积，逆时针为正，因此在这里均为正
                var c1 = (Math.Vector<double>) (ch[q] - ch[p + 1]);
                var c2 = (Math.Vector<double>) (ch[p] - ch[p + 1]);
                var c3 = (Math.Vector<double>) (ch[q + 1] - ch[p + 1]);
               
                while(c1.Cross2D(c2) > c3.Cross2D(c2))
                    q = (q + 1) % n; // 移到下一个点，因为q可能会转几圈，因此对n取余
 
                // 这里要比较q点到线段 |p->p+1| 两个端点的距离，因为有可能在旋转的时候两个边平行
                ans = MathExtension.Max( ans, System.Math.Max((double)(ch[p] - ch[q]).Normal(), (double)(ch[p+1] - ch[q+1]).Normal()) );            
            }
            return ans; 
        }

      
        public static IEnumerable<Point2D> GetConvexHull2(IEnumerable<Point2D> points)
        {
            var ps = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            if (ps.Count < 3)
                return ps;
            var u = new List<Point2D> {ps[0], ps[1]}; //上部要放x最小
            var l = new List<Point2D> {ps[^1], ps[^2]};
            
            
            //构造上部
            for (var i = 2; i < ps.Count; i++)
            {
                for (var j = u.Count; j >= 2 && Ccw(u[j - 2], u[j - 1], ps[i]) != CcwResult.ClockWise; j--)
                {
                    u.RemoveAt(u.Count - 1);
                }
                u.Add(ps[i]);
            }
            //下部
            for (var i = ps.Count - 3; i >= 0; i--)
            {
                for (var j = l.Count; j >= 2 && Ccw(l[j - 2], l[j - 1], ps[i]) != CcwResult.ClockWise; j--)
                {
                    l.RemoveAt(l.Count - 1);
                }
                l.Add(ps[i]);
            }

            l.Reverse();
            return l.Concat(u).Distinct();
        }
        public static IEnumerable<Point2D> GetConvexHull(IEnumerable<Point2D> points)
        {
            //O(nlogn) 也可以分上下，利用旋转方向判别构建凸包同样的复杂度
            var simplePolygon = CreatePolygonFromPoints(points).ToArray();
            //$"simple polygon = {simplePolygon.GetMultiDimensionString()}".PrintToConsole();
            if (simplePolygon.Length <= 2)
                return null;
            var q1 = simplePolygon[0];
            var q2 = simplePolygon[1];
            var q3 = simplePolygon[2];
            var res = new Point2D[simplePolygon.Length + 1];
            res[1] = q1;
            res[2] = q2;
            res[3] = q3;
            var m = 3;
            
            for (var k = 4; k <= simplePolygon.Length; k++)
            {
                var l1 = (Math.Vector<double>)(res[m] - res[m - 1]); //解答里的最后2点
                var l2 = (Math.Vector<double>)(simplePolygon[k - 1] - res[m]); //k与m
                
                var angle = GetVecAngle(l1, l2) / System.Math.PI *  180;
                var cross = l1.Cross2D(l2);
                //$">> {angle},cross = {cross}".PrintToConsole();
                if (cross < 0)
                    angle = 360 - angle;
                
                while (angle > 180)
                {
                    //$"angle = {angle}, pop {res[m]}".PrintToConsole();
                    m--;
                    l1 = (Math.Vector<double>)(res[m] - res[m - 1]); //解答里的最后2点
                    l2 = (Math.Vector<double>)(simplePolygon[k - 1] - res[m]); //k与m
                
                    angle = GetVecAngle(l1, l2) / System.Math.PI *  180;
                    cross = l1.Cross2D(l2);
                    if (cross < 0)
                        angle = 360 - angle;
                    
                   
                }

                m++;
                res[m] = simplePolygon[k - 1];
                
            }

            return res.Skip(1).Take(m);
        }
        public static IEnumerable<Point2D> CreatePolygonFromPoints(IEnumerable<Point2D> points)
        {
            var ps = points.ToList();
            if(points.Count() <= 2)
                return Enumerable.Empty<Point2D>();
            //选择一个特殊点。选择横坐标最大的，thenby纵坐标最小的
            var p1 = ps.ArgMax(e => e,
                new CustomerComparer<Point2D>((p2d1, p2d2) => p2d1.X.CompareTo(p2d2.X)))
                .Item2;
            $"p1 = {p1}".PrintToConsole();
            ps.Remove(p1);
            var xAxis = new[] {0.0, 1.0}.ToVector();
            
            var set = ps.Select(p => (p, (double)GetVecAngle( (Math.Vector<double>)(p -  p1), xAxis)))
                .OrderBy(e => e.Item2).ThenBy(e => (e.p - p1).Normal()).ToArray();
            
            return set.Select(e => e.p).Prepend(p1);
        }

        public static double GetVecAngle(Math.Vector<double> vec1, Math.Vector<double> vec2)
        {
            var cos = (double)(vec1.Dot(vec2) / (double) (vec1.Normal() * vec2.Normal()));
            return System.Math.Acos(cos);
        }
        
        
        public static bool IsInnerPolygon(IEnumerable<Segment2D> segments, Point2D point)
        {
            var s = segments.ToArray();
            var x0 = point.X;
            var y0 = point.Y;
            var seg = new Segment2D(x0, double.MinValue, x0, double.MaxValue);
            const double epsilon = 1e-6;
            var cnt = (
                    from e in s 
                    where x0 >= e.P1.X && x0 <= e.P2.X 
                    where !IsParallel((dynamic) e.P2 - (dynamic) e.P1, (dynamic) new[] {0, 1}.ToVector()) 
                    let cross = GetCrossPoint(e, seg) 
                    where (!(System.Math.Abs(cross.X - e.P1.X) < epsilon) || !(System.Math.Abs(cross.Y - e.P1.Y) < epsilon)) 
                          && (!(System.Math.Abs(cross.Y - e.P2.Y) < epsilon) || !(System.Math.Abs(cross.X - e.P2.X) < epsilon)) 
                    select cross)
                .Count(cross => cross.Y < y0);

            return cnt % 2 == 1;
        }
        public static bool IsOrthogonal(Math.Vector<double> v1, Math.Vector<double> v2)
        {
            return System.Math.Abs(v1.Dot(v2)) < 1e-9;
        }

        public static double Distance(Math.Vector<double> v, Math.Vector<double> v2)
        {
            var t = (v - v2).Normal();
            return (double) t;
        }
        
        public static bool IsParallel(Math.Vector<double> v1, Math.Vector<double> v2)
        {
            return  System.Math.Abs(v1.Cross2D(v2)) < 1e-9;
        }
        public static bool IsCross(Segment2D segment1, Segment2D segment2)
        {
            return (int)Ccw(segment1.P1, segment1.P2, segment2.P1) * (int)Ccw(segment1.P1, segment1.P2, segment2.P2) <= 0 &&
                   (int)Ccw(segment2.P1, segment2.P2, segment1.P1) * (int)Ccw(segment2.P1, segment2.P2, segment1.P2) <= 0;
        }

        public static Point2D Project(Segment2D s, Point2D p)
        {
            var baseVec = s.P2 - s.P1;
            baseVec.PrintCollectionToConsole();
            var r = (dynamic) ((p - s.P1).Dot(baseVec)) / (dynamic)baseVec.Normal();
            ((double)r).PrintToConsole();
            var doubleR = (double) r;
            return s.P1 + baseVec * doubleR / baseVec.Normal();
        }
        
        
        

        public static Point2D Reflection(Segment2D s, Point2D p)
        {
            return p + (Project(s, p) - p) * 2.0;
        }



        public static (Point2D p1, Point2D p2) GetCrossPoint(Circle c1, Circle c2)
        {
            double Arg(Math.Vector<double> vec) => System.Math.Atan2(vec[1], vec[0]);
            Math.Vector<double> Polar(double a, double r) => new Point2D(System.Math.Cos(r) * a, System.Math.Sin(r) * a);
            if ((c2.R + c1.R) * (c2.R + c1.R) < (double) (c1.C - c2.C).Normal())
            {
                return (null, null);
            }

            var d = (double)(c1.C - c2.C).Normal();
            var a = System.Math.Acos((c1.R * c1.R + d * d - c2.R * c2.R) / (2 * c1.R * d));
            var t = Arg((Math.Vector<double>)(c2.C - c1.C));

            return (c1.C + Polar(c1.R, t + a), c1.C + Polar(c1.R, t - a));
        }
        public static (Point2D p1, Point2D p2) GetCrossPoint(Circle c1, Segment2D l)
        {
            var pr = Project(l, c1.C);
            var e = (l.P2 - l.P1) / (l.P2 - l.P1).Normal();
            var @base = System.Math.Sqrt(c1.R * c1.R - (double) (pr - c1.C).Normal());
            return (pr + e * @base, pr - e * @base);
        }

        public static double GetDistanceLP(Segment2D l, Point2D p)
        {
            return (double) (l.P2 - l.P1).Cross2D(p - l.P1) / (double) (l.P2 - l.P1).Normal();
        }
        public static double GetDistanceSP(Segment2D s, Point2D p)
        {
            if ((double) (s.P2 - s.P1).Dot(p - s.P1) < 0.0)
            {
                return (double) (p - s.P1).Normal();
            }
            if ((double) (s.P1 - s.P2).Dot(p - s.P2) < 0.0)
            {
                return (double) (p - s.P2).Normal();
            }            
            return GetDistanceLP(s, p);
        }
        public static Point2D GetCrossPoint(Segment2D s1, Segment2D s2)
        {
            if (!IsCross(s1, s2))
                return null;
            var baseVec = s2.P2 - s2.P1;
            var d1 = System.Math.Abs((double)baseVec.Cross2D(s1.P1 - s2.P2));
            var d2 = System.Math.Abs((double) baseVec.Cross2D(s1.P2 - s2.P1));
            var t = d1 / (d1 + d2);
            return s1.P1 + (s1.P2 - s1.P1) * t;
        }
        
        //判断p2位于P1哪里
        public static CcwResult Ccw(Point2D p0, Point2D p1, Point2D p2)
        {
            var t1 = p1 - p0;
            var t2 = p2 - p0;
            var crossRes = (double)t1.Cross2D(t2);
            switch (crossRes)
            {
                case > 1e-9:
                    return CcwResult.CounterClockwise;
                case < -1e-9:
                    return CcwResult.ClockWise;
            }

            //t1.PrintCollectionToConsole();
            //t2.PrintCollectionToConsole();
            if ((double) t1.Dot(t2) < -1e-9)
                return CcwResult.OnlineBack;
       
            return (double)t1.Normal() < (double)t2.Normal() ? CcwResult.OnlineFront : CcwResult.OnSegment;
        }
        public static CcwResult Ccw(Math.Vector<double> t1, Math.Vector<double> t2)
        {
            var crossRes = t1.Cross2D(t2);
            switch (crossRes)
            {
                case > 1e-9:
                    return CcwResult.CounterClockwise;
                case < -1e-9:
                    return CcwResult.ClockWise;
            }

            //t1.PrintCollectionToConsole();
            //t2.PrintCollectionToConsole();
            if (t1.Dot(t2) < -1e-9)
                return CcwResult.OnlineBack;
       
            return t1.Normal() < t2.Normal() ? CcwResult.OnlineFront : CcwResult.OnSegment;
        }
    }

    public struct Circle
    {
        public double R;
        public Point2D C;
    }
}