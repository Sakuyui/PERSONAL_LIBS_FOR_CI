using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;
using CIExam.os.Cache;

namespace CIExam.Structure.Graph
{
    public class GraphAlgorithm
    {
        public static BaseGraph<int, int> Kruskal(BaseGraph<int, int> baseGraph)
        {
            var eSet = baseGraph.Edges.OrderBy(e => e.Data);
            var graph = new LinkedGraph<int, int>();
            Enumerable.Range(0, baseGraph.NodeCount).ElementInvoke(i => graph.AddNode(graph[i].Data));

            var jSet = new UnionFindTree(graph.NodeCount);
            
            foreach (var e in eSet)
            {
                if (jSet.Find(e.From) == jSet.Find(e.To)) continue;
                //add this edge
                graph.AddEdge(graph[e.From], graph[e.To], e);
                jSet.Merge(e.From, e.To);
            }

            return graph;
        }

        public static (IEnumerable<int> keyActivity, int keyPath) SolveAoeNetwork(BaseGraph<int, int> graph, int startNode, int finishNode)
        {
            IEnumerable<BaseEdge<int>> GetInEdge(int code) => graph.Edges.Where(e => e.To == code);
            IEnumerable<BaseEdge<int>> GetOutEdge(int code) => graph.Edges.Where(e => e.From == code);

            if (startNode >= graph.NodeCount || GetInEdge(startNode).Any() || graph.GetExtendNodes(finishNode, NodesRepresentType.CodeType).Any())
                throw new Exception("error");
            
            var finish = new bool[graph.NodeCount];
            //最早开始，最晚结束
            var dict = new Dictionary<int, (int fastestBegin, int slowestEnd)>();
            //记录路径
            var keyPath = new int[graph.NodeCount];
            keyPath[startNode] = -1;
            
            while (finish.Any(e => !e))
            {
                //找到第一个前置任务都已处理的节点
                var firstCanBegin = Enumerable.Range(0, graph.NodeCount)
                    .Where(node => !finish[node] && GetInEdge(node).All(e => finish[e.From]));
                if (!firstCanBegin.Any())
                    throw new Exception("No solve!");
                
                var choose = firstCanBegin.First();
                var fastBegin = GetInEdge(choose).Select(e => dict[e.From].fastestBegin + e.Data).Max();
                dict[choose] = (fastBegin, int.MaxValue);
                finish[choose] = true;
            }

            dict[finishNode] = (dict[finishNode].fastestBegin, dict[finishNode].fastestBegin);
            Enumerable.Range(0,graph.NodeCount).ElementInvoke(i => finish[i] = false) ;
            finish[finishNode] = true;
            while (finish.Any(e => !e))
            {
                var firstCanBegin = Enumerable.Range(0, graph.NodeCount)
                    .Where(node => !finish[node] && GetOutEdge(node).All(e => finish[e.To]));
                if (!firstCanBegin.Any())
                    throw new Exception("No solve!");
                
                var choose = firstCanBegin.First();
                var slowestEnd = GetOutEdge(choose).Select(e => dict[e.To].fastestBegin - e.Data).Min();
                dict[choose] = (dict[choose].fastestBegin, slowestEnd);
                finish[choose] = true;
            }
            
            //关键活动
            var keyActivity = dict.Where(kv => kv.Value.fastestBegin == kv.Value.slowestEnd)
                .Select(kv => kv.Key);
            
            //关键路径
            var cur = startNode;
            while (cur != finishNode)
            {
                var ext = graph.GetExtendNodesIndex(cur, NodesRepresentType.CodeType)
                    .First(node => dict[node].fastestBegin == dict[node].slowestEnd);
                keyPath[ext] = cur;
                cur = ext;
            }

            return (keyActivity, cur);

        }
        
        public static BaseGraph<int, int> Prim(BaseGraph<int, int> baseGraph)
        {
            var graph = new LinkedGraph<int, int>();
            if (baseGraph.NodeCount == 0)
                return new LinkedGraph<int, int>();
            graph.AddNode(baseGraph[0].Data); //从一个节点开始

            var jSet = new UnionFindTree(graph.NodeCount);
            var q = new PriorityQueue<int, BaseEdge<int>>();
            graph.GetExtendNodesIndex(0, NodesRepresentType.CodeType).ElementInvoke(e =>
            {
                q.EnQueue(graph[0, e].Data, graph[0, e]);
            });
            while (graph.NodeCount != baseGraph.NodeCount && q.Any())
            {
                while (q.Any())
                {
                    var f = q.DeQueue();
                    if (jSet.Find(f.item.From) != jSet.Find(f.item.To))
                    {
                        graph.AddNode(baseGraph[f.item.To].Data);
                        graph.AddEdge(graph[f.item.From], graph[f.item.To], f.item); //这里有Bug.和kruskal不同。这里节点序号可能是变化的。
                        jSet.Merge(f.item.From, f.item.To);
                        break;
                    }
                }
            }

            return graph;
        }

        public int Tsp<T, TE>(BaseGraph<T, TE> graph)
        {
            var n = graph.NodeCount;
            var dp = Enumerable.Repeat(int.MaxValue, (1 << n) * n).GroupByCount(1 << n);
            dp[1][0] = 0;

            for (var s = 2; s < 1 << n; s++)
            {
                for (var u = 0; u < n; u++)
                {
                    if(((s >> u) & 1) == 0)
                        continue;
                    for (var v = 0; v < n; v++)
                    {
                        if (((s >> v) & 1) == 0) continue;
                        dp[s][v] = System.Math.Min(dp[s][v], dp[s - (1 << v)][u] + (dynamic)graph[u, v].Data);
                    }
                }
            }

            var ans = int.MaxValue;
            for (var i = 0; i < n; i++)
            {
                ans = System.Math.Min(ans, dp[(1 << n) - 1][i] + (dynamic)graph[i, 0].Data);
            }
            return ans;
        }
        // public int[] findOrder(int numCourses, int[][] prerequisites) {
        //     if (numCourses <= 0) {
        //         return new int[0];
        //     }
        //
        //     HashSet<Integer>[] adj = new HashSet[numCourses];
        //     for (int i = 0; i < numCourses; i++) {
        //         adj[i] = new HashSet<>();
        //     }
        //
        //     // [1,0] 0 -> 1
        //     int[] inDegree = new int[numCourses];
        //     for (int[] p : prerequisites) {
        //         adj[p[1]].add(p[0]);
        //         inDegree[p[0]]++;
        //     }
        //
        //     Queue<Integer> queue = new LinkedList<>();
        //     for (int i = 0; i < numCourses; i++) {
        //         if (inDegree[i] == 0) {
        //             queue.offer(i);
        //         }
        //     }
        //
        //     int[] res = new int[numCourses];
        //     // 当前结果集列表里的元素个数，正好可以作为下标
        //     int count = 0;
        //
        //     while (!queue.isEmpty()) {
        //         // 当前入度为 0 的结点
        //         Integer head = queue.poll();
        //         res[count] = head;
        //         count++;
        //
        //         Set<Integer> successors = adj[head];
        //         for (Integer nextCourse : successors) {
        //             inDegree[nextCourse]--;
        //             // 马上检测该结点的入度是否为 0，如果为 0，马上加入队列
        //             if (inDegree[nextCourse] == 0) {
        //                 queue.offer(nextCourse);
        //             }
        //         }
        //     }
        //
        //     // 如果结果集中的数量不等于结点的数量，就不能完成课程任务，这一点是拓扑排序的结论
        //     if (count == numCourses) {
        //         return res;
        //     }
        //     return new int[0];
        // }


        
        public static IEnumerable<int> TopologicalSort<T, TE>(BaseGraph<T, TE> graph)
        {
            var n = graph.NodeCount;
            var visited = new bool[n];
            int GetInRank(int u) => graph.Edges.Count(e => e.To == u && !visited[e.From]);
            var ans = new List<int>();
            
            while (visited.Any(e => !e))
            {
                var f = Enumerable.Range(0, n).Where(e => !visited[e] && GetInRank(e) == 0).ToArray();
                if (!f.Any()) //说明出现环路，拓扑排序识别
                    return Array.Empty<int>();
                ans.Add(f.First());
                visited[f.First()] = true;
            }
            return ans;
        }
        
        public static CommonTreeNode<(int v, int order)> GetDfsTree<T, T2>(BaseGraph<T, T2> graph)
        {
            var n = graph.NodeCount;
            if (n == 0)
                return new CommonTreeNode<(int v, int order)>((-1,-1));
            
            var visit = new bool[n];
            return GetDfsTreeDfs(graph, 0, visit);
            
        }
        public static CommonTreeNode<(int v, int order)> GetDfsTreeDfs<T, T2>(BaseGraph<T, T2> graph, int s, bool[] visit, int order = 0)
        {
            var node = new CommonTreeNode<(int, int)>((s, order));
            visit[s] = true;
            if (visit.All(e => e))
            {
                return node;
            }
            var ext = graph.GetExtendNodesIndex(s, NodesRepresentType.CodeType)
                .Where(e => !visit[e]);
            foreach (var i in ext)
            {
                var nextNode = GetDfsTreeDfs(graph, i, visit, order + 1);
                node.Children.Add(nextNode);
            }

            return node;
        }
        public static bool IsEularGraph<T, TE>(BaseGraph<T, TE> graph)
        {
            //判定连通性
            var connected = IsConnectedGraph(graph);
            if (!connected)
                return false;
            //入度=出度
            for (var i = 0; i < graph.NodeCount; i++)
            {
                var outRank = graph.GetExtendNodesIndex(i, NodesRepresentType.CodeType).Count;
                var inRank = graph.Edges.Count(e => e.To == i);
                if (outRank != inRank)
                    return false;
            }

            return true;
        }

        public static bool IsConnectedGraph<T, TE>(BaseGraph<T, TE> graph)
        {
            var n = graph.NodeCount;
            for (var u = 0; u < n; u++)
            {
                var visit = new bool[n];
                visit[u] = true;
                //对于每2对点判定连通性
                for (var v = 0; v < n; v++)
                {
                    if(u == v)
                        continue;
                    var q = new Queue<int>();
                    q.Enqueue(u);
                    while (q.Any())
                    {
                        var f = q.Dequeue();
                        visit[f] = true;
                        var ext = graph.GetExtendNodesIndex(f, NodesRepresentType.CodeType)
                            .Where(e => !visit[e] && !q.Contains(e));
                        ext.ElementInvoke(e => q.Enqueue(e));
                    }
                }

                if (visit.Any(e => !e))
                    return false;
            }

            return true;
        }
    }
}