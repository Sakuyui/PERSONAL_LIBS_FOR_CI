using System;
using System.Collections.Generic;

namespace CIExam.Math.Test
{
    public static class TupleTest
    {
        public static void Test()
        {
            //========================Tuple============================
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>Tuple Test<<<<<<<<<<<<<<<<<<<<<<<<");
            var tuple1 = new Tuple<int, double>(11,10.1);
            var tuple2 = new Tuple<double, double>(28.7,9.1);
            var tuple3 = new Tuple<int, double>(8,9.1);
            var tuple4 = new Tuple<double, double>(33,9.1);
            var tuples = new List<Tuple<object, object>> {tuple1, tuple2, tuple3, tuple4};


            Console.WriteLine(tuple1);
            tuple1.Key += 1;
            Console.WriteLine(tuple1.ConvertTo<double,int>());
            tuples.Sort();  //测试Sort()
            Console.WriteLine(Utils.ListToString(tuples));
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>Tuple Test End<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
    }
}