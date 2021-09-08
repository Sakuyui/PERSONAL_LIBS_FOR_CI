using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Structure
{

    public static class PriorityQueueTest
    {
        public static void Test()
        {
            var priorityQueue = new PriorityQueue<int, (int, double)>((e1, e2) => e2 - e1);
            priorityQueue.EnQueue(2, (2, 45.6));
            priorityQueue.EnQueue(3, (3, 48.6));
            priorityQueue.EnQueue(0, (8, 41.6));
            foreach (var e in priorityQueue)
            {
                ("(" + e.Item1 + "," + e.Item2 + ")").PrintToConsole();
            }

        }
    }
    
 
    public class PriorityQueue<TPriority,TItem>  : IEnumerable<TItem>
    //where TPriority: IComparable
    {
        
        SortedList<TPriority, TItem> _sortedList;
        public IEnumerable<KeyValuePair<TPriority, TItem>> KvEnumerator => 
            _sortedList.Select(e => e);

        public IEnumerable<TPriority> Keys => _sortedList.Keys;
        public IEnumerable<TItem> Values => _sortedList.Values;

        public KeyValuePair<TPriority, TItem> Peek()
        {
            return _sortedList.First();
        }
        public void UpdateOrSetPriority(TItem item, TPriority priority)
        {
            if (ContainsValue(item))
            {
                RemoveByItem(item);
            }
            
            EnQueue(priority, item);
        }
        public PriorityQueue(DuplicateKeyComparer<TPriority>.ComparerStrategy comparerStrategy = null)
        {
            _sortedList = new SortedList<TPriority, TItem>(new DuplicateKeyComparer<TPriority>(comparerStrategy));
        }

        public void Clear()
        {
            _sortedList.Clear();
        }
        public bool ContainsKey(TPriority priority)
        {
            return _sortedList.ContainsKey(priority);
        }
        
        public bool ContainsValue(TItem item)
        {
            return _sortedList.ContainsValue(item);
        }

        public void RemoveByItem(TItem item)
        {
            if(_sortedList.ContainsValue(item))
                _sortedList.RemoveAt(_sortedList.IndexOfValue(item));
        }
        public void RemoveByKey(TPriority priority)
        {
            if(_sortedList.ContainsKey(priority))
                _sortedList.RemoveAt(_sortedList.IndexOfKey(priority));
        }
        
        public void EnQueue(TPriority priority, TItem e)
        {
            _sortedList.Add(priority, e);
        }
        
        
        public (TItem item, TPriority priority) DeQueue()
        {
            if (_sortedList.Count == 0)
                return default;
            var v = _sortedList.Values[0];
            var p = _sortedList.Keys[0];
            _sortedList.RemoveAt(0);
            return (v, p);
        }

        public (TItem item, TPriority priority) DeQueueFromTail()
        {
            var c = _sortedList.Count;
            if (c == 0)
                return default;
            var v = _sortedList.Values[c - 1];
            var p = _sortedList.Keys[c - 1];
            _sortedList.RemoveAt(c - 1);
            return (v, p);
        }


        public (TPriority, TItem) GetFullEntryByItem(TItem item)
        {
            if (ContainsValue(item))
            {
                var index = _sortedList.IndexOfValue(item);
                return (_sortedList.Keys[index], _sortedList.Values[index]);
            }

            return (default, default);
        }
     

        public IEnumerator<TItem> GetEnumerator()
        {
            return _sortedList.Select(e => e.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}