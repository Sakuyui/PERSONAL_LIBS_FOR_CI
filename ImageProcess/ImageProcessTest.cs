using System;
using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.ImageProcess
{
    public class ImageProcessTest
    {
        public static void FilterTest()
        {

            var mat = new double[]
            {
                56, 192, 45,
                1, 122, 44.0,
                11, 112, 45,
                125, 156, 222
            };
            var mat2 = mat.ToMatrix(4,3);
            ImageProcessUtils.GetConnectedArea(mat2, 100);
            
            
            
            var mat1 = new double[]
            {
                56, 192, 45,
                1, 122, 44.0,
                11, 112, 45,
                125, 156, 222
            }.ToMatrix(4, 3);
            
            var w = mat1.WindowSelect2D((3,3),(1, 1), (0,0,0,0), false);
            
            //中值滤波
            w.Select(m => {
                        var list = m.ElementEnumerator.OrderBy(e => e).ToList();
                        if ((list.Count & 1) == 1)
                            return list[list.Count / 2];
                        return (list[list.Count / 2 - 1] + list[list.Count / 2]) / 2;
                    }
                )
                .ToMatrix(4, 3).PrintToConsole();
            
            //均值滤波
            w.Select(m => m.ElementEnumerator.Average(e => e))
                .ToMatrix(4, 3).PrintToConsole();
            return;
            var matrix = new Matrix<int>(Utils.CreateTwoDimensionList(new int[12]
            {
                1,2,3,4,
                5,6,7,8,
                9,10,11,12
            },4,3));
            
            
            
            var filter = new Matrix<int>(Utils.CreateTwoDimensionList(new []
            {
                1, 0, 1,
                0, 5, 0,
                1, 0, 1
            },3,3));
            ImageFilter.ApplyCustomConvolutionToImage(matrix, filter);
            
           // ImageFilter.ApplyEqualWithConvolutionToImage(matrix, filter);
        }
    }
}