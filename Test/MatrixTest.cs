using System.Collections.Generic;
using System.Linq;
using CIExam.Geometry;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.Test
{
    public static class MatrixTest
    {
        private static int OddCells(int m, int n, int[][] indices)
        {
            var matrix = new Matrix<int>(m, n);
            foreach (var i in indices)
            {
                matrix.Apply(i[0]..i[0], ..^1, e => e + 1);
                matrix.Apply(..^1, i[1]..i[1], e => e + 1);
            }
            
            return matrix.SelectMany(e => e).Count(e => (e & 1) == 1);
        }
        
        public static void Koch(int d, Point2D p1, Point2D p2)
        {
            
            if(d == 0)
                return;
            var l = (p1 * 2 + p2) / 3;
            var r = (Vector<object>)((dynamic)(p1 + p2 * 2) / 3);
            var k = 
                ((Vector<object>)((l - p1).Insert(1) * TransFormUtil.Rotation2D(-60))).Delete();
            
            var u = l + k;
           
            Koch(d - 1,p1, l);
            l.PrintToConsole();
            Koch(d - 1, l ,u);
            u.PrintToConsole();
            Koch(d - 1, u, r);
            r.PrintToConsole();
            Koch(d - 1, r, p2);
            
            
        }

        public static void Simplify()
        {
            var mat = new int[]
            {
                1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
                1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
                1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
                0, 0, 0, 0, 0, 0, 1, 1, 0, 1,
                1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
                1, 1, 1, 0, 1, 1, 1, 1, 1, 0,
                1, 1, 1, 0, 1, 1, 1, 1, 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 1, 1, 0, 1, 1, 1, 1, 1, 0,
                1, 1, 1, 0, 1, 1, 1, 1, 1, 0,
            }.ToMatrix(10, 10);
            mat.PrintToConsole();
            
            //row reduce
            var m = mat.RowsEnumerator.Walk(delegate(List<int> l1, List<int> l2)
            {
                if (l1 == null && l2 != null)
                    return l2;
                return !l2.SequenceEqual(l1) ? l2 : null;
            }).ToList();
            var f = m.SelectMany(e => e).ToMatrix(m.Count, m[0].Count)
                .ColumnsEnumerator.Walk((l1, l2) =>
            {
                
                if (l1 == null && l2 != null)
                    return l2;
                return !l2.SequenceEqual(l1) ? l2 : null;
            }).ToList();
            f.SelectMany(e => e).ToMatrix(f.Count, f[0].Count)._T().PrintToConsole();
        }
        
        public static void SwapTest()
        {
            var mat = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.ToMatrix(3, 5);
            mat.PrintToConsole();
            var t = mat[..^1, ..0];
            
            
            //反转某部分
            mat[..^1, ..2] = mat[..^1, ..2].ColumnsEnumerator.Reverse().ToMatrix2D(true);
            

            mat[..^1, 3..^1] = mat[..^1, 3..^1].ColumnsEnumerator.Reverse().ToMatrix2D(true);
            mat.PrintToConsole();

            mat[..^1, ..^1] = mat[..^1, ..^1].ColumnsEnumerator.Reverse().ToMatrix2D(true);
            mat.PrintToConsole();
            
            mat[..^1, ..0] = mat[..^1, 2..2];
            
            mat[..^1, 2..2] = t;
            mat.PrintToConsole();
        }
        public static void F1()
        {
            var matrix = new Matrix<int>(Utils.CreateTwoDimensionList(new int[]
            {
                1,2,3,4,
                5,6,7,8,
                9,10,11,12
            },4,3));
            matrix[1][0].PrintToConsole();
            
            ("max = " + matrix.Max(e => e.Max(p => p))).PrintToConsole();
            
            //dense
            matrix.Aggregate((e1, e2) => e1.Concat(e2).ToList()).PrintEnumerationToConsole();
            
            matrix.PrintToConsole();
            matrix.Iloc(0, 1).PrintEnumerationToConsole();
            //matrix[-1,-1,0,1].PrintToConsole();
            matrix[2] -= (dynamic) new Vector<int>(1,6,2);
            matrix[1] += (dynamic) 10;
            matrix.PrintToConsole();
            matrix.Reverse180();
            matrix.PrintToConsole();
            // matrix.ColumnsEnumerator.Select(e => e.Max()).PrintEnumerationToConsole();
            //matrix.RowsEnumerator.Select(e => e.Max()).PrintEnumerationToConsole();
            
            ((Matrix<int>)new []{1, 2, 3, 4} * (Matrix<int>)new[]{2, 3, 4, 5}).PrintToConsole();
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28, -1};
            var m = nums.ToMatrix(3, 3);
            m.PrintToConsole();
            
            
            //以朴素的想法，下面的运算应该是没问题的。
            (m / m).PrintToConsole();
            (m / (m * 0.5)).PrintToConsole();
            (m * m).PrintToConsole();
            m.DotMultiply(m).PrintToConsole();
            (m * (m + 2)).PrintToConsole();
            m.ExtendIndex2D().PrintEnumerationToConsole();
            
            
            
            

            m.MatrixSelect((a,b,c) => a + " " + (b, c)).PrintToConsole();
            //平均
            // matrix.RowsEnumerator.Select(e => e.Sum() / e.Count).PrintEnumerationToConsole();

        }
        
        public static void Test()
        {
            var mat1 = new[] {0, 0, 0, 0, 0, 0}.ToMatrix(2, 3);
            var k = mat1[..1, ..1];
            k.PrintToConsole();
            mat1[..1, ..1] = 1.CastToMatrix();
            mat1.PrintToConsole();
            mat1[..1, ..1] = new[] {1, 2, 3, 4}.ToMatrix();
            mat1.PrintToConsole();
            var m = mat1[..^1, ..^1];
            m.PrintToConsole();
            m += (dynamic) 2.CastToMatrix();
            m.PrintToConsole();
            m.Apply(..^1,..^1, e => e + 2);
            m.PrintToConsole();
            OddCells(2, 2, new []{new []{1, 1}, new []{0, 0}}).PrintToConsole();
            //列连接
            m.ColumnsEnumerator.Concat(m.ColumnsEnumerator).ToMatrix2D()._T().PrintToConsole();
            //行连接
            m.Concat(m).ToMatrix2D().PrintToConsole();
        }
    }
}