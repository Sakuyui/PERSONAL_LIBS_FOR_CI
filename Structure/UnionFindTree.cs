using System.Linq;

namespace CIExam.Structure
{
    public class UnionFindTreeWithData<TNodeType> : UnionFindTree
    {
        public readonly TNodeType[] DataOfSet;
        public UnionFindTreeWithData(int size): base(size)
        {
            DataOfSet = new TNodeType[size];
        }
    }
    public class UnionFindTree
    {
        public readonly int[] Root;
        public readonly int Size;
        public readonly int[] Rank;
        
        public UnionFindTree(int size)
        {
            Size = size;
            Root = Enumerable.Range(0, size).ToArray();
            Rank = new int[size];
        }

        public int Find(int u)
        {
            if (u >= Size)
                return -1;
            if (Root[u] == u)
                return u;

            //找到祖先
            var p = u;
            while (Root[p] != p)
                p = Root[p];
            //路径压缩
            while (u != p)
            {
                var t = Root[u];
                Root[u] = p; //向上迭代，每个节点都接到父节点
                u = t;
            }

            return Root[u];

            //return _root[u] = Find(u);
        }

        public void Merge(int u, int v)
        {
            var r1 = Find(u);
            var r2 = Find(v);
            if (r1 == r2 || r1 < 0 || r2 < 0)
                return;
            if (Rank[r1] > Rank[r2])
            {
                Root[r2] = r1;
                //_rank[r2] += _rank[r1];
            }else if (Rank[r1] == Rank[r2])
            {
                Root[r1] = r2;
                Rank[r2]++;
            }
        }
        
    }
}