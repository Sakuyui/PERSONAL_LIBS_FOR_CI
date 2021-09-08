using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;
using CIExam.Structure;

namespace CIExam.CI
{
    public class CikMeans
    {
        public class VectorKMeans
        {
            public static void Test()
            {
                var input = System.IO.File.ReadAllLines("input.txt");
                
                //从文件中读取像素值，并且每3个一组转换为3维向量
                var points = input
                    .Select(e => e.Split(" "))
                    .Select(e => e.Select(int.Parse).GroupByCount(3))
                    .SelectMany(e => e.Select(p => p.ToVector()).ToList())
                    .ToList();

                //为了记录原像素所在的位置，这里创建一个数据集，给原像素全部加上编号。
                var dataSetForKMeans = points
                    .Select((p, index) => new Math.Tuple<int, Vector<int>>(index, p))
                    .ToList();
                

                var kMean = new VectorKMeans(k: 3);
                var indexesChosen = new[] {1, 4, 5}; //选择三个向量作为初始代表点

                //初始化初始代表点
                kMean.Init(indexesChosen.Select(index => dataSetForKMeans[index]).ToArray(), dataSetForKMeans);

                kMean.Train(epoch: 5);

                //获取最后各类代表点
                var rPoints = kMean.CentralVectors;
                //获取各类下的数据集合（以原像素索引呈现）
                var sets = kMean.Clusters;

                rPoints.PrintCollectionToConsole();
                sets.PrintCollectionToConsole();
            }

            public int K { get; }

            public IEnumerable<Vector<double>> CentralVectors => Represent.Select(p => p.Item2).ToList();

            public IEnumerable<List<int>> Clusters => _representSets.Select(p => p.Set.Select(
                s => s.Key).ToList()).ToList();

            private readonly RepresentSet<Vector<double>, Math.Tuple<int, Vector<int>>>[] _representSets;

            private IEnumerable<(int, Vector<double>)> Represent
            {
                get
                {
                    var t = _representSets.Select((e, i) => (i, e.Represent));
                    return t.ToList();
                }
            }

            private List<Math.Tuple<int, Vector<int>>> _dataSet;

            //初始化代表点
            public void Init(Math.Tuple<int, Vector<int>>[] represents,
                IEnumerable<Math.Tuple<int, Vector<int>>> dataset)
            {
                if (represents.Length != K)
                    throw new ArithmeticException();
                var i = 0;
                foreach (var r in represents)
                {
                    _representSets[i] =
                        new RepresentSet<Vector<double>, Math.Tuple<int, Vector<int>>>(r.Val.Select(e => (double) e).ToVector());
                    
                    i++;
                }

                _dataSet = dataset.ToList();
            }

            public void Train(int epoch = 3)
            {
                for (var i = 0; i < epoch; i++)
                {
                    Categorize();
                    UpdateRepresentPoint();
                }
            }

            private void Categorize()
            {
                var t = _dataSet.Select(d =>
                    Represent.Select(r => (r.Item1, (Vector<double>)(r.Item2 - d)))
                        .ArgMin(data => data, 
                            new CustomerComparer<(int, Vector<double>)>((t1, t2) =>
                                t1.Item2.Normal() < t2.Item2.Normal() ? -1: t1.Item2.Normal() > t2.Item2.Normal() ? 1 : 0
                            )).Item1).ToList();

                foreach (var r in _representSets)
                {
                    r.Clear();
                }

                for (var i = 0; i < _dataSet.Count; i++)
                {
                    _representSets[t[i]].Set.Add(_dataSet[i]);
                }

                //选取新代表点
                UpdateRepresentPoint();


            }

            private void UpdateRepresentPoint()
            {
                foreach (var r in _representSets)
                {
                    var newVector = r.Set.Select(e => e.Val)
                        .Sum(e => e,
                            (v1, v2) => (Vector<int>)( v1 + v2))
                        .Select(e => (double) e).ToVector();

                    r.Represent = (Vector<double>) (newVector / r.Represent.Count);
                }
            }

            public VectorKMeans(int k)
            {
                K = k;
                _representSets = new RepresentSet<Vector<double>, Math.Tuple<int, Vector<int>>>[k];
            }


            public class RepresentSet<Tr, Tv>
            {
                public Tr Represent;
                public readonly List<Tv> Set = new();

                public void Clear()
                {
                    Set.Clear();
                }

                public RepresentSet(Tr r)
                {
                    Represent = r;
                }
            }

        }
    }
}