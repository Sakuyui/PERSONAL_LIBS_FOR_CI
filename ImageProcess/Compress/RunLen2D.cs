using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.ImageProcess.Compress
{

    public static class RunLen2DTest
    {
        public static void Test()
        {
            var runLen2D = new RunLen2D();
            runLen2D.EncodeFor01Matrix(new[]
            {
                1, 0, 0, 1, 0,
                1, 1, 1, 1, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 1, 1, 0, 1
            }.ToMatrix(5, 5));
        }
    }
    public class RunLen2D
    {
        public List<List<int>> EncodeFor01Matrix(Matrix<int> matrix)
        {
            var r = matrix.RowsCount;
            var c = matrix.ColumnsCount;
            var ans = new List<List<int>>();
            for (var i = 0; i < r; i++)
            {
                var tmp = new List<int> {i};
                if(matrix[i].All(e => e == 0))
                    continue;
                var begin = -1;
                for (var j = 0; j < c; j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        if(begin == -1)
                            continue;
                        tmp.Add(begin);
                        tmp.Add(j - 1);
                        begin = -1;
                    }
                    else if (begin == -1)
                    {
                        begin = j;
                    }
                }

                if (begin != -1)
                {
                    tmp.Add(begin);
                    tmp.Add(c - 1);
                }
                if(tmp.Count > 1)
                    ans.Add(tmp);
            }
            ans.PrintMultiDimensionCollectionToConsole();
            return ans;
        }
    }
}