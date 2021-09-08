using System.Linq;

namespace CIExam.Structure.Graph
{
    public class MaxFlowAlgorithm
    {
        public static int MaxFlow(MatrixGraph<int, (int, int)> matrixGraph,int s, int t)
        {
            var mflow = 0;
            var n = matrixGraph.NodeCount;
            var used = new bool[n];
            while (true)
            {
                //不断寻找整广路，直到不存在
                var f = MaxFlowDfs(matrixGraph, used, s, t, int.MaxValue);
                if (f == 0)
                    return mflow;
                mflow += f;
            }
        }
    
        //寻找增广路, f是已找到的最小能流动的大小
        public static int MaxFlowDfs(MatrixGraph<int, (int flow, int capacity)> matrixGraph, bool[] used, int v, int t, int f)
        {
            //DFS套路，，判断叶子，处理访问记录
            //判断叶子
            if (v == t)
                return f;
            //记录
            used[v] = true;
            
            //DFS套路-遍历所有有效分支节点
            var ext = matrixGraph.GetExtendNodes(matrixGraph[v])
                .Where(e => 
                (((int f, int c))matrixGraph[v, matrixGraph[e]].Data).c > 0 && !used[matrixGraph[e]]).ToList();
            
            
            for (var i = 0; i < ext.Count; i++)
            {
                var edge = matrixGraph[v, i];
                var (_, cap) = ((int flow, int cap)) edge.Data;
               
                
                //开始DFS，是要找最小可用容量
                var d = MaxFlowDfs(matrixGraph, used,  //矩阵和边传递
                    edge.To, t, System.Math.Min(f, cap)); //要传递最小容量惹，这个最小容量是要传递下去的
                
                //如果找到增广路，为这里加反向边，减去容量
                if (d > 0)
                {
                    var valueTuple = ((int f, int c)) matrixGraph[edge.To, edge.From].Data;
                        valueTuple.c += d;
                        return d;
                }
            }

            //无法增广
            return 0; //不存在流到终点的路径
        }


   
        //反向边要变为Cost的相反数, 同时用bellman-ford求最短路
        public static int MinCostFlow(MatrixGraph<int, (int, int)> matrixGraph, int s, int t, int f)
        {
            var n = matrixGraph.NodeCount;
            
            //初始化距离，要计算最短路径
            var dist = new int[n].Select(e => int.MaxValue).ToArray(); //shortest distance
            dist[s] = 0;
            //记下前一个点和边
            var prevv = new int[n];
            var preve = new int[n];
            var res = 0;
            while (f > 0)
            {
                //bellman - ford
                var update = true;
                while (update)
                {
                    update = false;
                    //每个节点
                    for (var v = 0; v < n && dist[v] != int.MaxValue; v++)
                    {
                        var ext = matrixGraph.GetExtendNodes(matrixGraph[v]).ToList();
                        ext = ext.Where(tv =>
                            //v -> 其拓展点的容量必须 > 0
                            (((int cost, int cap)) matrixGraph[v, matrixGraph[tv]].Data).cap > 0 &&
                            //选出可以经过v松弛的点tv。（经过v）
                            dist[v] + (((int cost, int cap)) matrixGraph[v, matrixGraph[tv]].Data).cost < dist[matrixGraph[tv]]
                        ).ToList();
                        
                        //遍历可访问的拓展节点
                        for (var i = 0; i < ext.Count; i++)
                        {
                            var edge = matrixGraph[v, matrixGraph[ext[i]]];
                            var edgeData = ((int cost, int cap)) edge.Data;
                            dist[edge.To] = dist[v] + edgeData.cost; //松弛
                            prevv[edge.To] = v;  //记录父节点
                            preve[edge.To] = matrixGraph[ext[i]]; //记录这个拓展节点
                            update = true;
                        }
                    }
                }
                
                //无法到达终点
                if (dist[t] == int.MaxValue) //无法增广
                    return -1;
                
                //沿着s -> t尽量增广
                //找到最小容量，增广
                var d = f;
                for (var v = t; v != s; v = prevv[v])
                {
                    d = System.Math.Min(d, (((int cost, int cap)) matrixGraph[prevv[v], preve[v]].Data).cap);
                }
                
                f -= d;
                res += d * dist[t];
                for (var v = t; v != s; v = prevv[v])
                {
                    var edge = matrixGraph[prevv[v], preve[v]];
                    var edgeData = ((int cost, int cap)) edge.Data;
                    edgeData.cap -= d;
                    var valueTuple = ((int cost, int cap)) matrixGraph[v, edge.From].Data;
                    valueTuple.cap += d; //添加反向边
                }
            }

            return res;
        }
    }
}