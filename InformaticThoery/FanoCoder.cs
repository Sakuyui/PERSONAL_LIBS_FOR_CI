using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.InformationThoery
{
    public class FanoCoder
    {
        public static void Test()
        {
            var dict = FanoCoder.GetCodeDictionary(
                new[]
                {
                    ('A', 0.2),
                    ('B', 0.19),
                    ('C', 0.18),
                    ('D', 0.17),
                    ('E', 0.15),
                    ('F', 0.10),
                    ('G', 0.01)
                }.ToList()
            );
            var input = "ACDEFABBCFG";
            dict.PrintCollectionToConsole();
            
            input.Aggregate("", (s, c) => s + dict[c]).PrintToConsole();
            
        }
        public static Dictionary<char, string> GetCodeDictionary(List<char> signals)
        {
            //频率计算并排序
            var n = signals.Count;
            var freq = signals.GroupBy(e => e)
                .Select(g => (g.Key, g.Count() / (double) n)).OrderByDescending(g => g.Item2).ToList();
            return GetCodeDictionary(freq);
        }
        public static Dictionary<char, string> GetCodeDictionary(List<(char, double)> freq)
        {
            
            var root = GetCodingTree(freq, "");
            
            return root.InorderEnumerator.Where(node => node.IsLeaf())
                .ToDictionary(node => (char)node.Data[0], node => (string)node.Data[2]);
        }
        private static BinaryTreeNode<ValueTupleSlim> GetCodingTree(IReadOnlyList<(char, double)> set, string path)
        {
            var n = set.Count;
            var s = set.Sum(e => e.Item2) / 2;
            var diff = double.MaxValue;
            var curSum = 0.0;
            if (set.Count == 1)
            {
                return new BinaryTreeNode<ValueTupleSlim>(new ValueTupleSlim(set[0].Item1, set[0].Item2, path));
            }
               
            var i = 0;
            while (i < n)
            {
                curSum += set[i].Item2;
                if (System.Math.Abs(curSum - s) < diff)
                {
                    diff = System.Math.Abs(curSum - s); //找到与中值的最小位置。这里其实可以用二分。相当于给一个值，找到左右边界的点，判定哪个更近
                    i++;
                }
                else
                    break;
            }

            
            var leftSet = set.Take(i).ToList();
            var rightSet = set.Skip(i).ToList();

            var root = new BinaryTreeNode<ValueTupleSlim>(null);
            var leftNode = GetCodingTree(leftSet, path + "0");
            var rightNode = GetCodingTree(rightSet, path + "1");
            root.Left = leftNode;
            root.Right = rightNode;
            return root;
        }
    }
}