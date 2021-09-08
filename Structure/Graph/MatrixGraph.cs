using System.Collections.Generic;
using System.Linq;
using CIExam.Math;

namespace CIExam.Structure.Graph
{
    public class MatrixGraph<TNodeType, TEdgeType> : BaseGraph<TNodeType, TEdgeType>
    {
        public override int NodeCount => Nodes.Count;
        public int EdgeCount => Edges.Count;
        
        //索引 -> 节点
        public override List<GraphNode<TNodeType>> Nodes => _nodesList;
        public override List<BaseEdge<TEdgeType>> Edges => 
            _matrix.SelectMany(e => e).Where(e => e != null).ToList();
        private Matrix<BaseEdge<TEdgeType>> _matrix;


        public override GraphNode<TNodeType> this[int nodeIndex] => Nodes[nodeIndex];

        public override int this[GraphNode<TNodeType> node] => _nodeMapping[node];

        //编号对应节点
        private List<GraphNode<TNodeType>> _nodesList = new List<GraphNode<TNodeType>>();
        
        //节点对应的编号
        private Dictionary<GraphNode<TNodeType>, int> _nodeMapping = new Dictionary<GraphNode<TNodeType>, int>();

        public MatrixGraph()
        {
            _matrix = new Matrix<BaseEdge<TEdgeType>>(0,0);
        }

        
        public void Dfs(NodeAccessStrategy accessStrategy, int from)
        {
            var n = _nodesList.Count;
            var stack = new Stack<GraphNode<TNodeType>>();
            var vis = new bool[n];
            stack.Push(_nodesList[from]);
            while (stack.Count != 0)
            {
                var p = _nodeMapping[stack.Peek()];
                if (!vis[p])
                {
                    vis[p] = true;
                    accessStrategy.Invoke(stack.Peek());
                }
                var s = GetExtendNodes(stack.Peek());
                if (s.Count == 0 || s.All(e => vis[_nodeMapping[e]]))
                {
                    stack.Pop();
                    continue;
                }
                var np = s.First(e => vis[_nodeMapping[e]]);
                stack.Push(np);
            }
        }

        
        

        public void Bfs(NodeAccessStrategy accessStrategy, int from)
        {
            var n = _nodesList.Count;
            var queue = new Queue<int>();
            queue.Enqueue(from);
            var vis = new bool[n];
            while (queue.Count != 0)
            {
                var p = queue.Dequeue();
                vis[p] = true;
                accessStrategy.Invoke(_nodesList[p]);
                var ext = GetExtendNodes(_nodesList[p]).Where(e => !vis[_nodeMapping[e]]);
                foreach (var e in ext)
                {
                    queue.Enqueue(_nodeMapping[e]);
                }
            }
        }
        
        
    
        //空白图
        public MatrixGraph(int nodeCount)
        {
            _matrix = new Matrix<BaseEdge<TEdgeType>>(nodeCount,nodeCount);
            for (var i = 0; i < nodeCount; i++)
            {
                var n = new GraphNode<TNodeType>(default, i + 1);
                _nodesList.Add(n);
                _nodeMapping.Add(n, i + 1);
            }
        }


        public GraphNode<TNodeType> AddNode(TNodeType data)
        {
            var node = new GraphNode<TNodeType>(data,_matrix.ColumnsCount + 1);
            var len = _nodeMapping.Count;
            _matrix.AddARow();
            _matrix.AddColumn();
            _nodeMapping.Add(node,len);
            _nodesList.Add(node);
            return node;
        }
        
        public List<GraphNode<TNodeType>> CreateFromNodes(IEnumerable<TNodeType> nodes)
        {
            //编号与nodes同顺序
            var cur = 0;
            var res = nodes.Select(n => new GraphNode<TNodeType>(n, cur++)).ToList();
            var len = res.Count;
            _matrix = new Matrix<BaseEdge<TEdgeType>>(len,len);
            _nodeMapping = null;
            _nodeMapping = res.ToDictionary(x => x, x => x.NodeCode);
            _nodesList = null;
            _nodesList = res;
            return res;
        } 
        

        public override BaseEdge<TEdgeType> this[int node1, int node2]
        {
            set
            {
                var n1 = _nodesList[node1];
                var n2 = _nodesList[node2];
                if (n1 == null || n2 == null) return;
                _matrix[node1, node2] = value;
            }
            get
            {
                var nodeCount = _nodeMapping.Count;
                if (node1 >= nodeCount || node2 >= nodeCount) return null;
                return _matrix[node1, node2];
            }
        }
        
        public override void AddEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge)
        {
            var nI1 = _nodeMapping[startNode];
            var nI2 = _nodeMapping[endNode];
            edge.From = nI1;
            edge.To = nI2;
            if (nI1 < 0 || nI2 < 0) return;
            this[nI1, nI2] = edge;
        }

        public override void DeleteEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge)
        {
            var nI1 = _nodeMapping[startNode];
            var nI2 = _nodeMapping[endNode];
            if (nI1 < 0 || nI2 < 0) return;
            this[nI1, nI2] = default;
        }

        public override void ChangeEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge)
        {
            AddEdge(startNode,endNode,edge);
        }

        
        //从某点到达可能的所有节点
        

        public override System.Collections.Generic.HashSet<GraphNode<TNodeType>> GetExtendNodes(
            GraphNode<TNodeType> node)
        {
            var set = new System.Collections.Generic.HashSet<GraphNode<TNodeType>>();
            var code = _nodeMapping[node];
            if (code < 0) return set;

            var r = _matrix[code];
            for (var i = 0; i < r.Count; i++)
            {
                if (r[i] != null) 
                    set.Add(_nodesList[i]);
            }
            return set;
        }

        public override HashSet<int> GetExtendNodesIndex(int i, NodesRepresentType type)
        {
            return GetExtendNodes(i, type).Select(e => this[e]).ToHashSet();
        }


        public override System.Collections.Generic.HashSet<GraphNode<TNodeType>> GetExtendNodes(int node,
            NodesRepresentType type = NodesRepresentType.CodeType)
        {
            
            return GetExtendNodes(this[node]);
        }

      
    }
}