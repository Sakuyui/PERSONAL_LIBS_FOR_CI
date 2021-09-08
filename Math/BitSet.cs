using System;
using System.Linq;
using System.Text;

namespace CIExam.Math
{
    public class BitSet : IEquatable<BitSet>, ICloneable
    {
        public readonly int Size;
        public StringBuilder RepresentationString;
        public BitSet(int len)
        {
            Size = len;
            RepresentationString = new StringBuilder(Enumerable.Repeat('0', len).Aggregate("", (a, b) => a + b));
        }

        public void SetBit(int pos)
        {
            RepresentationString[pos] = '1';
        }

        public void ClearBit(int pos)
        {
            RepresentationString[pos] = '0';
        }

        public void Clear()
        {
            RepresentationString = new StringBuilder(Enumerable.Repeat('0', Size).Aggregate("", (a, b) => a + b));
        }

        public bool IsSetBit(int pos)
        {
            return pos < Size && RepresentationString[pos] == '1';
        }

        public override string ToString()
        {
            return RepresentationString + "";
        }

        public object Clone()
        {
            var newBitset = new BitSet(Size);
            for (var i = 0; i < Size; i++)
            {
                newBitset.RepresentationString[i] = RepresentationString[i];
            }

            return newBitset;
        }

        public static BitSet operator >>(BitSet bsBitSet, int size)
        {
            var bs = bsBitSet.Clone() as BitSet;
            for (var i = 0; i < size; i++)
            {
                bs.RepresentationString.Remove(bs.Size - 1, 1);
                bs.RepresentationString.Insert(0, '0');
            }

            return bs;
        }
        public static BitSet operator <<(BitSet bsBitSet, int size)
        {
            var bs = bsBitSet.Clone() as BitSet;
            for (var i = 0; i < size; i++)
            {
                bs.RepresentationString.Remove(0, 1);
                bs.RepresentationString.Insert(bs.Size - 1, '0');
            }

            return bs;
        }

        public static BitSet operator &(BitSet bitSet, BitSet otherBitSet)
        {
            var more = bitSet.Size >= otherBitSet.Size ? bitSet : otherBitSet;
            var less = bitSet.Size < otherBitSet.Size ? bitSet : otherBitSet;
            var bs = more.Clone() as BitSet;
            var r1 = less.Size - 1;
            var r2 = more.Size - 1;
            while (r1 >= 0)
            {
                bs[r2] = bs[r2] == '1' && less[r1] == '1' ? '1' : '0';
                r1--;
                r2--;
            }

            while (r2 >= 0)
            {
                bs[r2--] = '0';
            }
            return bs;
        }
        public char this[int pos]
        {
            get => RepresentationString[pos];
            set => RepresentationString[pos] = value;
        }
        public static BitSet operator |(BitSet bitSet, BitSet otherBitSet)
        {
            var more = bitSet.Size >= otherBitSet.Size ? bitSet : otherBitSet;
            var less = bitSet.Size < otherBitSet.Size ? bitSet : otherBitSet;
            var bs = more.Clone() as BitSet;
            var r1 = less.Size - 1;
            var r2 = more.Size - 1;
            while (r1 >= 0)
            {
                bs[r2] = bs[r2] == '1' || less[r1] == '1' ? '1' : '0';
                r1--;
                r2--;
            }

            
            return bs;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is BitSet bitSet
                   && RepresentationString.ToString() == bitSet.RepresentationString.ToString();
        }

        public bool Equals(BitSet other)
        {
            return other != null && RepresentationString.ToString() == other.RepresentationString.ToString();
        }

        public override int GetHashCode()
        {
            return RepresentationString.ToString().GetHashCode();
        }
    }
}