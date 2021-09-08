using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.Geometry;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.ImageProcess
{
    public class Line2DAlgorithms
    {
        public class ParamLine2D
        {
            public double A;
            public double B;
            public double C;
            //ax+by+c=0
            public ParamLine2D(double a, double b, double c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        public class PolarCorCurve2D
        {
            public double Theta;
            public double CosCoefficient;
            public double SinCoefficient;
            public PolarCorCurve2D(double sinCoefficient, double cosCoefficient)
            {
                SinCoefficient = sinCoefficient;
                CosCoefficient = cosCoefficient;
            }

            public override bool Equals(object? obj)
            {
                return obj switch
                {
                    null => false,
                    PolarCorCurve2D curve2D =>
                        System.Math.Abs(curve2D.CosCoefficient - CosCoefficient) < double.Epsilon &&
                        System.Math.Abs(curve2D.SinCoefficient - SinCoefficient) < double.Epsilon,
                    _ => false
                };
            }

            public override int GetHashCode()
            {
                return CosCoefficient.GetHashCode() + SinCoefficient.GetHashCode();
            }
        }

        private class Pair
        {
            public int E1;
            public int E2;

            public Pair(int e2, int e1)
            {
                E2 = e2;
                E1 = e1;
            }

            public override int GetHashCode()
            {
                return E1.GetHashCode() + E2.GetHashCode();
            }

            public override bool Equals(object? obj)
            {
                return obj switch
                {
                    null => false,
                    Pair pair => pair.E1 == E1 && pair.E2 == E2 || pair.E2 == E1 && pair.E1 == E2,
                    _ => false
                };
            }
        }
        //r = xcos t + ycos t  パラメータ平面で(r,t)は一つの直線を表します。
        public static ParamLine2D HoughTransform(IEnumerable<Point2D> points)
        {
            var curveSet = new HashSet<PolarCorCurve2D>();
            foreach (var p in points)
            {
                curveSet.Add(new PolarCorCurve2D(p.Y, p.X));
            }
            
            //要求曲线交点。很麻烦惹
            (List<double> theta, double r) GetCross(PolarCorCurve2D curve1, PolarCorCurve2D curve2)
            {
                var ce = curve1.CosCoefficient - curve2.CosCoefficient;
                var se = curve2.SinCoefficient - curve1.SinCoefficient;
                var res = new List<double>();
                if (ce != 0 && se != 0)
                {
                    var theta = se switch
                    {
                        > 0 when ce > 0 => (System.Math.Atan(se / ce) * 180) / System.Math.PI,
                        > 0 when ce < 0 => 180 + (System.Math.Atan(se / ce) * 180) / System.Math.PI,
                        < 0 when ce > 0 => 180 - (System.Math.Atan(se / ce) * 180) / System.Math.PI,
                        _ => -(System.Math.Atan(se / ce) * 180) / System.Math.PI
                    };
                    res.Add(theta);
                }
                
                if (ce == 0)
                {
                    res.AddRange(new []{0.0, 180.0, -180.0});
                }else if (se == 0)
                {
                    res.AddRange(new []{90.0, 270.0, -90.0, -270.0});
                }
                //var theta = System.Math.Atan2();
                //(System.Math.Atan2(45, 45) * 180/ System.Math.PI).PrintToConsole();
                var cos = System.Math.Cos(res[0] * System.Math.PI / 180);
                var sin = System.Math.Sin(res[0] * System.Math.PI / 180);

                return (res, curve1.CosCoefficient * cos + curve1.SinCoefficient * sin);
            }

            var curves = curveSet.ToList();
            var dict = new Dictionary<ValueTupleSlim, HashSet<Pair>>(); 
            
            var n = curves.Count;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if(i == j)
                        continue;
                    var cross = GetCross(curves[i], curves[j]);
                    foreach (var ta in cross.theta)
                    {
                        var t = new ValueTupleSlim(ta, cross.r); //两个参数
                        if (dict.ContainsKey(t))
                        {
                            dict[t].Add(new Pair(i, j));
                        }
                        else
                        {
                            dict[t] = new HashSet<Pair> {new(i, j)};
                        }
                    }
                    
                }
            }

            var most = dict.OrderByDescending(e => e.Value.Count)
                .First().Key;
            
            var theta = (double)most[0];
            var r = (double) most[1];
            var xE = System.Math.Cos(theta * System.Math.PI / 180.0);
            var yE = System.Math.Sin(theta * System.Math.PI / 180.0);
            var b = -r;
            return new ParamLine2D(xE, yE, b);
        }
    }
}