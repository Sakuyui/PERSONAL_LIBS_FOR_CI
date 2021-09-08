using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using C5;
using CIExam.FunctionExtension;
using CIExam.OS;
using CIExam.Structure;
using CIExam.Math;
using CIExam.Math.Test;
using CIExam.Praticle.TextBook;
using CIExam.Structure.Automata;
using CIExam.Structure.Graph;

namespace CIExam.Test
{
    public class TempTest
    {
        
        public void RegexTest()
        {
            var s = "<img>abc</img>";
            Regex.Replace(s, @"<img>(?<str>.*?)</img>", "[图片]${str}").PrintToConsole();
            var s2 = @"<img src=""file://c:\abc.png""></img>";
            //(?<=...)表示前置占位但不匹配 (?=...)表示后置占位但不匹配
            
            /*
             *  (pattern) 匹配pattern 并获取这一匹配。
                (?:pattern) 匹配pattern 但不获取匹配结果，也就是说这是一个非获取匹配，不进行存储供以后使用。
                (?=pattern) 正向预查，在任何匹配 pattern 的字符串开始处匹配查找字符串。这是一个非获取匹配，也就是说，该匹配不需要获取供以后使用。
                (?!pattern) 负向预查，与(?=pattern)作用相反
             *
             * 
             */
            
            /*
             限定符: {n}恰好n, {n,}至少n {n,m} *?匹配0或多次，次数尽可能少 +?匹配1或多次，次数尽可能少。 ??0或1次，尽可能匹配少
             {n}? {n,m}? 匹配上一个元素，限定次数
             
             */
            /*反向引用
             * \number 
             * 
             */
            /*
             *替换模式: $$ 替换$,  $' 替换为匹配到结果后的字符串。  $+ 替换为最后匹配到的组 $_ 替换整个输入字符串 $&替换整个匹配项的一个副本
             * 
             */
            Regex.Replace(s2,@"(?<=<img src="")(?<g1>.+?)(?=""></img>)", "rep-${g1}").PrintToConsole();
            Regex.Matches(s2, @"(?<=<img src="")(?<g1>.+?)(?=""></img>)")[0].Groups["g1"].PrintToConsole();
        }
        public static void Test()
        {
            var test = new TempTest();
            var sim = new InstructionExecuteSim();
            sim.DecodeString("3[ab4[c]]5[ab]");
            
        }
      
        public static void Swap<T>(T[] nums, int i, int j)
        {
            var t = nums[i];
            nums[i] = nums[j];
            nums[j] = t;
        }


        public static void MatrixTest()
        {
               
            
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28, 1};



            nums.PrintCollectionToConsole();
            //读取文本形式存储的图片
            var img = File.ReadLines("D:\\创情复习\\img1.txt")
                .Select(line => line.Split(" ").Select(int.Parse))
                .ToMatrix2D();
            
            img.PrintToConsole();
            
            //var ext = ImageProcessUtils.GetEdgePoints();
            // var numMat1 = nums.ToMatrix(3, 3);
            // var numMat2 = nums.ToMatrix(3, 3)._T();
            // numMat1.PrintToConsole();
            // numMat2.PrintToConsole();
            //nums.ToMatrix(3,3).Prepend(new []{0,0,0}.ToList()).SelectMany(e => e).ToMatrix(4,3).PrintToConsole();

            //nums.BoyerMooreIndexOfSubPattern(new []{5, 1, 44}).PrintToConsole();
            //StringCompress.Test();
            
            //fw.sa
            //fw.SaveChange();
            //CI2020.Test2();
            
            //ImageProcessUtils.TestLabeling();
            //CalcGeometryTest.Test();
            //FormalLanguageParserTest.Test();

           
            return;


            var v = new List<Math.Vector<double>>
            {
                new(1.4, 3, 4.4, 5.7),
                new(5, 3, 4.2, 5)
            };
            
            var vec1 = new[] {1, 2}.ToVector();
            var vec2 = new[] {3, 4}.ToVector();
            var arr = new int[8] {1, 2, 3, 4, 5, 6, 7, 8};
            arr.GroupByCount(2).PrintMultiDimensionCollectionToConsole();
            var t = arr.GroupByCount(2).ToDataFrame(new[] {"a", "b"});
          
            t.PrintToConsole();
            var t2 = t.Sum(e => e, (serial, serial1) => serial + serial1);
            t2.PrintCollectionToConsole();
            var t3 = t2.ToDataFrame(2).ResetColNames(new []{"a", "b"});
            
            t3.PrintToConsole();
            arr.SplitCollection(new []{1,3,2,2}).PrintMultiDimensionCollectionToConsole();
            (vec1.Cross2D(vec2)).PrintToConsole();
            vec1.ToDataFrame().PrintToConsole();
            if (true)
                return;

            var df = nums.ToDataFrame();
            df["a"] = null;
            //df.PrintToConsole();
            df["0"].ToList(false)[0].MaxBy(e => e).PrintToConsole();
            var t1 = df["0"].OrderBy(e => e["0"]).Select(e => e["0"]).ToDataFrame();
            df["a"] = t1;
            df.AddColumn("b", (index, serial) => (int)serial["0"] + (int)serial["a"] + index);
            
            
            //df.PrintToConsole();
            df.OrderBy(e => e.Sum(s => (int)s)).ToDataFrame().PrintToConsole();
            var df2 = df.Select(e => e.Map((_, o) => System.Math.Sqrt((int)o)))
                .ToDataFrame();
            //df2.PrintToConsole();

            df2.Sum(e => e, (serial, serial1) => serial + serial1).PrintCollectionToConsole();
            
            // nums.ConditionFindWithBoolResult(e => e < 10).PrintCollectionToConsole();
            // nums.ConditionSelect(nums.ConditionFindWithBoolResult(e => e < 10)).PrintCollectionToConsole();
            // nums.ConditionSelect(e=> e < 10, (i, i1) => (i1, i)).PrintCollectionToConsole();
            // nums.ToMatrix(3, 3).ToDataFrame(new []{"一", "二", "三"}).PrintToConsole();
            
            
            
            var p = new Program();
        
            var mat = new Matrix<int>(5, 6)
             {
                 [e => e < 1] = 1
             };
             mat.PrintToConsole();
             
             //nums.WindowSelect(4, e => e.Sum()).PrintEnumerationToConsole();
             
             //TempTest.Test();
             return;
             
             //CollectionHelper.Test2();
            //TempTest.Test();
            //ImageProcessTest.FilterTest();
            //CoderTest.Test();
            //nums.ToMatrix(2,4).PrintToConsole();
            //nums.GroupByCount(3).PrintCollectionToConsole();
            //nums.GroupBy(e => e)
            //  .Select(e => new Math.Tuple<int, double>(e.Key,e.Count() / (double)(nums.Length)))
            //.PrintEnumerationToConsole();
            //GraphTest.Test();
            //PriorityQueueTest.Test();
            //DataFrameTest.Test();
            //DisJointSetTest.Test();
            //MemoryCacheTest.Test();
            //var m = nums.Select(e => e).MinBy(e => e - 10);
            //Console.WriteLine(m);
            //MatrixTest.Test();

            //AlgorithmP.GetTopK(new []{25,36,4,55,71,18,0,71,89,65},3);
            //AdvanceStructureTest.CacheTest();
            //AdvanceStructureTest.DictionaryTest();    
            //CounterTest();
            //AdvanceStructureTest.IndexKeepTableTest();
            //FileSysTest.TestReadFile();
            //CollectionHelper.Test();
            return;
            TupleTest.Test();
            MatrixAndVectorTest.Test();
        }
        
        
        public static System.Collections.Generic.HashSet<BinaryTreeNode<int>> Search(int l, int r,
            BinaryTreeNode<int> root)
        {

            if (l > r || root == null)
                return new System.Collections.Generic.HashSet<BinaryTreeNode<int>>();

            if (root.Data >= l && root.Data <= r)
            {
                var left = Search(l, r, root.Left);
                var right = Search(l, r, root.Right);
                return left.Union(right).Union(new[] {root}).ToHashSet();
            }

            return Search(l, r, root.Data < l ? root.Left : root.Right);
        }


        


      
    }

  
    
    
    
}