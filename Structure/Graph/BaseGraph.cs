using System.Collections.Generic;
using C5;

namespace CIExam.Structure.Graph
{
    public abstract class BaseGraph<TNodeType, TEdgeType>
    {
        
        public delegate object NodeAccessStrategy(GraphNode<TNodeType> node);
        public delegate BaseEdge<TEdgeType> EdgeAddStrategy(GraphNode<TNodeType> sN, GraphNode<TNodeType> eN, BaseGraph<TNodeType, TEdgeType> graph);
        public abstract void AddEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge);

        public void AddDoubleEdge(int node1, int node2, TEdgeType data)
        {
            AddEdge(node1, node2, data);
            AddEdge(node2, node1, data);
        }
        public void AddEdge(int startNode, int endNode, BaseEdge<TEdgeType> edge)
        {
            AddEdge(this[startNode], this[endNode], edge);
        }
        public void AddEdge(int startNode, int endNode, TEdgeType edgeData)
        {
            AddEdge(this[startNode], this[endNode], new GraphEdge<TEdgeType>(){Data = edgeData});
        }
        public void AddEdge(int startNode, int endNode)
        {
            AddEdge(this[startNode], this[endNode], new GraphEdge<TEdgeType>());
        }

        public abstract void DeleteEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge);

        public void DeleteEdge(int startNode, int endNode, BaseEdge<TEdgeType> edge)
        {
            DeleteEdge(this[startNode], this[endNode], edge);
        }

        public abstract void ChangeEdge(GraphNode<TNodeType> startNode, GraphNode<TNodeType> endNode, BaseEdge<TEdgeType> edge);
        public abstract System.Collections.Generic.HashSet<GraphNode<TNodeType>> GetExtendNodes(
            int i, NodesRepresentType type);

        
        public abstract int NodeCount { get; }
        public abstract List<GraphNode<TNodeType>> Nodes { get; }
        public abstract List<BaseEdge<TEdgeType>> Edges { get; }
        public abstract System.Collections.Generic.HashSet<GraphNode<TNodeType>> GetExtendNodes(GraphNode<TNodeType> node);
        public abstract System.Collections.Generic.HashSet<int> GetExtendNodesIndex(int i, NodesRepresentType type);
        public abstract BaseEdge<TEdgeType> this[int s, int e] { set; get; }
        public abstract int this[GraphNode<TNodeType> node] { get; }
        public abstract GraphNode<TNodeType> this[int nodeCode] { get; }
      
    }
    public enum NodesRepresentType
    {
        ObjectType,
        CodeType
    }
}