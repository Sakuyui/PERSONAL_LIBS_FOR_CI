using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.Math;
using CIExam.Structure.Graph;
using KdTree;

namespace CIExam.Practice.Interview
{
    public static class AlgorithmExt
    {
        public class FrequencyNode<T> : IComparable
        {
            public T Val;
            public int Freq;
            public IPriorityQueueHandle<FrequencyNode<T>> Handle = null;
            public FrequencyNode(T v, int i)
            {
                Val = v;
                Freq = i;
            }
            

            public override int GetHashCode()
            {
                return Val.GetHashCode();
            }

            public int CompareTo(object? obj)
            {
                if (obj is FrequencyNode<T> node)
                {
                    return Freq.CompareTo(node.Freq);
                }

                return -1;
            }
        }
        public static IEnumerable<T> DynamicFrequencyStatisticTopK<T>(IEnumerable<T> data, int k)
        {
            //使用堆。两种方式。一种利用size = k 的小堆+hash表记录出现次数。每次和堆顶比较决定选不选取新数据。
            //方法2是：利用2个哈希表记录节点是否在堆上，另一个记录每个节点（数据，记录）。
            //遇到一个数据，如果是第一次出现就创建统计节点并记录。否则的话修改频率，并且按在堆上以及不在堆上的情况分类讨论
            var heap = new IntervalHeap<FrequencyNode<T>>();
            var eDict = new Dictionary<T, FrequencyNode<T>>();
            var existDict = new Dictionary<FrequencyNode<T>, int>();
            foreach (var e in data)
            {
                //$"meet {e}".PrintToConsole();
                if (!eDict.ContainsKey(e))
                {
                    //$"create {e}".PrintToConsole();
                    var node = new FrequencyNode<T>(e, 1);
                    eDict[e] = node;
                    existDict[node] = -1; //不在堆上
                }
                else
                {
                    var node = eDict[e];
                    if (existDict[node] != -1)
                    {
                        //$"heap exist {node.Val}".PrintToConsole();
                        heap.Delete(node.Handle);
                    }
                    node.Freq++;
                    node.Handle = null;
                    heap.Add(ref node.Handle, node);
                    //$"update {node.Val},{node.Freq}".PrintToConsole();
                }

                var t = eDict[e];
                if (existDict[t] != -1)
                {
                    continue;
                }
                if (heap.Count < k)
                {
                    //可以放心的直接插入
                    t.Handle = null;
                    heap.Add(ref t.Handle, t);
                    existDict[t] = 1;
                }
                else
                {
                    var min = heap.FindMin();
                    if (min.Freq >= t.Freq) 
                        continue;
                    heap.DeleteMin();
                    existDict[min] = -1;
                    existDict[t] = 1;
                    heap.Add(ref t.Handle, t);
                    //如果最小元素比当前元素频率还大那就不用管了
                }
                //$"{heap.Count}\n".PrintToConsole();
            }
            //(heap.Count, k).PrintToConsole();
            return heap.Select(e => e.Val);
        }
        //k从0开始
        public static void TopK<T>(T[] arr, int s, int e, int k) where T : IComparable
        {
            while (true)
            {
                if (e >= s)
                {
                    return;
                }

                var r = new Random().Next(s, e);
                var tk = Partition(arr, r, s, e);
                if (tk >= k) return;

                s = tk + 1;
                k -= tk;
            }
        }

        public static int KColorizeGraphProblem<T, TE>(BaseGraph<T, TE> graph)
        {
            var n = graph.NodeCount;
            var l = 0;
            var r = n - 1;
            while (l <= r)
            {
                var m = l + ((r - l) >> 1);
                var t = IsCanColorized(graph, m);
                if (t)
                {
                    r = m - 1;
                }
                else
                {
                    l = m + 1;
                }
            }

            return l;
        }

        private static bool IsColorSafe<T, TE>(BaseGraph<T, TE> graph, int v, int c, int[] colored)
        {
            colored[v] = c;
            //至今为止的着色是否合法
            for (var i = 0; i <= v; i++)
            {
                var i1 = i;
                var ext = graph.GetExtendNodesIndex(i, NodesRepresentType.CodeType).Select(e => colored[e])
                    .Where(e => e != i1 && e != -1);
                var t = ext.Any(e => e == colored[i]);
                if (!t) 
                    continue;
                colored[v] = -1;
                return false;
            }
            colored[v] = -1;
            return true;
        }
        
        public static int SixteenGrid(ValueMatrix<int> initState)
        {
            //各格子到正确位置的曼哈顿距离和
            int CalcHeuristic(Matrix<int> state)
            {
                var s = 0;
                for (var i = 0; i < 4; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        if (state[i][j] == 0)
                            s += 6 - i - j;
                        else
                        {
                            var rightX = (state[i][j] - 1) / 4;
                            var rightY = (state[i][j] - 1) % 4;
                            s += System.Math.Abs(i - rightX) + System.Math.Abs(j - rightY);
                        }
                    }
                }

                return s;
            }

            bool IsFinalState(ValueMatrix<int> state) =>
                state.SelectMany(e => e).Take(15).SequenceEqual(Enumerable.Range(1, 15));

            void Swap(Matrix<int> mat, int si, int sj, int ti, int tj)
            {
                var t = mat[si, sj];
                mat[si, sj] = mat[ti, tj];
                mat[ti, tj] = t;
            }
            var closeSet = new System.Collections.Generic.HashSet<ValueMatrix<int>>();
            if (IsFinalState(initState))
                return 0;
            var q = new Structure.PriorityQueue<int, ValueMatrix<int>>();
            q.EnQueue(CalcHeuristic(initState), initState);
            var dist = new Dictionary<ValueMatrix<int>, int> {[initState] = 0};

            var direct = new[] {(-1, 0), (1, 0), (0, 1), (0, -1)};
            
            while (q.Any())
            {
                var f = q.DeQueue();
                closeSet.Add(f.item); // 记忆化
                
                if (IsFinalState(f.item))
                {
                    return dist[f.item];
                }
                //ext
                var zeroPos = f.item.MatrixFind(e => e == 0).First();
                foreach (var offset in direct)
                {
                    var nx = zeroPos.Item1 + offset.Item1;
                    var ny = zeroPos.Item2 + offset.Item2;
                    if(nx is < 0 or >= 4 || ny is < 0 or >= 4)
                        continue;
                    var newState = (ValueMatrix<int>) f.item.Clone();
                    Swap(newState, zeroPos.Item1, zeroPos.Item2, nx, ny);
                    if(closeSet.Contains(newState) || q.Values.Contains(newState))
                        continue;
                    dist[newState] = dist[f.item] + 1;
                    q.EnQueue(CalcHeuristic(newState) + dist[newState], newState);
                }
            }

            return -1;
        }
        private static bool IsCanColorized<T, TE>(BaseGraph<T, TE> graph, int k, int[] colored = null, int v = 0)
        {
            
            var n = graph.NodeCount;
            if (v == n)
                return true;
            if (n == 0)
                return true;
            if (k == 0)
                return false;
          
            colored ??= Enumerable.Repeat(-1, k).ToArray();
            
            for (var c = 0; c < k; c++)
            {
                //检测到v的着色是否合法
                if (!IsColorSafe(graph, v, c, colored)) 
                    continue;
                colored[v] = c;
                if (IsCanColorized(graph, k, colored, v + 1))
                {
                    return true;
                }

                colored[v] = -1;
            }
            
            

            return false;
        }
        public static int Partition<T>(T[] arr, int index, int s, int e) where T : IComparable
        {
            if (index < s || index > e)
                return -1;
            Swap(arr, index, e);
            var l = 0;
            var r = e - 1;
            var p = arr[e];
            while (l <= r)
            {
                if (arr[l].CompareTo(arr[r]) <= 0)
                {
                    l++;
                }
                else
                {
                    Swap(arr, l, r--);
                }
            }

            if (arr[l].CompareTo(p) < 0)
                l++;
            Swap(arr, l, e);
            return l;
        }

        public static void Swap<T>(T[] arr, int i, int j)
        {
            var t = arr[i];
            arr[i] = arr[j];
            arr[j] = t;
        }
        public static int BinaryCyclicSearch(int[] a, int n, int target){
            if(n<=0)
                return -1;
            var left = 0;
            var right = n-1;
            while(left <= right)
            {
                var mid = left + ((right - left) >> 1);
                if(a[mid] == target){
                    return mid;
                }
                
                //转折点在右半边
                if(a[left] <= a[mid])
                {
                    if(a[left] <= target && target < a[mid]){
                        right = mid - 1;
                    } else {
                        left = mid + 1;
                    }
                }
                else //转折点在左半边
                {
                    if(a[mid] < target && target <= a[right]){
                        left = mid + 1;
                    } else {
                        right = mid - 1;
                    }
                }
            }
            return -1;
        }
    }
}