using System.Collections.Generic;
using System.Linq;
using CIExam.Geometry;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.CI.ML
{
    public static class LayerClusteringTest
    {
        public static void Test()
        {
            var layerClustering = new LayerClustering();
            var points = new[]
            {
                new Point2D(2.3, 4.2),
                new Point2D(4.5, 2.1),
                new Point2D(2.4, 4.4),
                new Point2D(4.0, 2.4)
            };

           
            
            var root = layerClustering.Train(points);
            root.ClusterData.PrintEnumerationToConsole();
        }
    }
    public class ClusteringTreeNode : BinaryTreeNode<Point2D>
    {
        public IEnumerable<Point2D> ClusterData
        {
            get
            {
                return PreorderEnumerator
                    .Where(e => e.IsLeaf())
                    .Select(e => e.Data);


                /*
                 * *********************************** Method Closure *****************************************
                 *  the C# compiler detects when a delegate forms a closure which is passed out of
                 * the current scope and it promotes the delegate, and the associated local variables
                 * into a compiler generated class. This way, it simply needs a bit of compiler trickery
                 * to pass around an instance of the compiler generated class, so each time we invoke the
                 * delegate we are actually calling the method on this class. Once we are no longer holding
                 * a reference to this delegate, the class can be garbage collected and it all works exactly
                 * as it is supposed to!
                 * *******************************************************************************************
                 */
                
                /*var ans = new List<Point2D>();
                PreOrderDfs(delegate(BinaryTreeNode<Point2D> node, object[] _)
                {
                    if (node.IsLeaf())
                        ans.Add(node.Data); //Method Closure

                    return null;
                });
                return ans;*/
            }
        }
        
        public ClusteringTreeNode(Point2D value) : base(value)
        {
        }
    }
    public class LayerClustering
    {
        private readonly HashSet<ClusteringTreeNode> _nodeSet = new();
        private void Init(IEnumerable<Point2D> points)
        {
            _nodeSet.Clear();
            foreach (var p in points)
            {
                _nodeSet.Add(new ClusteringTreeNode(p));
            }
        }

        //寻找距离最近的两个类
        private (ClusteringTreeNode n1, ClusteringTreeNode n2) GetLeastDistanceTwoClusters()
        {
            if (_nodeSet.Count < 2)
                return (null, null);
            //可以使用更快的办法 - 分治法，这里偷懒直接linq了
            var argMinNode = (
                    from n1 in _nodeSet
                    join n2 in _nodeSet on 1 equals 1
                    where n1 != n2
                    select (n1, n2))
                .ArgMin(e => (double)(e.n1.Data - e.n2.Data).Dot(e.n1.Data - e.n2.Data)).Item2;

            
            return argMinNode;
        }
        
        
        private (ClusteringTreeNode n1, ClusteringTreeNode n2, double dis) GetLeastDistanceTwoClustersDivide(IReadOnlyList<ClusteringTreeNode> ps)
        {
            //节点少于2，不可能存在两个节点间最短距离
            if (_nodeSet.Count < 2)
                return (null, null, double.MaxValue);
            
            var n = ps.Count;
            //轴
            var sel = n / 2;
            
            //左右分治
            var left = GetLeastDistanceTwoClustersDivide(ps.Take(n / 2).ToList());
            var right = GetLeastDistanceTwoClustersDivide(ps.Skip(n / 2 + 1).ToList());
            
            var min = System.Math.Min(left.dis, right.dis);
            ClusteringTreeNode n1;
            ClusteringTreeNode n2;
            if (left.dis < right.dis)
            {
                n1 = left.n1;
                n2 = left.n2;
            }
            else
            {
                n1 = right.n1;
                n2 = right.n2;
            }
            
            //左寻找
            for (var i = n / 2 ; i >= 0; i--)
            {
                //横坐标差大直接放弃
                if(System.Math.Abs(ps[sel].Data.X - ps[i].Data.X) >= min)
                    break;
                var newDist = (double) (ps[sel].Data - ps[i].Data).Dot(ps[sel].Data - ps[i].Data);
                if(newDist >= min)
                    break;
                min = newDist;
                n1 = ps[i];
                n2 = ps[sel];
            }
            
            
            //左寻找
            for (var i = n / 2 + 1 ; i < n; i++)
            {
                if(System.Math.Abs(ps[sel].Data.X - ps[i].Data.X) >= min)
                    break;
                var newDist = (double) (ps[sel].Data - ps[i].Data).Dot(ps[sel].Data - ps[i].Data);
                if(newDist >= min)
                    break;
                min = newDist;
                n1 = ps[i];
                n2 = ps[sel];
            }

            return (n1, n2, min);
        }

        public ClusteringTreeNode Train(IEnumerable<Point2D> points)
        {
            var enumerable = points as Point2D[] ?? points.ToArray();
            if (!enumerable.Any())
                return null;
            Init(enumerable);
            while (_nodeSet.Count > 1)
            {
                var (n1, n2) = GetLeastDistanceTwoClusters();
                //合并最相似的两类
                _nodeSet.Remove(n1);
                _nodeSet.Remove(n2);
                var newNode = new ClusteringTreeNode((n1.Data + n2.Data) / 2) {Left = n1, Right = n2};
                _nodeSet.Add(newNode);
            }

            return _nodeSet.First();
        }
        
    }
}