using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Network.TCP
{
    //需要使用连续区间结构
    public class SelRetransmitStructure
    {
        private bool _begin = false;
        private int WindowBegin = 0;
        public int WindowSize;
        private int wLast = 0;
        private HashSet<int> set = new HashSet<int>();
        public SelRetransmitStructure(int wSize)
        {
            WindowSize = wSize;
        }
        public void Begin(int ackNumberBegin = 0)
        {
            _begin = true;
        }

        public void End()
        {
            _begin = false;
            
        }

        private readonly ContinuousRange _continueRange = new ();
        public int Offer(int ack)
        {
            _continueRange.Offer(ack);
            if (ack != WindowBegin) 
                return ack;
            var list = _continueRange.GetAndRemove(WindowBegin);
            if (list == null)
                throw new Exception();
            WindowBegin += list.Count;
            //output list there

            return ack;
        }
    }

    class ContinuousRange
    {
        private readonly Dictionary<List<int>, (int l, int r)> ranges = new();

        //获取并删除以seqBegin开头的连续区间
        public List<int> GetAndRemove(int seqBegin)
        {
            if (ranges.Any(r => r.Value.l == seqBegin))
            {
                var k = ranges.First(r => r.Value.l == seqBegin);
                ranges.Remove(k.Key);
                return k.Key;
            }

            return null;
        }
        public void Offer(int x)
        {
            //只能接到一个头或者尾
            if (ranges.Any(r => r.Value.l == x + 1 || r.Value.r == x - 1))
            {
                var kv = ranges.First(r => r.Value.l == x + 1 || r.Value.r == x - 1);
                var (l, r) = kv.Value;
                var list = kv.Key;
                ranges.Remove(kv.Key);
                if (kv.Value.l == x + 1)
                {
                    kv.Key.Insert(0, x);
                    l--;
                }
                else
                {
                    kv.Key.Add(x);
                    r++;
                }

                //merge的可能性有
                while (true)
                {
                    //合并右侧
                    if (ranges.Any(range => range.Value.l == r + 1))
                    {
                        var f = ranges.First(range => range.Value.l == r + 1);
                        var rList = f.Key;
                        var rRange = f.Value;
                        ranges.Remove(f.Key);
                        list.AddRange(rList);
                        r = rRange.r;
                    }else if (ranges.Any(range => range.Value.r == l - 1)) { //合并左侧
                        var f = ranges.First(range => range.Value.l == r + 1);
                        var lList = f.Key;
                        var lRange = f.Value;
                        ranges.Remove(f.Key);
                        list.InsertRange(0,lList);
                        l = lRange. l;
                    }
                    else
                    {
                        break;
                    }
                }

                ranges[list] = (l, r);
            }
            else //无法连接到现存连续区间
            {
                var list = new List<int> {x};
                ranges.Add(list, (x, x));
            }
        }
    }
}