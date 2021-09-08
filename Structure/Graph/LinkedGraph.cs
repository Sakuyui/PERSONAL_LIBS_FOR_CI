using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Structure.Graph
{
    public class LinkedGraph<NodeType, TEdgeType> : BaseGraph<NodeType,TEdgeType>
    {
        public override List<BaseEdge<TEdgeType>> Edges =>
            EdgeMap.SelectMany(e =>
                e.Value.Select(e2 => e2.Value)).ToList();

        public readonly List<HashSet<int>> NodesMap = new();

        
        public override int this[GraphNode<NodeType> node] => NodeIndexMap[node];
        public readonly Dictionary<GraphNode<NodeType>, int> NodeIndexMap = new();
        public readonly Dictionary<int, Dictionary<int, BaseEdge<TEdgeType>>> EdgeMap = new();

        public override GraphNode<NodeType> this[int nodeCode] => Nodes[nodeCode];

        public int AddNode(NodeType data)
        {
            var c = NodeCount;
            var node = new GraphNode<NodeType>(data, c);
            NodeIndexMap[node] = c;
            NodesMap.Add(new HashSet<int>());
            return c;
        }
        public int AddNode()
        {
            var c = NodeCount;
            var node = new GraphNode<NodeType>(default, c);
            NodeIndexMap[node] = c;
            NodesMap.Add(new HashSet<int>());
            return c;
        }
        public override void AddEdge(GraphNode<NodeType> startNode, GraphNode<NodeType> endNode, BaseEdge<TEdgeType> edge)
        {
           
            if (!NodeIndexMap.ContainsKey(startNode) || !NodeIndexMap.ContainsKey(endNode))
                return;
            var nodeIndex = NodeIndexMap[startNode];
            var endIndex = NodeIndexMap[endNode];
            NodesMap[nodeIndex].Add(endIndex);
            if (!EdgeMap.ContainsKey(nodeIndex))
                EdgeMap[nodeIndex] = new Dictionary<int, BaseEdge<TEdgeType>>();
            EdgeMap[nodeIndex][endIndex] = edge;
        }
        

        public override void DeleteEdge(GraphNode<NodeType> startNode, GraphNode<NodeType> endNode, BaseEdge<TEdgeType> edge)
        {
            if (!NodeIndexMap.ContainsKey(startNode) || !NodeIndexMap.ContainsKey(endNode))
                return;
            var nodeIndex = NodeIndexMap[startNode];
            var endIndex = NodeIndexMap[endNode];
            NodesMap[nodeIndex].Remove(endIndex);
            EdgeMap[nodeIndex].Remove(endIndex);
        }

        public override void ChangeEdge(GraphNode<NodeType> startNode, GraphNode<NodeType> endNode, BaseEdge<TEdgeType> edge)
        {
            if (!NodeIndexMap.ContainsKey(startNode) || !NodeIndexMap.ContainsKey(endNode))
                return;
            var nodeIndex = NodeIndexMap[startNode];
            var endIndex = NodeIndexMap[endNode];
            EdgeMap[nodeIndex][endIndex] = edge;
        }

        public override HashSet<GraphNode<NodeType>> GetExtendNodes(int i, NodesRepresentType type)
        {
            return NodesMap[i].Select(index => this[index]).ToHashSet();
        }

        public override int NodeCount => Nodes.Count;
        public override List<GraphNode<NodeType>> Nodes => NodeIndexMap.Keys.ToList();


        public override HashSet<int> GetExtendNodesIndex(int i, NodesRepresentType type)
        {
            return NodesMap[i].ToHashSet();
        }

        public override BaseEdge<TEdgeType> this[int s, int e]
        {
            get => EdgeMap[s][e];
            set => throw new System.NotImplementedException();
        }

        public override HashSet<GraphNode<NodeType>> GetExtendNodes(GraphNode<NodeType> node)
        {
            return GetExtendNodes(this[node], NodesRepresentType.ObjectType);
        }

        public override string ToString()
        {
            return NodesMap.GetMultiDimensionString();
        }
    }
}