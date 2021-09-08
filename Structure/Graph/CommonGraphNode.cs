using System.Collections.Generic;

namespace CIExam.Structure.Graph
{
    public class CommonGraphNode<T>
    {
        public List<CommonGraphNode<T>> NextNodes = new();
        public T Data;
    }
}