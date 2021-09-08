using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.CI
{
    public static class Ci2017
    {
        public static void WriteDigit(int num, Dictionary<int, Matrix<char>> digitDictionary)
        {
            var fullMatrix = new Matrix<char>(5, 0);
            var numStr = num + "";
            foreach (var d in numStr)
            {
                fullMatrix = d == '1' ?  fullMatrix.ColumnConcat(digitDictionary[d - '0'][..^1, ..0]):
                    fullMatrix.ColumnConcat(digitDictionary[d - '0']);
                fullMatrix.AddColumn(fullMatrix.ColumnsCount);
                fullMatrix.AddColumn(fullMatrix.ColumnsCount);
            }
            fullMatrix.PrintToConsole();
            //to file
            const string path = "d:\\out1.txt";
            var w = fullMatrix.Select(e => e.Aggregate("", (a, b) => a + b));
            //var fs = File.OpenWrite(path);
            //var sb = new BufferedStream(fs);
            File.WriteAllLines(path, w.ToArray());
        }

        public static int Recognize1( Dictionary<int, Matrix<char>> digitDictionary, int numCount)
        {
            var mat = File.ReadAllLines("d:\\out1.txt").ToMatrix2D();
            var ansBuilder = new StringBuilder();
            //mat.PrintToConsole();
            var newMat = mat.MatrixSelect((c, _, _) => c == '*' || c == '|' ? 1.0 : 0.0);
            //newMat.PrintToConsole();
            //连通域求解
            //这个库稍微修改一下，能够返回所有标签，还有标签对应的集合
            var labeledImg =ImageProcess.ImageProcessUtils.Labeling(newMat);
            /*foreach (var d in labeledImg.data)
            {
                d.Key.PrintToConsole();
                d.Value.PrintEnumerationToConsole();
            }*/
            
            labeledImg.mat.PrintToConsole();
            var targets = digitDictionary.Values
                .Select(e => e.MatrixSelect((me, _, _) => me == '*' || me == '|' ? 1 : 0)).ToArray();
            
            foreach (var areaPoints in labeledImg.data.Select(d => d.Value))
            {
                if (areaPoints.Count == 5)
                {
                    ansBuilder.Append('1');
                    continue;
                }
                areaPoints.PrintCollectionToConsole();
                var test = new Matrix<int>(5, 4)
                {
                    [..^1, ..^1] = newMat[areaPoints.First().Item1..areaPoints.Last().Item1,
                        areaPoints.First().Item2..areaPoints.Last().Item2].MatrixSelect((e,_,_) => (int)e)
                };
               
                
                var minIndex = targets.Select(e => ((Matrix<int>) (e - test)).MatrixSelect((e2, _, _) 
                        => System.Math.Abs(e2)).SelectMany(l => l).Sum()
                ).ArgMin(e => e).Item1;
                minIndex.PrintToConsole();
                ansBuilder.Append(minIndex + "");
            }
            ansBuilder.PrintToConsole();
            return int.Parse(ansBuilder.ToString());
        }
        public static void T2()
        {
            var digits = System.IO.File.ReadAllLines("d:\\digit.txt").GroupByCount(5)
                .Select(g => g.SelectMany(e => e).ToMatrix(5, 4)).ToList();
            var digMap = Enumerable.Range(0, 10)
                .ToDictionary(k => k, v => digits[v]);
            WriteDigit(831, digMap);
            Recognize1(digMap, 3);
        }
        
        

    }
}