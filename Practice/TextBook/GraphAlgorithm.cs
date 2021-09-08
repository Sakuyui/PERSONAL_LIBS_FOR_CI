using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;
using CIExam.Structure.Graph;
using CIExam.Math;
using CIExam.Test;

namespace CIExam.Praticle.TextBook
{
    public class GraphAlgorithm
    {
        public delegate int HeuristicCalc(int s, int t);

        public static (int[] previous, int cost) AStar(MatrixGraph<int, int> matrixGraph, int s, int t, HeuristicCalc h)
        {
            var n = matrixGraph.NodeCount;
            if (s == t || n == 0)
                return (System.Array.Empty<int>(), 0);

            var dist = Enumerable.Repeat(int.MaxValue, n).ToArray();
            var previous = new int[n];
            var closeSet = new HashSet<int>();
            var openSet = new PriorityQueue<int, int>();
            openSet.EnQueue(s, 0);
            while (openSet.Any())
            {
                var firstNodeIndex = openSet.DeQueue();
                closeSet.Add(firstNodeIndex.item);
                //slack
                foreach (var extNode in matrixGraph
                    .GetExtendNodesIndex(firstNodeIndex.item, NodesRepresentType.CodeType).Where(
                        e => !closeSet.Contains(e)))
                {
                    var newDistance = dist[firstNodeIndex.item] + matrixGraph[firstNodeIndex.item, extNode].Data;
                    if (newDistance <= dist[extNode])
                    {
                        dist[extNode] = newDistance;
                        openSet.UpdateOrSetPriority( dist[extNode] + h.Invoke(extNode, t), extNode);
                    }
                }
            }

            return (previous, dist[t]);

        }
      
        
        
     
        public static (int[] previous, int cost) AStar(MatrixGraph<int, (int flow, int capacity)> matrixGraph, int s, int t, HeuristicCalc h = null)
        {
            var n = matrixGraph.NodeCount;
            
            //注意，A*对于终点必须是0 cost。因为 0 <= h(x, y) <= real_cost(x, y)。 估计代价必须比实际代价少，终点实际代价是0 
            if (s == t || n == 0) //图是空的，或者起点已经是终点了
                return (System.Array.Empty<int>(), 0);
            
            
            //记录距离和路径
            var distance = new int[n].Select(e => int.MaxValue).ToArray();
            var previous = new int[n];
            previous[s] = s;
            distance[s] = 0 + (h?.Invoke(s,t) ?? 0);
            //2个set
            var openSet = new PriorityQueue<int, int>();
            var closeSet = new HashSet<int>();
            openSet.EnQueue(0, s);
            
            //A*
            while (openSet.Any())
            {
                var fNode = openSet.DeQueue();
                closeSet.Add(fNode.item);
                foreach (var node in matrixGraph.GetExtendNodes(matrixGraph[fNode.item])
                    .Where(e => !closeSet.Contains(matrixGraph[e])))
                {
                    var x = matrixGraph[node];
                    //注意这里新距离计算的是实际的
                    var newDistance = distance[fNode.item] + matrixGraph[fNode.item, x].Data.capacity;
                    if (newDistance < distance[x])
                    {
                        previous[x] = fNode.item;
                        distance[x] = newDistance;
                        //注意这里的优先度是要加上启发函数值的。
                        openSet.UpdateOrSetPriority(x, distance[fNode.item] + (h?.Invoke(x, t) ?? 0));
                    }
                }
            }
            
            return (previous, distance[t]); //返回路径
        }


        
        //O(EV^2)
        public static int MaxFlow(MatrixGraph<int, (int flow, int capacity)> matrixGraph, int s, int t)
        {
            if (s == t)
                return 0;
            int f;
            var maxFlow = 0;
            var n = matrixGraph.NodeCount;
            while ((f = MaxFlowDfs(matrixGraph, s, t, int.MaxValue, new bool[n])) > 0)
            {
                maxFlow += f;
            }

            return maxFlow;
        }

        public static int MaxFlowDfs(MatrixGraph<int, (int flow, int capacity)> matrixGraph, int s, int t, int min, bool[] visited)
        {
            //判断叶节点
            if (s == t)
                return min;
            
            //标记
            visited[s] = true;
            
            //拓展节点
            var ext = matrixGraph.GetExtendNodes(matrixGraph[s]).Where(e => 
                (((int f, int c))matrixGraph[s, matrixGraph[e]].Data).c > 0 && !visited[matrixGraph[e]]).ToList().Select(e => matrixGraph[e]);
            
            foreach (var extNode in ext)
            {
                var edge = matrixGraph[s, extNode];
                var edgeCost = matrixGraph[s, extNode].Data;
                var d = MaxFlowDfs(matrixGraph, extNode, t, System.Math.Min(min, edgeCost.capacity), visited);
                if (d > 0) //dfs找到一条增广路
                {
                    var reverseEdge = ((int f, int c)) matrixGraph[edge.To, edge.From].Data;
                    reverseEdge.c += d; //反向边
                    edgeCost.capacity -= d;
                    return d; //dfs，找到一个解就不管了
                }
            }

            
            return 0;
        }

        
        //O(VE)
        public static int[] BellmanFord(MatrixGraph<int, int> matrixGraph, int s)
        {
            var n = matrixGraph.NodeCount;
            var dist = new int[n].Select(_ => int.MaxValue).ToArray();
            dist[s] = 0;
            for (var i = 0; i < n - 1; i++)
            {
                foreach (var edge in matrixGraph.Edges)
                {
                    //尝试通过这条边到边的终点v
                    var newDist = dist[edge.From] + edge.Data;
                    if (dist[edge.To] > newDist)
                    {
                        dist[edge.To] = newDist;
                    }
                }
            }
            return dist;
        }
        
        //树的直径： 找到任意s最远的点x，找x最远的点y。xy的距离就是

        public static IEnumerable<int> GetArtPoints<TN, TE>(BaseGraph<TN, TE> graph)
        {
            var visit = new bool[graph.NodeCount];
            var n = visit.Length;
            var parent = new int[n];
            var preNum = new int[n];
            var lowest = new int[n];
            var timer = 1; //计数。DFS中被访问的顺序
            void Dfs(int current, int prev)
            {
                preNum[current] = lowest[current] = timer;
                timer++;
                visit[current] = true;
                foreach (var next in graph.GetExtendNodesIndex(current, NodesRepresentType.CodeType).Where(index => visit[index]))
                {
                    parent[next] = current;
                    Dfs(next, current);
                    lowest[current] = System.Math.Min(lowest[current], preNum[next]);
                }
            }
            //calc lowest
            Dfs(0, -1);
            var ap = new HashSet<int>();
            var np = 0;
            for (var i = 1; i < n; i++)
            {
                var p = parent[i];
                if (p == 0)
                    np++;
                else if (preNum[p] <= lowest[i]) //关节点的判断条件
                    ap.Add(p);
            }
            /*
             *   1）求桥的时候：因为边是无方向的，所以父亲孩子节点的关系需要自己规定一下，

                                在tarjan的过程中if（v不是u的父节点） low[u]=min(low[u],dfn[v]);

                               因为如果v是u的父亲，那么这条无向边就被误认为是环了。

            2）找桥的时候：注意看看有没有重边，有重边的边一定不是桥，也要避免误判
             */
            if (np > 1)
                ap.Add(0);
            //graph.Edges.Select(e => (u: e.From, v: e.To)).Where(e => preNum[e.u] < lowest[e.v]);
            return ap;
        }
        //O(V^3)
        public static (int[][] previous, int[][] cost) Floyd(MatrixGraph<int, int> matrixGraph)
        {
            var n = matrixGraph.NodeCount;
            var prev = new int[n].Select(_ => new int[n]).ToArray();
            var cost = new int[n].Select(_ => Enumerable.Repeat(int.MaxValue, n).ToArray()).ToArray();
            for (var i = 0; i < n; i++)
                cost[i][i] = 0;
            foreach (var edge in matrixGraph.Edges)
            {
                cost[edge.From][edge.To] = edge.Data;
            }

            for (var k = 0; k < n; k++)
            {
                for (var i = 0; i < n; i++)
                {
                    for (var j = 0; j < n; j++)
                    {
                        cost[i][j] = System.Math.Min(cost[i][j], cost[i][k] + cost[k][j]);
                    }
                }
            }

            return (prev, cost);
        }

        //O(E + VlogV) //O(Elog V)
        public static (int[] prev, int[] cost) Dijstra(MatrixGraph<int, int> matrixGraph, int s)
        {
            var n = matrixGraph.NodeCount;
            
            //注意，A*对于终点必须是0 cost。因为 0 <= h(x, y) <= real_cost(x, y)。 估计代价必须比实际代价少，终点实际代价是0 
            if (n == 0) //图是空的，或者起点已经是终点了
                return (System.Array.Empty<int>(), System.Array.Empty<int>());
            
            
            //记录距离和路径
            var distance = new int[n].Select(e => int.MaxValue).ToArray();
            var previous = new int[n];
            previous[s] = s;
            distance[s] = 0;
            //2个set
            var openSet = new PriorityQueue<int, int>();
            var closeSet = new HashSet<int>();
            openSet.EnQueue(0, s);
            
            while (openSet.Any())
            {
                var fNode = openSet.DeQueue();
                closeSet.Add(fNode.item);
                foreach (var node in matrixGraph.GetExtendNodes(matrixGraph[fNode.item])
                    .Where(e => !closeSet.Contains(matrixGraph[e])))
                {
                    var x = matrixGraph[node];
                    //注意这里新距离计算的是实际的
                    var newDistance = distance[fNode.item] + matrixGraph[fNode.item, x].Data;
                    if (newDistance < distance[x])
                    {
                        previous[x] = fNode.item;
                        distance[x] = newDistance;
                        //注意这里的优先度是要加上启发函数值的。
                        openSet.UpdateOrSetPriority(x, distance[fNode.item]);
                    }
                }
            }
            
            return (previous, distance); //返回路径
        }
        //O(EV + v(e + vlogv) + V^2) = O(ve + v^2logv)
        public static (int[][] prev, int[][] cost) Johson(MatrixGraph<int, int> matrixGraph)
        {
            var n = matrixGraph.NodeCount;
            if (n == 0)
                return (null, null);
            var prev = new int[n].Select(_ => new int[n]).ToArray();
            var cost = new int[n].Select(_ => Enumerable.Repeat(int.MaxValue, n).ToArray()).ToArray();
            for (var i = 0; i < n; i++)
                cost[i][i] = 0;

            var h = BellmanFord(matrixGraph, 0);
            //change edge value
            foreach (var edge in matrixGraph.Edges)
            {
                var u = edge.From;
                var v = edge.To;
                edge.Data = edge.Data + h[u] - h[v];
            }
            
            //n round dijstra
            for (var i = 0; i < n; i++)
            {
                var res = Dijstra(matrixGraph, i);
                prev[i] = res.prev;
                cost[i] = res.cost;
            }
            
            //recover edge value
            foreach (var edge in matrixGraph.Edges)
            {
                var u = edge.From;
                var v = edge.To;
                edge.Data = edge.Data - h[u] + h[v];
            }
            //process distance
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    cost[i][j] = cost[i][j] - h[i] + h[j];
                }
            }
            return (prev, cost);
        }
        //可以检测负环。如果一个节点入队超过n次，那么说明图中存在负环。
        
        public static void BellmanFordSPFA(MatrixGraph<int, int> matrixGraph, int s)
        {
            var n = matrixGraph.NodeCount;
            var distance = new int[n].Select(e => int.MaxValue).ToArray();
            var previous = new int[n].Select(e => -1).ToList();
            distance[s] = 0;
            previous[s] = s;
            var queue = new Queue<int>();
            queue.Enqueue(s);
            //使用队列优化的 BellmanFord
            //O(EV)
            while (queue.Any())
            {
                var node = queue.Dequeue();
                var ext = matrixGraph.GetExtendNodes(node).Select(e => matrixGraph[e]);
                //SPFA可以检测负环。如果某个元素入队超过N次，那么肯定存在负环。
                foreach (var e in ext)
                {
                    var eFrom = node;
                    var eTo = e;
                    
                    //只有产生变更才会放入队列
                    if (distance[eTo] <= distance[eFrom] + matrixGraph[eFrom, eTo].Data) 
                        continue;
                    previous[eTo] = eFrom;
                    distance[eTo] = distance[eFrom] + matrixGraph[eFrom, eTo].Data;
                    queue.Enqueue(e);
                }
            }

        }


        
        public static (int cost, bool isFound) IDAStart(MatrixGraph<int, int> matrixGraph, int s, int t,List<int> path, int g, int limit)
        {
            if (path.Count == 0)
                return (g, false);
            var lastNode = path.Last();
            var f = g + 0;
            if (f > limit)
            {
                return (f, false);
            }

            if (s == t)
            {
                return (f, true);
            }

            var min = int.MaxValue;
            foreach (var node in matrixGraph.GetExtendNodes(matrixGraph[lastNode])
                .Where(e => path.BinarySearch(matrixGraph[e]) < 0))
            {
                var x = matrixGraph[node];
                path.Add(x);
                var newDistance = g + (int) matrixGraph[lastNode, x].Data;
                var result = IDAStart(matrixGraph, x, t, path, newDistance, limit);
                min = System.Math.Min(min, result.cost);
                path.Remove(path.Count - 1);
            }

            return (min, false);

        }
        
        
    }
}