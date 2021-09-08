using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.Practice.LC
{
    public class BeautyProgramming
    {
        public static IEnumerable<int> TwoSum(int[] nums, int t)
        {
            var map = new Dictionary<int, int>();
            for (var i = 0; i < nums.Length; i++)
            {
                if (map.ContainsKey(t - nums[i]))
                {
                    return new[] {t - nums[i], nums[i]};
                }
                map[nums[i]] = i;
            }
            return null;
        }
        static IEnumerable<int> Get9Neighbor(ValueMatrix<int> s, int x, int y)
        {
            var r = x / 3;
            var c = y / 3;
            return s[(r * 3)..(r * 3 + 2), (c * 3)..(c * 3 + 2)].SelectMany(e => e).ToList();
        }
        static Dictionary<ValueTupleSlim, List<int>> GetFillMap(ValueMatrix<int> s)
        {
            //寻找所有空的点
            var pos = s.MatrixFind(e => e == 0);
            var canFill = pos.Select(e =>
            {
                var n9 = Get9Neighbor(s, e.Item1, e.Item2);
                var rest = Enumerable.Range(1, 9).Except(n9).Except(s[e.Item1])
                    .Except(s.ColumnsEnumerator.ToList()[e.Item2]).ToList();
                return (e.Item1, e.Item2, rest);
            }).ToDictionary(k => new []{k.Item1, k.Item2}.ToValueTupleSlim(), v => v.rest);
            return canFill;
        }

        static bool CheckState(Dictionary<ValueTupleSlim, List<int>> s)
        {
            return s.All(e => Enumerable.Any<int>(e.Value));
        }

        public static ValueMatrix<int> SudokuSolve(ValueMatrix<int> sudoku)
        {
           
            //坐标 -> 可以填入的元素
            
            if(SudokuDfs(sudoku))
                return sudoku;
            return null;
        }
        
        public static bool SudokuDfs(ValueMatrix<int> sudoku)
        {
            var dict = GetFillMap(sudoku);
            if (!dict.Any())
                return true;
            if (!CheckState(dict))
            {
                return false;
            }
            
            //优先探索
            var q = new PriorityQueue<int, KeyValuePair<ValueTupleSlim, List<int>>>();
            dict.ElementInvoke(kv => q.EnQueue(kv.Value.Count, kv));
            //这里重复构建太麻烦了，可以考虑动态改变优先队列。
            while (q.Any())
            {
                var f = q.DeQueue();
                var cor = f.item.Key;
                var selList = f.item.Value.ToArray();
                for (var i = 0; i < selList.Length; i++)
                {
                    sudoku[(int) cor[0], (int) cor[1]] = selList[i];
                    if (SudokuDfs(sudoku))
                    {
                        return true;
                    }
                    sudoku[(int) cor[0], (int) cor[1]] = 0;
                }
                return false;
            }

            return true;
        }
        
     
        
    }
}