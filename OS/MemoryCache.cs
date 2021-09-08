using System;
using System.Collections.Generic;
using CIExam.os.Cache;

namespace CIExam.OS
{
    
    // cache结构为 | 主存tag部分(末尾是相连度E) || (组号) | 块号 |偏移   （tag | index | 行内偏移）
    //每个组相当于是独立运行cache策略的。组内有多行。未满时可以选择任一行。满了就替换。
    //组里面包含map。key就是tag。
    //在每组里，遍历tag看看是否有符合的（读取时）
    //对于组相连（一组里有多行）index找到的是组。再通过tag在组里找到对应行。然后通过块号和偏移找到指定数据
    
    public class MemoryCacheTest
    {
        public static void PrintBits(int a)
        {
            Stack<byte> stack = new Stack<byte>();
            for (var i = 0; i < 32; i++)
            {
                stack.Push( (byte)(a & 1));
                a >>= 1;
            }

            while (stack.Count != 0)
            {
                Console.Write(stack.Pop());
            }
            Console.WriteLine();
        }
        public static void Test()
        {
            //2路组相连，共8组，每行4块，每块4字节 =》一共16行惹
            var memoryCache = new MemoryCache(2,8,4,4);
            memoryCache.Read(198,null);
        }
    }
    public class MemoryCache
    {
        private int _size => _e * _s * _b * _bs;
        private int _e; //相连度
        private int _s; //组数
        private int _b;  //块数
        private int _bs; //块大小
        private CacheGroup[] _cacheGroups;
     
        public MemoryCache(int e, int s, int b, int bs)
        {
            if((e & (e - 1)) !=0 || (s & (s - 1)) != 0 || (b & b - 1) != 0 || (bs & bs - 1) != 0)
                throw new ArithmeticException();
            _e = (int)System.Math.Log(e,2);
            _s = (int)System.Math.Log(s,2);
            _b = (int)System.Math.Log(b,2);
            _bs = (int)System.Math.Log(bs,2);
            _cacheGroups = new CacheGroup[s];
            for (var i = 0; i < s; i++)
            {
                _cacheGroups[i] = new CacheGroup(e, b * bs); //每行大小是块数*大小
            }
        }

        public delegate CaCheLine ReadFromSecondStorage(int addr);
        public byte Read(int addr,ReadFromSecondStorage loadStrategy)
        {
            var cacheBitLen = _s + _b + _bs;
            var ge = (int)((addr >> cacheBitLen) % System.Math.Pow(2,_e)) ; //组内的第几行
            var cachePart = ~((~0) << (cacheBitLen)) & addr;
            var s = cachePart >> (_b + _bs); //第几组
            var b = (cachePart - (s << (_b + _bs))) >> _bs;
            MemoryCacheTest.PrintBits(addr);
            MemoryCacheTest.PrintBits(cachePart);
            var bs = cachePart - (s << (_b + _bs)) - (b << _bs);
            Console.WriteLine("组内第: " + ge + " 行，第" + s +"组,第" +b+"块，第" + bs + "位");
            Console.WriteLine("tag = " + (addr >> cacheBitLen));
            var tag = (addr >> cacheBitLen);
            //TODO
            var g = _cacheGroups[s];
            var line = g.SearchLineWithTag(tag);
            if (line != null)
            {
                return line[b * _bs + bs];
            }
            else
            {
                line = loadStrategy(addr);
                g.Write(tag, line);
                return line[b * _bs + bs];
            }
            
        }

        public CacheGroup this[int gIndex] => _cacheGroups[gIndex];

        public int GetRangeMask(int lsb, int hsb)
        {
            var b1 = (~0) << lsb;
            var b2 = (~0) << hsb;
            var b3 = b1 ^ b2; //异或
            return b3;
        }
        
        /*
         * E 联想度/组相连度 每组的行数
         * S 组的个数
         * B 块数
         * BS 块大小
         */
        public class CacheGroup
        {
            private readonly List<CaCheLine> _caCheLines;
            
            Dictionary<int, CaCheLine> _linesMap = new Dictionary<int, CaCheLine>();
            public int Capacity;

            public CaCheLine SearchLineWithTag(int t)
            {
                if (_linesMap.ContainsKey(t))
                    return _linesMap[t];
                return null;
            }
            public CaCheLine this[int l] => _caCheLines[l];
            //相连度
            public CacheGroup(int e, int lineSize)
            {
                _caCheLines = new List<CaCheLine>(e);
                Capacity = e;
                for (var i = 0; i < e; i++)
                {
                    _caCheLines[i] = new CaCheLine(lineSize);
                }
            }

            public void Write(int tag, CaCheLine line)
            {
                
            }
        }
        public class CaCheLine
        {
            private byte[] _bytes;
            public byte this[int offset] => _bytes[offset];
            public CaCheLine(int size)
            {
                _bytes = new byte[size + 1];
            }
        }
        
    }
}