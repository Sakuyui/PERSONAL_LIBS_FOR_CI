using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Math.Test
{
    public static class CounterTest
    {
        public static void Test()
        {
            var list = new List<int>(new int[]{11,22,33,44,44,11,22,44});
            
            var counter = new Counter<int[],int>(new int[]{11,22,33,44,44,11,22,44},null);
            counter.DataList.Sort((tuple, tuple1) => tuple.Val - tuple1.Val);
            Console.WriteLine(Utils.HashMapToString(counter.CountTable));
            Console.WriteLine(Utils.ListToString(counter.DataList));

            
            
            
            //快速转换!!
            var a = counter.DataList.ConvertAll((input => (Vector<int>) input));
            
           
            
            
            Console.WriteLine(Utils.ListToString(a));


            var sum = counter.DataList.Sum(p => p.Val);
            //Linq!!
            Matrix<int> matrix = new Matrix<int>((from e in counter.DataList
                    orderby e.Key
                    select ((Vector<int>) new Math.Tuple<int, int>(e.Key, e.Val / sum)).Data
                ).ToList());
             
            
            
            
            var obj1 = (object) 3.25;
            var obj2 = (object) new Matrix<int>(3,2, (i, j, e) => i + j);
            //dynamic
            var objSum = (Matrix<double>)((dynamic) obj2 + obj1);   //コンパイル可能
            Console.WriteLine(objSum);
            Console.WriteLine(matrix);

            
            //var a =(Matrix<int>)(mat[mat[e => e == 1]]) - 1;
            // a.PrintToConsole();
           
            
            
        }

    }
}