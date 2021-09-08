using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Structure
{
    public static class RangeTreeTest
    {
        public static void Test()
        {
            //范围树，支持任意维度！
            var rangeTree = new SortedRangeTreeMd<string>(3) 
                {{"a", 10, 2, 9}, {"b", 5, 4, 92}, {"c", 7, 14, 4}, {"d", 28, 24, 12}};
            
            rangeTree.PrintEnumerationToConsole();
            //范围查询 至此任意维度。参数个数必须为偶数（因为每个维度要指定上下界）

            
            //惹，支持【任意可以比较的类型（实现IComparable接口）】作为类型。
            var doubleRangeTree = new SortedRangeTreeMd<string>(2)
            {
                { "a", 2.4, 5.2}, {"b", 3.4, 5.3}, {"c", 8.2, 2.3}, {"d", 3.5, 4.7}
            };
            doubleRangeTree.PrintEnumerationToConsole();
            doubleRangeTree.Query(2.1, 4.0).PrintCollectionToConsole();
            doubleRangeTree.Query(2.1, 4.0, 4.8, 5.4).PrintEnumerationToConsole();

            
        }
    }
    public class SortedRangeTreeMd<TVal> : IEnumerable<(ValueTupleSlim key, TVal val)>
    {
        private int DimensionCount { get; }


        //书套树惹。。可能是<Key, 树> 也可能是 <Key, 真实val>。。所以这里用dynamic处理。
        
        private readonly SortedList<dynamic, dynamic> _data = new(
            new CustomerComparer<dynamic>(delegate(dynamic t1, dynamic t2)
                {
                    var c1 = (IComparable) t1;
                    var c2 = (IComparable) t2;
                    var cmp = c1.CompareTo(c2);
                    return cmp == 0 ? 1 : cmp;
                }));
        
        public SortedRangeTreeMd(int dim)
        {
            DimensionCount = dim;
        }

        public void Remove(TVal val)
        {
            if (DimensionCount == 1)
            {
                if(_data.ContainsKey(val))
                    _data.Remove(val);
            }
            else
            {
                _data.ElementInvoke(e =>
                {
                    var tree = (SortedRangeTreeMd<TVal>) e.Value;
                    tree.Remove(val);
                });
            }
        }
        private int BinarySearchLeft(object t)
        {
            var arr = _data.ToArray();
            var n = arr.Length;
            var l = 0;
            var r = n - 1;
            while (l <= r)
            {
                var m = (l + r) >> 1;
                if (((IComparable)arr[m].Key).CompareTo(t) >= 0)
                {
                    r = m - 1;
                }
                else
                {
                    l = m + 1;
                }
            }

            return l;
        }


       
        
        private int BinarySearchRight(object t)
        {
            var arr = _data.ToArray();
            var n = arr.Length;
            var l = 0;
            var r = n - 1;
            while (l <= r)
            {
                var m = (l + r) >> 1;
                if (((IComparable)arr[m].Key).CompareTo(t) <= 0)
                {
                    l = m + 1;
                }
                else
                {
                    r = m - 1;
                }
            }

            return r;
        }
        
        public IEnumerable<(ValueTupleSlim key, TVal val)> Query(params object[] ranges)
        {
            var n = ranges.Length;

            if (ranges.Length == 0)
                return this;

            if (n % 2 != 0)
                throw new Exception("ranges count should be even");
            
            var l = (IComparable)ranges[0];
            var r = (IComparable)ranges[1];
            
            var leftRange = BinarySearchLeft(l);
            var rightRange = BinarySearchRight(r);
            
            if (r.CompareTo(l) < 0)
                return new List<(ValueTupleSlim key, TVal val)>();
            
            if (DimensionCount == 1)
            {
                return _data.Skip(leftRange).Take(rightRange - leftRange + 1)
                    .Select(t => (new ValueTupleSlim(t.Key), (TVal) t.Value));
            }
            
            //需要递归操作
            var sub = _data.Skip(leftRange).Take(rightRange - leftRange + 1)
                .SelectMany(t =>
                {
                    var (key, value) = t;
                    var subTree = (SortedRangeTreeMd<TVal>) value;
                    var s = subTree.Query(ranges.Skip(2).ToArray())
                        .Select(e => (new ValueTupleSlim(e.key.Prepend((object)key).ToArray()), e.val));
                    return s;
                });

            return sub;
        }
        
        
        public void Add(TVal val, params object[] objects)
        {
            if (objects.Length == 0)
                return;
            if (objects.Length != DimensionCount)
                throw new Exception("Params Length not equal to RangeTree's dimension");
           
            //一维就是普通的树。直接加入键值对就行
            if(DimensionCount == 1)
                _data.Add(objects[0], val);
            else //维度大于2，需要处理子树
            //注意，在非最终维度时，Sortedlist存储的都是k - 1维子树
            {
                var key = objects[0];
                if (!_data.ContainsKey(key))
                {
                    var newTree = new SortedRangeTreeMd<TVal>(DimensionCount - 1) {{val, objects.Skip(1).ToArray()}};
                    _data.Add(key, newTree);
                }
                else
                {
                    var tree = (SortedRangeTreeMd<TVal>)_data[key];
                    tree.Add(val, objects.Skip(1).ToArray());
                }
            }
        }

        public IEnumerator<(ValueTupleSlim key, TVal val)> GetEnumerator()
        {
            var dim = DimensionCount;
            if (dim == 1)
                return _data.Select(t => (key: new ValueTupleSlim(t.Key), val: (TVal) t.Value))
                    .GetEnumerator();
            
            var ans = _data
                .SelectMany(curKv => ((SortedRangeTreeMd<TVal>) curKv.Value)
                    .Select(e =>
                    {
                        var (key, val) = e;
                        var tuple = new ValueTupleSlim(key.Prepend((object) curKv.Key).ToArray()); //这里相当于获取递归迭代器惹
                        var t = (tuple, val);
                        return t;
                    }));
            
            return ans.GetEnumerator();
            
        }



        public IEnumerable<(ValueTupleSlim key, TVal val)> this[params Range[] ranges]
        {
            get
            {
                var n = ranges.Length;
                var objects = new object[2 * n];
                for (var i = 0; i < n; i++)
                {
                    objects[i * 2] = ranges[i].Start.Value;
                    objects[i * 2 + 1] = ranges[i].End.Value;
                }

                return Query(objects);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}