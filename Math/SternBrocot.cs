using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;

namespace CIExam.Math
{
    public class SternBrocot
    {
        public class SBNode : BinaryTreeNode<((int up, int down) left, (int up, int down) right)>
        {
            public SBNode(((int up, int down) left, (int up, int down) right) value) : base(value)
            {
            }
        }

        public SBNode Root = new SBNode(((0, 1),(1, 0)));
        public SternBrocot(int layarMax = 10)
        {
            var q = new Queue<SBNode>();
            q.Enqueue(Root);
            var layer = 0;
            while (q.Any())
            {
                if(layer >= layarMax)
                    break;
                var size = q.Count;
                for (var i = 0; i < size; i++)
                {
                    var f = q.Dequeue();
                    var a = f.Data.left.up;
                    var b = f.Data.left.down;
                    var c = f.Data.right.up;
                    var d = f.Data.right.down;
                    var lNode = new SBNode(((a, b),(a + c, b + d)));
                    var rNode = new SBNode(((a + c, b + d),(c, d)));
                    f.Left = lNode;
                    f.Right = rNode;
                    q.Enqueue(lNode);
                    q.Enqueue(rNode);
                }

                layer++;
            }
        }
    }
}