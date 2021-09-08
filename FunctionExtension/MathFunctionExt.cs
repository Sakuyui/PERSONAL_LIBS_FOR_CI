using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.Math;

namespace CIExam.FunctionExtension
{
    public static class MathFunctionExt
    {
        public static double GetPi(int samplingCount = 10000, int samplingRange = 10000)
        {
            var tmp = new List<(int, int)>();
            var random = new Random();
            for (var i = 0; i < samplingCount; i++)
            {
                tmp.Add((random.Next(2 * samplingRange) - samplingRange, random.Next(2 * samplingRange) - samplingRange));
            }

            var r = tmp.Where(e => e.Item1 * e.Item1 + e.Item2 * e.Item2 < samplingRange * samplingRange);
            //面积比。 pi/4
            return r.Count() * 4 / (double)samplingCount;
        }
        public static T Normal<T>(this Vector<T> vector)
        {
            var t = vector.Sum(e => (dynamic) e * (dynamic) e, (n1, n2) => n1 + n2);
            return System.Math.Sqrt(t);
        }

        public static Matrix<T> CastToMatrix<T>(this T obj)
        {
            return (Matrix<T>) obj;
        }

        public static string Bin(this int n)
        {
            var s = new Stack<char>();
            var cur = (uint) n;
            while (cur != 0)
            {
                s.Push((cur & 1) == 1 ? '1' : '0');
                cur >>= 1;
            }

            return s.Aggregate("", (a, b) => a + b);
        }
    }
}