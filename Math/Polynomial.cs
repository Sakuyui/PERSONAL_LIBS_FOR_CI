using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;
using CIExam.Structure;

namespace CIExam.Math
{
    public class Polynomial
    {
        public struct Term
        {
            public double Coeff;
            public int N;

            public Term(double coeff, int n)
            {
                Coeff = coeff;
                N = n;
            }

        };

        public static (Polynomial syo, Polynomial rem) operator /(Polynomial p1, Polynomial p2)
        {
            var p1R = p1.GetSetRepresentation;
            var p2R = p2.GetSetRepresentation;
            var (s, rem) = PolyDivMod(p1R, p2R);
            return (new Polynomial(s, false), new Polynomial(rem, false));
        }
        public Polynomial(){}
        public Polynomial(IEnumerable<double> representList, bool isHighToLow = true)
        {
            var list = representList.ToList();
            var len = list.Count;
            for(var i = 0; i < len; i++)
            {
                this[i] = isHighToLow ? list[len - i - 1] : list[i];
            }
        }
        public readonly SortedList<int, Term> Dict = new(new CustomerComparer<int>((t1, t2) => t2.CompareTo(t1)));
        public double this[int n]
        {
            get => Dict[n].Coeff;
            set => Dict[n] = new Term(value, n);
        }

        // def normalize(poly):
        //     while poly and poly[-1] == 0:
        // poly.pop()
        // if poly == []:
        // poly.append(0)
        public static void Normalize(List<double> poly)
        {
            while (poly.Any() && poly[^1] == 0)
                poly.RemoveAt(poly.Count - 1);
            
            if (!poly.Any())
            {
                poly.Add(0);
            }
        }
        public static (List<double> s, List<double> rem) PolyDivMod(IEnumerable<double> p1, IEnumerable<double> p2)
        {
            var num = p1.ToList();
            var den = p2.ToList();
            Normalize(num);
            Normalize(den);
            var shiftLen = num.Count - den.Count;
            if (num.Count >= den.Count)
            {
              
                den.InsertRange(0,Enumerable.Repeat<double>(0, shiftLen));
            }
            else
            {
                return (s: new List<double> {0}, rem: num);
            }

            var quot = new List<double>();
            var divisor = den[^1];

            for (var i = 0; i <= shiftLen; i++)
            { 
                //Get the next coefficient of the quotient.
                var mult = num[^1] / divisor;
                quot.Insert(0, mult);

                if (mult != 0)
                {
                    var d = den.Select(u => mult * u);
                    num = num.Zip(d, (d1, d2) => (u: d1, v: d2)).Select(uv => uv.u - uv.v).ToList();
                }
                num.RemoveAt(num.Count - 1);
                den.RemoveAt(0);
            }
            Normalize(num);
            return (quot, num);
        }
   
        public IEnumerable<double> GetSetRepresentation
        {
            get
            {
                if(!Dict.Any())
                    return ArraySegment<double>.Empty;
                var len = Dict.First().Value.N + 1;
                var ans = new double[len];
                Dict.ElementInvoke(v => ans[v.Key] = v.Value.Coeff);
                return ans;
            }
        }
        public override string ToString()
        {
            return Dict.Aggregate("", (current, kv) => current + " + " +(kv.Value.Coeff + $"*x^({kv.Key})"));
        }
    }
}