using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.CI.ML
{
    public class KNearest
    {
        private readonly int _k;

        public KNearest(int k)
        {
            _k = k;
        }

        //Tuple[0] = Vector<double>  Tuple[1] = y
        public int GetResult(IEnumerable<ValueTupleSlim> tuple, Vector<double> input)
        {
            return tuple.Select(t => {
                    var vec = (Vector<double>) t[0];
                    var y = (int) t[1];
                    return (dis: (double) (vec - input).Normal(), y);
                }).OrderBy(d => d.dis).Take(_k).ArgMax(d => d.y).Item2.y;
        }

    }

    public class WordNaiveBayes
    {
        public class Passage
        {
            public IEnumerable<string> Words;
            public int Y;
        }

        private readonly Dictionary<ValueTupleSlim, double> _conditionPossibility = new();
        private Dictionary<int, double> _yPossibility = new ();

        public void Train(IEnumerable<Passage> data)
        {
            var passage = data as Passage[] ?? data.ToArray();
            _conditionPossibility.Clear();
            _yPossibility.Clear();
            
            var wordSet = from ty in passage.SelectMany(e => e.Words) select ty;
            var totalCount = passage.Length;
            
            //y分布
            passage.Select(e => e.Y).GroupBy(e => e).ElementInvoke(delegate(IGrouping<int, int> ints)
            {
                _yPossibility[ints.Key] = ints.Count();
            });
            
            
            // x | y 条件分布
            var pGroup = passage.GroupBy(p => p.Y);
            foreach (var g in pGroup)
            {
                var y = g.Key; //y;
                var words = g.SelectMany(e => e.Words).ToArray();
                words.GroupBy(w => w).Select(g => (g.Key, g.Count())).ElementInvoke(
                    delegate((string Key, int) tuple)
                    {
                        var (key, val) = tuple;
                        var p = val / (double) words.Length;
                        _conditionPossibility.Add(new ValueTupleSlim(
                            key, y
                        ), p);
                    });
            }
            
        }

        public int GetOutput(List<string> words)
        {
            var list = new List<(int y, double p)>();
            var yList = _yPossibility.Keys.ToList();
            var wList = _conditionPossibility.Keys.Select(t => (string) t[0]).Distinct().ToList();

            var p = 0.0;
            foreach (var y in yList)
            {
                p = words.Where(w => _conditionPossibility.Keys.Contains(new ValueTupleSlim(w, y)))
                    .Sum(w => _conditionPossibility[new ValueTupleSlim(w, y)]);
                list.Add((y, p));
            }
            
            return list.ArgMax(e => e.p).Item2.y;
        }
        
    }
}