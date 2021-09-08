using System.Collections.Generic;
using System.Linq;
using BigMath;
using CIExam.Math.NumSimulation;
using BigInteger = System.Numerics.BigInteger;

namespace CIExam.Math
{
    public static class BitAlgorithms
    {
        public static int GetLowestOne(ulong a)
        {
            for (var i = 0; i < 64; i++)
            {
                if ((a & 1) == 1)
                    return i;
                a >>= 1;
            }

            return 64;
        }
        public static int GetHighestOne(ulong a)
        {
            var cur = -1;
            for (var i = 0; i < 64; i++)
            {
                if ((a & 1) == 1) cur = i;
                a >>= 1;
            }
            return cur;
        }
        public class BitTreeArray //可以维护动态前缀和
        {
            int[] Array;
            public readonly int Size;
            public BitTreeArray(int size)
            {
                Array = new int[size + 1];
                Size = size;
            }
            public int LowBit(int x)
            {
                return x & -x;
            }

            
            //在i位置加上一个树
            public void Update(int i, int delta)
            {
                i += 1;
                while (i < Array.Length)
                {
                    Array[i] += delta;
                    i += LowBit(i);
                }
            }
            
            //更该
            public void SetVal(int i, int val)
            {
                i += 1;
                Update(i, val - Array[i]);
            }

            public int Query(int i)
            {
                i += 1;
                var res = 0;
                while (i > 0)
                {
                    res += Array[i];
                    i -= LowBit(i);
                }

                return res;
            }

            public int Query(int i, int j)
            {
                if (i > j)
                    return 0;
                return Query(j) - Query(i);
            }
            
            /*
             *example
              def countSmaller(self, nums: List[int]) -> List[int]:
        hashTable = {v: i for i, v in enumerate(sorted(set(nums)))}
        # print(hashTable)
        tree = BinaryIndexedTree([0] * len(hashTable))
        res = []
        for i in range(len(nums) - 1, -1, -1):
            res.append(tree.prefix(hashTable[nums[i]] - 1))
            tree.updata(hashTable[nums[i]] , 1)
        
        return res[::-1]
             */
        }
        public static int SetOne(int data, int pos)
        {
            return data | (1 << pos);
        }
        public static int SetZero(int data, int pos)
        {
            return data & ~(1 << pos);
        }

        public static int Reverse(int data, int pos)
        {
            return data ^ (1 << pos);
        }
        public static BigInteger NextGrayCode(BigInteger gray)
        {
            var oneCount = CountOne(gray);
            var n = gray;
            if (oneCount.IsEven)
            {
                //偶数个1，直接取反末尾
                return n ^ 1;
            }

            var t = n & -n;
            return n ^ (t << 1);
        }

        public static BigInteger ToGrayCode(BigInteger n)
        {
            return n ^ (n >> 1);
        }

        public static BigInteger GrayToNum(BigInteger n)
        {
            BigInteger mask;
            for (mask = n >> 1; mask != 0; mask >>= 1)
            {
                n ^= mask;
            }
            return n;
        }
        //先解码再编码的方式
        public static BigInteger GrayNextGray(BigInteger num)
        {
            return ToGrayCode((GrayToNum(num) + 1) );
        }

        public static BigInteger PrevGray(BigInteger num)
        {
            return ToGrayCode(GrayToNum(num) - 1 );
        }
        
        public static BigInteger CountOne(BigInteger n)
        {
            var t = n;
            var cnt = BigInteger.Zero;
            while (t != 0)
            {
                cnt++;
                t &= (t - 1);
            }

            return cnt;
        }
        
    }
}