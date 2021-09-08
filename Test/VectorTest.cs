using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Test
{
    public class VectorTest
    {
        public static void Test()
        {
            var input = "1 2 3 4 5 5\r\n2 3 4 2 3 3";
            var a = input.Split("\r\n")
                .Select(e => e.Split(" ").Select(int.Parse).GroupByCount(3))
                .Select(line => line.Select(list => new Math.Vector<int>(list[0], list[1], list[2])).ToList())
                .ToList();


            var a2 =
                (from line in input.Split("\r\n")
                    let tmp = line.Split(" ").Select(int.Parse).ToList().GroupByCount(3)
                    select tmp.Select(l => new Math.Vector<int>(l[0], l[1], l[2])).ToList()
                ).ToList();
                
            foreach (var line in a2)
            {
                line.PrintCollectionToConsole();
            }
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28, -1};
            var vs = nums.GroupByCount(3).Select(e => e.ToVector());
            var s = vs.Sum(e => e, (v1, v2) =>
                (Math.Vector<int>) (v1 +  v2)); //注意一定要强制类型转换。为了防止二义性。不提供object到特定类型的隐式转换
            (s / 9.0).PrintToConsole();
            (s * s).PrintToConsole();
            (s * s._T()).PrintToConsole();
            (s._T() * s).PrintToConsole();

        }
    }
}