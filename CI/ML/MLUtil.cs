using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.Geometry;
using CIExam.Structure;
using CIExam.FunctionExtension;
using CIExam.Math;

namespace CIExam.CI.ML
{
    
    
    
    public static class MlUtils
    {


        public static void GradientDescendingTest()
        {
            var points = new[] {(1, 2), (4, 4), (6, 8), (10, 10), (2, 2)}
                .Select(e => new Point2D(e.Item1, e.Item2)).ToArray();
            var center = new Point2D(3, 3);
            var step = 0;
            double GetLoss() => points.Select(p => (double) ((p - center).Dot(p - center))).Sum();
            while (step < 10)
            {
                const double alpha = 0.05;
                var loss = GetLoss();
                $"cur step = {step}, loss = {loss}".PrintToConsole();
                var gradient = points.Select(xi => center - xi)
                    .Sum(e => e, (a, b) => a + b) * 2;
                $"gradient = {gradient}".PrintToConsole();
                center -= gradient * alpha;
                step++;
            }
            $"final point = {center}".PrintToConsole();
        }
        public static (object y, double p) GetBayesPossibility(IEnumerable<(ValueTupleSlim x, object y)> dataset, ValueTupleSlim inputX)
        {
            var valueTuples = dataset as (ValueTupleSlim x, object y)[] ?? dataset.ToArray();
            var p = GetConditionDistribution(valueTuples);
            var pxOy = p.pxoy;
            var y = valueTuples.Select(e => e.y).Distinct().ToArray();

           var ps = y.Select(ty => inputX.Aggregate(1.0, (a, b) =>
                a * pxOy[new ValueTupleSlim(b, ty)]));

           var (yIndex, possibility) = ps.ArgMax(e => e);
           return (y[yIndex], possibility);
        }
        //return p(x|y)
        public static (Dictionary<ValueTupleSlim, double> pxy, Dictionary<ValueTupleSlim, double> pxoy, Dictionary<object, double> py) GetConditionDistribution(IEnumerable<(ValueTupleSlim x, object y)> input)
        {
            var dataset = input.ToArray();
            //calc p(y|x)
            var pxy = new Dictionary<ValueTupleSlim, double>(); //p(x, y)
            foreach (var (x, y) in dataset)
            {
                foreach (var e in x)
                {
                    var xy = new ValueTupleSlim(e, y);
                    if (pxy.ContainsKey(xy))
                    {
                        pxy[xy] = pxy[xy] + 1;
                    }
                    else
                    {
                        pxy[xy] = 1;
                    }
                }
            }

            var eN = dataset.Select(t => t.x.Count()).Sum();
            foreach (var key in pxy.Keys)
            {
                pxy[key] = pxy[key] / eN; //p(x_i, y)
            }
            
            //get py
            var nY = dataset.Select(t => t.y).Distinct().Count();
            var dataN = dataset.Length;
            //p(y)
            var py = dataset.Select(e => e.y).GroupBy(y => y)
                .Select(g => (g.Key, g.Count() / (double)dataN))
                .ToDictionary(kv => kv.Key, kv => kv.Item2);
            
            //p(x_i|y)
            var pxOy = pxy.Select(e => (e.Key, e.Value / (double)py[e.Key[1]]))
                .ToDictionary(kv => kv.Key, kv => kv.Item2);
            
            return (pxy, pxOy, py);
        }
        public static double MultiLeast2Square(IEnumerable<Point2D> points, double c)
        {
            var data = points.ToArray();
            var n = data.Length;
            var dp = new double[n].Select(_ => double.MaxValue).ToArray();
            dp[0] = 0;
            double CalcCost(int p1, int p2)
            {
                var s = 0.0;
                for (var i = p1 + 1; i < p2; i++)
                {
                    var p = Point2D.Project(new Segment2D(data[p1], data[p2]), data[i]);
                    s += (double) ((data[i] - p1) - (p - s)).Normal();
                }
                
                return s;
            }
            for (var i = 1; i < n; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    var optJ = dp[j];
                    //j~i段的偏差
                    var cost = optJ + c + CalcCost(j, i);
                    dp[i] = System.Math.Min(dp[i], cost);
                }
            }

            return dp[n - 1];
        }
    }
}