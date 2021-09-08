using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{


    public class SkipTableTest
    {
        public static void Test()
        {
            var skipTable = new SkipTable<int, int>();
        }
    }
    public class SkipTable<TKey, TVal> where TKey : IComparable
    {

        public class SkipTableNode
        {
            public TVal Content;
            public TKey Key;
           
            public bool IsTail = false;
            public bool IsHead = false;
            public SkipTableNode RightNode = null;
            public SkipTableNode DownNode = null;

            public SkipTableNode(TKey key, TVal content)
            {
                Content = content;
                Key = key;
            }

        }
        public readonly int MaxLayer;
        private readonly List<SkipTableNode> _layers = new();
        public SkipTable(int maxLayer = 4)
        {
            MaxLayer = maxLayer;
        }

        public TVal Search(TKey key)
        {
            var curNode = _layers[^1];
            var curLayer = _layers.Count - 1;
            while (curLayer >= 0)
            {
                if (!curNode.IsHead && !curNode.IsTail && curNode.Key.CompareTo(key) == 0)
                {
                    return curNode.Content;
                }
                if (curNode.RightNode.IsTail)
                {
                    if (curLayer == 0)
                        return default;
                    curLayer--;
                    curNode = curNode.DownNode;
                }else if (key.CompareTo(curNode.RightNode.Key) > 0)
                {
                    curNode = curNode.RightNode;
                }
                else
                {
                    if (curLayer == 0)
                        return curNode.Content;
                    curLayer--;
                    curNode = curNode.DownNode;
                }
            }

            return default;
        }

        public void Insert(TKey key, TVal val, int height)
        {
            if (height > MaxLayer || height <= 0)
                throw new Exception();
            if (!_layers.Any())
            {
             
                SkipTableNode prevData = null;
                SkipTableNode prevHead = null;
                SkipTableNode prevTail = null;
                //创建首->数据->尾结构
                for (var i = 0; i < height; i++)
                {
                    var tail = new SkipTableNode(default, default) {IsTail = true, DownNode = prevTail};
                    prevTail = tail;
                    var dataNode = new SkipTableNode(key, val){DownNode = prevData};
                    prevData = dataNode;
                    var head = new SkipTableNode(default, default)
                    {
                        IsHead = true,
                        DownNode = prevHead
                    };
                    prevHead = head;
                    _layers.Add(head); //注意，最终越高列表索引号代表越高层
                }
            }
            else
            {
                var curNode = _layers[^1];
                var curLayer = _layers.Count - 1;
                //注意，在层上下移动时，需要根据当前欲加入的层数记录节点。 左右移动不需要记录，上下移动需要。并且只需要记录符合高度内层数的节点。
                //因此最终记录集合中一定恰好有高度个节点
                var record = new List<SkipTableNode>();
                while (curLayer >= 0)
                {
                    if (!curNode.RightNode.IsTail)
                    {
                        if (curNode.RightNode.Key.CompareTo(key) < 0)
                        {
                            curNode = curNode.RightNode;
                        }
                        else
                        {
                            if (curLayer <= height)
                            {
                                record.Add(curNode);
                            }
                            if (curLayer == 0)
                            {
                                SkipTableNode prev = null;
                                //需要当前和下一个节点之间插入惹
                                //比较复杂，需要记录经过的层。
                                foreach (var t in record)
                                {
                                    var newNode = new SkipTableNode(key, val)
                                    {
                                        DownNode = prev, RightNode = t.RightNode
                                    };
                                    t.RightNode = newNode;
                                    prev = newNode;
                                }
                                break;
                            }

                            //还可能存在更小的。继续
                            curLayer--;
                            curNode = curNode.DownNode;

                        }
                    }
                    else
                    {
                        if (curLayer <= height)
                        {
                            record.Add(curNode);
                        }
                        if (curLayer == 0)
                        {
                            SkipTableNode prev = null;
                            //边界情况。在最尾部插入
                            foreach (var t in record)
                            {
                                var newNode = new SkipTableNode(key, val)
                                {
                                    DownNode = prev, RightNode = t.RightNode
                                };
                                t.RightNode = newNode;
                                prev = newNode;
                            }
                            break;
                        }

                        curLayer--;
                        curNode = curNode.DownNode;

                    }
                }
            }
        }
    }
}