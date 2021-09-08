using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{
    public abstract class AbstractTreeNode<T>
    {
        public T Data { get; set; }

        public AbstractTreeNode(T data)
        {
            Data = data;
        }
        public delegate object VisitStrategy(AbstractTreeNode<T> data, params object[] objects);

        public abstract object OrderDfs(VisitStrategy visitStrategy, int order, params object[] objects);
        
        public readonly List<AbstractTreeNode<T>> Children = new();
        public int ChildrenCount => Children.Count;

        public bool IsLeaf()
        {
            return ChildrenCount == 0;
        }
    }

    public class CommonTreeNode<T> : AbstractTreeNode<T>
    {
        public CommonTreeNode(T data) : base(data)
        {
        }

        public IEnumerable<AbstractTreeNode<T>> PreorderEnumerator => GetPreorderEnumerator();
        
        public IEnumerable<AbstractTreeNode<T>> PosterOrderEnumerator => GetPosterOrderEnumerator();
        
        private IEnumerable<AbstractTreeNode<T>> GetPreorderEnumerator()
        {
            var ans = new List<AbstractTreeNode<T>>();
            PreOrderDfs((data, objects) =>
            {
                ((List<AbstractTreeNode<T>>)objects[0]).Add(data);
                return null;
            }, ans);
            return ans;
        }
        private IEnumerable<AbstractTreeNode<T>> GetPosterOrderEnumerator()
        {
            var ans = new List<AbstractTreeNode<T>>();
            PostOrderDfs((data, objects) =>
            {
                ((List<AbstractTreeNode<T>>)objects[0]).Add(data);
                return null;
            }, ans);
            return ans;
        }
        public override object OrderDfs(VisitStrategy visitStrategy, int order, params object[] objects)
        {
            if (order >= 1 + ChildrenCount)
                return null;
            for (var i = 0; i < order; i++)
            {
                Children[i].OrderDfs(visitStrategy, order, objects);
            }
            

            for (var i = order + 1; i <= ChildrenCount; i++)
            {
                Children[i - 1].OrderDfs(visitStrategy, order, objects);
            }

            return null;
        }

        public object PostOrderDfs(VisitStrategy visitStrategy, params object[] objects)
        {
            foreach (var node in Children)
            {
                ((CommonTreeNode<T>) node)?.PostOrderDfs(visitStrategy, objects);
            }
            return visitStrategy.Invoke(this, objects);
        }
        public object PreOrderDfs(VisitStrategy visitStrategy, params object[] objects)
        {
            visitStrategy.Invoke(this, objects);
            foreach (var node in Children)
            {
                ((CommonTreeNode<T>) node)?.PreOrderDfs(visitStrategy, objects);
            }

            return null;
        } 
        
        
        
    }
}