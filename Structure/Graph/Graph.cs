using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.Structure.Graph
{

    public static class GraphTest
    {
        public static void Test()
        {
            var matrixGraph = new MatrixGraph<int, int>(5);

            var n1 = matrixGraph.AddNode(0);
            var n2 = matrixGraph.AddNode(1);
            var n3 = matrixGraph.AddNode(1);
            matrixGraph.AddEdge(n1, n2, GraphEdge<int>.FromObject(5));
            matrixGraph.Edges.Count.PrintToConsole();
            matrixGraph.Nodes.Count.PrintToConsole();
            matrixGraph.GetExtendNodes(matrixGraph[1]).Count.PrintToConsole();
        }



        public static (List<List<int>>, int[,]) Floyd(MatrixGraph<int, int> matrixGraph)
        {
            var n = matrixGraph.NodeCount;
            var dist = Utils.CreateTwoDimensionList(new int[n * n].Select(e => int.MaxValue).ToArray(), n, n);
            var previous = new int[n, n];
            for (var i = 0; i < n; i++)
            {
                dist[i][i] = 0;
                previous[i, i] = i;
            }
            //init
            foreach (var e in matrixGraph.Edges)
            {
                dist[e.From][e.To] = (int) matrixGraph[e.From, e.To].Data;
            }

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    for (var k = 0; k < n; k++)
                    {
                        var c = dist[i][j] + dist[j][k];
                        if (c < dist[i][k])
                        {
                            dist[i][k] = c;
                            previous[i, k] = j;
                        }
                    }
                }
            }
            return (dist, previous);
        }

        //dfs和回溯是相似的。path其实就是当前路径。遍历某个节点时扔进去，回溯的时候弹出。因为是iddfs要有limit,还要记录目前的cost
        public static (int cost, bool found) IDAStartDFS(MatrixGraph<int, int> matrixGraph, List<int> path, int g, int limit, int target)
        {
            if (!path.Any())
                return (g, false);
            var lastNode = path.Last();
            var h = 0; //估计函数
            var f = g + h;
            ///////////叶子判断
            //超过限制则返回
            if (f > limit)
                return (f, false);
            //找到目标
            if (lastNode == target)
                return (f, true);
            
            var min = int.MaxValue;
            
            
            ///////////分支操作
            //任意没访问过的节点进行dfs
            foreach (var e in 
                matrixGraph.GetExtendNodes(matrixGraph[lastNode]).Where(e => !path.Contains(matrixGraph[e])))
            {
                var x = matrixGraph[e];
                //dfs
                path.Add(x);
                
                var t = IDAStartDFS(matrixGraph, path, g + matrixGraph[lastNode, x].Data, limit, target);
                //搜索到惹
                if (t.found)
                    return t;
                if (t.cost < min)
                    min = t.cost;
                
                //回溯惹
                path.Remove(path.Count - 1);
            }

            return (min, false);
        }
        
        

        //多源最短惹。NB，但是O(n^3惹惹惹)
        public static void FloydWarshall(MatrixGraph<int, int> matrixGraph)
        {
            var n = matrixGraph.NodeCount;
            var mat = new Matrix<int>(n, n, int.MaxValue);
            var previous = new int[n, n];
            for (var i = 0; i < n; i++)
            {
                mat[i, i] = 0;
            }

            foreach (var x in matrixGraph.Edges)
            {
                mat[x.From, x.To] = mat[x.From, x.To];
            }

            for (var x = 0; x < n; x++)
            {
                for (var y = 0; y < n; y++)
                {
                    for (var z = 0; z < n; z++)
                    {
                        if (mat[x, z] > mat[x, y] + mat[y, z])
                        {
                            mat[x, z] = mat[x, y] + mat[y, z];
                            previous[x, z] = y; //以x为起点的最短路径下，z的前置是什么
                        }
                    }
                }
            }
        }
        
        
        //O(VE) 单源最短，允许负边
        public static void BellmanFord(MatrixGraph<int, int> matrixGraph, int start)
        {
            var n = matrixGraph.NodeCount;
            var previous = new int[n];
            var distance = new int[n].Select(e => int.MaxValue).ToArray();
            distance[start] = 0;
            for (var i = 0; i < n; i++)
            {
                //对于所有边
                foreach (var e in matrixGraph.Edges)
                {
                    var u = e.From;
                    var v = e.To;
                    //经过u到v
                    if (distance[v] > distance[u] + (int) matrixGraph[u, v].Data)
                    {
                        distance[v] = distance[u] + (int) matrixGraph[u, v].Data;
                        previous[v] = u;
                    }
                }
            }
        }

        
        //A* - 启发式算法。 这里启发函数=0，相当于迪杰特斯拉
        public static int AStart(MatrixGraph<int, int> matrixGraph, int start, int target)
        {
            var n = matrixGraph.NodeCount;
            var openSet = new PriorityQueue<int, int>();
            var closeSet = new System.Collections.Generic.HashSet<int>();
            var distance = new int[n].Select(e => int.MaxValue).ToArray();
            var previous = new int[n];
            distance[start] = 0;
            previous[start] = start;
            openSet.EnQueue(0, start);
            
            while (openSet.Any())
            {
                var firstNodeCode = openSet.DeQueue();
                closeSet.Add(firstNodeCode.item);
                foreach (var x in matrixGraph.GetExtendNodes(matrixGraph[firstNodeCode.item])
                    .Where(e => !closeSet.Contains(matrixGraph[e])))
                {
                    var xCode = matrixGraph[x];
                    var newDistance = distance[firstNodeCode.item] + (int) matrixGraph[firstNodeCode.item, xCode].Data;
                    if (newDistance < distance[xCode])
                    {
                        distance[xCode] = newDistance;
                        previous[xCode] = firstNodeCode.item;
                        openSet.UpdateOrSetPriority(xCode, newDistance);
                    }
                }
            }

            return distance[target];

        }
    }

    public abstract class BaseEdge<T>
    {
        public T Data = default;
        public int From;
        public int To;
    }

    public class GraphEdge<T> : BaseEdge<T>
    {
        
        
        public static GraphEdge<T> FromObject(T obj)
        {
            var g = new GraphEdge<T> {Data = obj};
            return g;
        }
    }

    public class GraphNode<TNodeData>
    {
      
        public GraphNode(TNodeData data, int nodeCode)
        {
            Data = data;
            NodeCode = nodeCode;
        }

        public int NodeCode;
        public TNodeData Data { get; set; }
    }
}