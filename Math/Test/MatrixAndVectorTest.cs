using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Math.Test
{
    public static class MatrixAndVectorTest
    {
        public static void Test()
        {
            
            var vector1 = new Vector<int>(new []{1,2,3,4,5,6,7,8});
            vector1.Delete(0);
            vector1.Insert(0,0);
            vector1.Insert(9,-1);
            Console.WriteLine(vector1._T());
            Console.WriteLine(vector1);
            var vector2 = new Vector<double>(new []{1.1,2.2,3.3});
            var vector3 = new Vector<int>(new []{1,2,3});
            Console.WriteLine(vector2 * vector3);
            Console.WriteLine(vector2 + vector3);
            
            Console.WriteLine(vector2 - vector3);
            Console.WriteLine(vector2 / vector3);
            Console.WriteLine(vector2._T()*vector3);
            Console.WriteLine(vector2.CompareTo(vector3));
            
            Console.WriteLine(((Vector<Object>)(vector2*vector3))
                .Map( ((i,o) => (dynamic)o+100)));
            Console.WriteLine(((Vector<Object>)(vector2*vector3)).L2_Distance(vector2+vector3));
            Console.WriteLine(((Vector<Object>)(vector2*vector3)).L1_Distance(vector2+vector3));
            Console.WriteLine(vector1 * 2.1);
            
            
            //向量维度排序
            Console.WriteLine(vector1.Sort(delegate(object obj1, object obj2)
            {
                if ((dynamic) obj1 < (dynamic) obj2)
                {
                    return 1;
                }

                return -1;
            }));
          
            List<Vector<int>> v = new List<Vector<int>>();
            v.Add(new Vector<int>(new int[4]{1,3,4,5}));
            v.Add(new Vector<int>(new int[4]{5,3,4,5}));
            v.Aggregate((a,b) => (Vector<int>)(a+ b)).PrintToConsole();
            
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28};
            ((Vector<int>)nums.ToMatrix(1, nums.Length)).PrintToConsole();
            (v.Aggregate((a, b) => (Vector<int>) (a + b)) / 2)
                .ToVector(e => Convert.ToInt32(e) + " a ").PrintToConsole();
            Console.WriteLine(((Matrix<int>)vector1._T())._T());
            Console.WriteLine(new Matrix<int>(5,5,1));
            Console.WriteLine((Vector<int>)( ((Matrix<int>)vector1._T())));
            //子矩阵
            var matrix1 = new Matrix<int>(5, 5, 1).SubMatrix(1, 2, 1, 2);
            Console.WriteLine(matrix1);
            
            
            //Map测试
            Console.WriteLine(matrix1.Map((r, c, o) => o +" 惹-("+r+","+c+")" ));
            
            //索引子矩阵测试
            Console.WriteLine((new Matrix<int>(5, 5, 1))[1,-1,1,-1]);
            Console.WriteLine(matrix1.DotMultiply(matrix1 + 1.2));
        }
    }
}