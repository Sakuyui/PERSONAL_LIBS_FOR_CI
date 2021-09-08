using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{
    public class MultiTreeNodeNode<TKey, TValue> : CommonTreeNode<TValue>
    {
        private List<AbstractTreeNode<TValue>> _children => Children;

        public IEnumerable<MultiTreeNodeNode<TKey, TValue>> ChildrenEnumerator =>
            _children.Cast<MultiTreeNodeNode<TKey, TValue>>();
        
        public TKey Key;
        public TValue Val
        {
            get => base.Data;
            set => base.Data = value;
        }

        public MultiTreeNodeNode() : base(default)
        {
            
        }
        public MultiTreeNodeNode(TKey k) : base(default)
        {  
            Key = k;
        }
        public MultiTreeNodeNode(TKey k, TValue val) : base(default)
        {  
            Key = k;
            Val = val;
        }
        public void Insert(TKey key, TValue val)
        {
            Children.Add(new MultiTreeNodeNode<TKey, TValue>(key, val));
        }
        public void Insert(TKey key)
        {
            Children.Add(new MultiTreeNodeNode<TKey, TValue>(key));
        }
        public void Insert(MultiTreeNodeNode<TKey, TValue> t)
        {
            Children.Add(t);
        } 
    }
}