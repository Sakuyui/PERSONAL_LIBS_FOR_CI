using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;

namespace CIExam.Structure
{
    public class PriorityDynamicQueue<TKey, TP> : IEnumerable<PriorityDynamicQueue<TKey, TP>.DNode>
    {
        public class DNode
        {
            public TKey Key;
            public TP Priority;
            public IPriorityQueueHandle<DNode> Handle = null;
            public DNode(TKey key, TP priority)
            {
                Key = key;
                Priority = priority;
            }

            public override string ToString()
            {
                return $"({Key}, {Priority})";
            }

            public override bool Equals(object? obj)
            {
                if (obj is DNode node)
                {
                    return Key.Equals(node.Key);
                }
                
                return false;
                //return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }
        private IComparer<DNode> _comparer = null;
        private IntervalHeap<DNode> _heap;
        private readonly Dictionary<TKey, DNode> _existSet = new();
        public IEnumerable<DNode> NodeEnumerator => _heap.Select(e => e);
        public PriorityDynamicQueue(IComparer<TP> comparer = null)
        {
            _comparer = new CustomerComparer<DNode>(delegate(DNode t1, DNode t2)
            {
                if (comparer != null)
                {
                    var cmp = comparer.Compare(t1.Priority, t2.Priority);
                    return  cmp == 0 ? - 1: cmp;
                }
                else
                {
                    var cmp = ((IComparable) t1.Priority).CompareTo(t2.Priority);
                    return cmp == 0 ? -1 : cmp;
                }
            });
            _heap = new IntervalHeap<DNode>(_comparer);
        }

        public (TKey val, TP priority) GetMax()
        {
            var max = _heap.FindMax();
            return (max.Key, max.Priority);
        }

        public (TKey val, TP priority) GetMin()
        {
            var min = _heap.FindMin();
            return (min.Key, min.Priority);
        }

        public (TKey val, TP priority) RemoveMax()
        {
            var max = _heap.DeleteMax();
            _existSet.Remove(max.Key);
            return (max.Key, max.Priority);
        }

        public (TKey val, TP priority) RemoveMin()
        {
            var min = _heap.DeleteMin();
            _existSet.Remove(min.Key);
            return (min.Key, min.Priority);
        }
        public bool Contains(TKey key)
        {
            return _existSet.ContainsKey(key);
        }

        public bool Update(TKey key, TP newPriority)
        {
            if (!_existSet.ContainsKey(key)) 
                return false;
            var node = _existSet[key];
            _heap.Delete(node.Handle);
            node.Handle = null;
            node.Priority = newPriority;
            _heap.Add(ref node.Handle, node);

            return false;
        }

        public void AddOrUpdate(TKey key, TP priority)
        {
            if (_existSet.ContainsKey(key))
                Update(key, priority);
            else
            {
                var node = new DNode(key, priority);
                _existSet[key] = node;
                _heap.Add(ref node.Handle, node);
            }
        }


        public void Clear()
        {
            _heap = new IntervalHeap<DNode>();
            _existSet.Clear();
        }

        public IEnumerator<DNode> GetEnumerator()
        {
            return _heap.Select(e => e).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}