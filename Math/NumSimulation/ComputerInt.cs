
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using JJ.Framework.Collections;
using CIExam.FunctionExtension;
using static CIExam.Math.NumSimulation.ComputerInt;

namespace CIExam.Math.NumSimulation
{
    public class ComputerInt : IComparable<ComputerInt>, IEquatable<ComputerInt>, ICloneable
    {
        public readonly int MaxLen;
        
        public BigInteger RepresentNumber {
            get
            {
                var s = BigInteger.Zero;
                var b = BigInteger.One;
                for (var i = Bytes.Length - 1; i >= 1; i--)
                {
                    if (Bytes[i] == 1)
                    {
                        s += b;
                    }
                    b *= 2;
                }

                if (IsUnSign)
                {
                    s += Bytes[0] * b;
                }
                else
                {
                    if (Bytes[0] == 1)
                    {
                        s -= b;
                    }
                }
                return s;
            }
        }
        public bool IsBigEnd = true;
        public bool IsUnSign = false;
        public readonly byte[] Bytes;

        public ComputerInt Abs()
        {
            if(IsUnSign || Bytes[0] != 1)
                return Clone() as ComputerInt;
            
            return Bytes[0] == 1 ? InverseNumber() : null;
        }
        public ComputerInt(int maxLen, long num = 0, bool isUnSign = false)
        {
            MaxLen = maxLen;
            Bytes = new byte[maxLen];
            IsUnSign = isUnSign;
            var bs = ToAbsBinary(num);
            var minus = num < 0;
            num = System.Math.Abs(num);
            //x为bigint偏移
            var (x, y) = bs.Length <= maxLen ? (0, maxLen - bs.Length) : (bs.Length - maxLen, 0);
            for (var i = y; i < maxLen; i++)
            {
                
                Bytes[i] = (byte)(bs[i + x - y] - '0');
            }

            if (minus)
            {
                var inv = InverseNumber();
                for (var i = 0; i < maxLen; i++)
                {
                    Bytes[i] = inv[i];
                }
            }
            
        }

        public static ComputerInt FromBigNum(int maxLen, BigInteger num)
        {
            var ans = new ComputerInt(maxLen);
            var bs = ToAbsBinary(num);
            //x为bigint偏移
            var (x, y) = bs.Length <= maxLen ? (0, maxLen - bs.Length) : (bs.Length - maxLen, 0);
            for (var i = y; i < maxLen; i++)
            {
                ans.Bytes[i] = (byte)(bs[i + x - y] - '0');
            }
            return ans;
        }
        public ComputerInt(int maxLen, string num)
        {
            MaxLen = maxLen;
            Bytes = new byte[maxLen];
            var bs = BigInteger.Parse(num).ToByteArray();
            var offset = bs.Length <= maxLen ? 0 : bs.Length - maxLen;
            for (var i = 0; i < maxLen; i++)
            {
                Bytes[i] = bs[offset + i];
            }
        }

        public override string ToString()
        {
            return Bytes.Aggregate("", (a, b) => a + b);
        }

        public static string ToAbsBinary(BigInteger integer)
        {
            var output = new StringBuilder();
            var stack = new Stack<byte>();
            var n = BigInteger.Abs(integer);
            while (n != 0)
            {
                stack.Add((byte)(n % 2));
                n /= 2;
            }

            while (stack.Any())
            {
                output.Append(stack.Pop());
            }

            return output.ToString() == "" ? "0" : output.ToString();
        }
        public object Clone()
        {
            var clone = new ComputerInt(MaxLen) {IsBigEnd = IsBigEnd, IsUnSign = IsUnSign};
            Bytes.ElementInvoke((e, i) => clone[i] = e);
            return clone;
        }

        public int CompareTo(ComputerInt? other)
        {
            if (other != null) return RepresentNumber.CompareTo(other.RepresentNumber);
            return -1;
        }

        public bool Equals(ComputerInt? other)
        {
            if (other == null)
                return false;
            return MaxLen == other.MaxLen && Bytes.SequenceEqual(other.Bytes);
        }

        public override int GetHashCode()
        {
            return MaxLen.GetHashCode() + Bytes.Select(e => e.GetHashCode()).Sum();
        }

        public byte this[int index]
        {
            get => Bytes[index];
            set => Bytes[index] = value;
        }

        public ComputerInt ShiftLeft(int step)
        {
            var clone = (ComputerInt) Clone();
            var times = MaxLen - step;
            for (var i = 0; i < times; i++)
            {
                clone.Bytes[i] = Bytes[i + step];
            }

            var fillTimes = times <= 0 ? MaxLen : step;
            var fillStart = times < 0 ? 0 : times;
            for(var i = fillStart; i < fillStart + fillTimes; i++)
            {
                clone.Bytes[i] = 0;
            }

            return clone;
        }
        public ComputerInt ShiftRightLogically(int step)
        {
            var clone = (ComputerInt) Clone();
            var times = MaxLen - step;
            for (var i = step; i < MaxLen; i++)
            {
                clone.Bytes[i] = Bytes[i - step];
            }

            var fillTimes = step > MaxLen ? MaxLen : step;
            for(var i = 0; i < fillTimes; i++)
            {
                clone.Bytes[i] = 0;
            }

            return clone;
        }

        public ComputerInt ShiftRightAlgorithm(int step)
        {
            var clone = (ComputerInt) Clone();
            var times = MaxLen - step;
            for (var i = step; i < MaxLen; i++)
            {
                clone.Bytes[i] = Bytes[i - step];
            }

            var fillTimes = step > MaxLen ? MaxLen : step;
            for(var i = 0; i < fillTimes; i++)
            {
                clone.Bytes[i] = IsUnSign ? (byte)0 : Bytes[0];
            }

            return clone;
        }
        public static ComputerInt operator <<(ComputerInt int1, int i)
        {
            return int1.ShiftLeft(i);
        }
        public static ComputerInt operator >>(ComputerInt int1, int i)
        {
            return int1.ShiftRightAlgorithm(i);
        }
        public static ComputerInt operator +(ComputerInt int1, ComputerInt i)
        {
            return int1.Add(i);
        }
        public static ComputerInt operator +(ComputerInt int1, BigInteger i)
        {
            return int1.Add(FromBigNum(int1.MaxLen, i));
        }
        public static ComputerInt operator +(ComputerInt int1, dynamic i)
        {
            return int1.Add(new ComputerInt(int1.MaxLen, i));
        }
        public static ComputerInt operator -(ComputerInt int1, ComputerInt i)
        {
            return int1.Sub(i);
        }
        public static ComputerInt operator |(ComputerInt op1, ComputerInt op2)
        {
            var (r1, r2) = op1.MaxLen >= op2.MaxLen ? ((ComputerInt) op1.Clone(), op2) : ((ComputerInt) op2.Clone(), op1);
            for (var i = 1; i <= r2.MaxLen; i++)
            {
                r1.Bytes[^i] |= r2.Bytes[^i];
            }
            return r1;
        }

        public bool IsOverflowAddWith(ComputerInt other, bool hasCin = false)
        {
            var len = System.Math.Max(MaxLen, other.MaxLen);
            var op1 = MaxLen < other.MaxLen ? FromArray(len,Bytes) : this;
            var op2 = other.MaxLen < MaxLen ? FromArray(len, other.Bytes) : other;
            var preCin = 0;
            var cIn = hasCin ? 1: 0;
            for (var i = len - 1; i >= 0; i--)
            {
                var o = (op1[i] & op2[i]) | (op1[i] & cIn) | (op2[i] & cIn);
                preCin = cIn;
                cIn = o;
            }

            return preCin != cIn;
        }
        public static ComputerInt operator &(ComputerInt op1, ComputerInt op2)
        {
            var (r1, r2) = op1.MaxLen >= op2.MaxLen ? ((ComputerInt) op1.Clone(), op2) : ((ComputerInt) op2.Clone(), op1);
            for (var i = 1; i <= r2.MaxLen; i++)
            {
                r1.Bytes[^i] &= r2.Bytes[^i];
            }
            return r1;
        }
        public static ComputerInt operator ~(ComputerInt op1)
        {
            var r = op1.Clone() as ComputerInt;
            for (var i = 0; i < r.MaxLen; i++)
            {
                r.Bytes[i] = (byte) (1 - r.Bytes[i]);
            }
            return r;
        }
        public ComputerInt InverseNumber(int plus = 1)
        {
            var newNum = new ComputerInt(MaxLen);
            for (var i = 0; i < MaxLen; i++)
            {
                newNum.Bytes[i] = Bytes[i] == 0 ? (byte)1 : (byte)0;
            }

            return newNum + new ComputerInt(MaxLen, plus);
        }
        public ComputerInt Add(ComputerInt other)
        {
            if (MaxLen != other.MaxLen)
                throw new Exception();
            if (RepresentNumber == 0)
                return other;
            if (other.RepresentNumber == 0)
                return this;
            var s = FromArray(MaxLen, Bytes.Zip(other.Bytes, (b, b1) => (byte) (b ^ b1)).ToArray());
            var c = FromArray(MaxLen, Bytes.Zip(other.Bytes, (b, b1) => (byte) (b & b1)).ToArray());
            c <<= 1;
            return s.Add(c);
        }
        public ComputerInt Sub(ComputerInt other)
        {
            return Add(other.InverseNumber());
        }
        public ComputerInt　Mul(ComputerInt other)
        {
            var newLen = MaxLen * 2;
            var ans = FromArray(newLen, Bytes);
            for (var i = 0; i < MaxLen; i++)
            {
                if (ans[newLen - 1] == 1)
                {
                    var op1 = FromArray(MaxLen, ans.Bytes.Take(MaxLen).ToArray()) + other;
                    for (var j = 0; j < MaxLen; j++)
                    {
                        ans[j] = op1.Bytes[j];
                    }
                }

                ans >>= 1;
            }
            return ans;
        }

        public (ComputerInt syou, ComputerInt amari) Div(ComputerInt divisor)
        {
            var newLen = MaxLen * 2;
            var ans = FromArray(newLen, Bytes) << 1;
            $"{RepresentNumber}/{divisor.RepresentNumber}".PrintToConsole();
            
            for (var i = 0; i < MaxLen; i++)
            {
                //符号拓展
                ans.Bytes[i] = Bytes[0];
            }
            $"code extended = {ans.Bytes.ToEnumerationString()}".PrintToConsole();
            
            //记录，方便使用
            var invDivisor = divisor.InverseNumber();
            
            for (var i = 0; i < MaxLen; i++)
            {
                $"now ans = {ans.Bytes.ToEnumerationString()}".PrintToConsole();
                var op1 = FromArray(MaxLen, ans.Bytes.Take(MaxLen).ToArray());
                (op1[0], divisor.Bytes[0]).PrintToConsole();
                var op2 = (op1[0] ^ divisor[0]) == 0 ? invDivisor : divisor;
                $"cur step = {op1.Bytes.ToEnumerationString()} + {op2.Bytes.ToEnumerationString()}".PrintToConsole();
                
                //Add
                var s = op1 + op2;
                s.Bytes.ToEnumerationString().PrintToConsole();
                ans[..(MaxLen - 1)] = s.Bytes;
                ans.Bytes.ToEnumerationString().PrintToConsole();
                //上商
                $"上商 {(byte) (1 - (divisor[0] ^ ans[0]))}".PrintToConsole();
                ans.Bytes[^1] = (byte) (1 - (divisor[0] ^ ans[0]));
                if (i == MaxLen - 1)
                {
                    $"now ans = {ans.Bytes.ToEnumerationString()}\n".PrintToConsole();
                    break;
                }
                ans <<= 1;
                
            }
            var syou = FromArray(MaxLen, ans.Bytes.Skip(MaxLen).Take(MaxLen).ToArray());
            var amari = FromArray(MaxLen, ans.Bytes.Take(MaxLen).ToArray());
            (syou, amari).PrintToConsole();
            if (syou[MaxLen - 1] == 0)
            {
                amari += amari[0] == divisor[0] ? invDivisor : divisor;
            }
            return (syou, amari);
        }

        public static ComputerInt operator *(ComputerInt op1, ComputerInt op2)
        {
            return op1.Mul(op2);
        }
        public static (ComputerInt syou, ComputerInt amari) operator /(ComputerInt op1, ComputerInt op2)
        {
            return op1.Div(op2);
        }
        public byte[] this[Range range]
        {
            get => Bytes[range];
            set
            {
                var from = range.Start.Value;
                var to = range.End.Value;
                for (var i = from; i <= to; i++)
                {
                    Bytes[i] = value[i - from];
                }
            }
        }
        public static ComputerInt FromArray(int maxLen, byte[] bs)
        {
            var ans = new ComputerInt(maxLen);
            
            //x为bigint偏移
            var (x, y) = bs.Length <= maxLen ? (0, maxLen - bs.Length) : (bs.Length - maxLen, 0);
            for (var i = y; i < maxLen; i++)
            {
                ans.Bytes[i] = bs[i + x - y];
            }

            return ans;
        }

       
    }

    public static class ComputerNumberTest
    {
        public static void TestInteger()
        {
            
            var cInt = new ComputerInt(32, 50);
            cInt.PrintToConsole();
            (cInt >> 2).PrintToConsole();
            cInt.RepresentNumber.PrintToConsole();
            
            cInt.Add(cInt).RepresentNumber.PrintToConsole();
            cInt.InverseNumber().PrintToConsole();
            cInt.InverseNumber().RepresentNumber.PrintToConsole();
            cInt.Mul(cInt).RepresentNumber.PrintToConsole();
            cInt.Mul(cInt.InverseNumber()).RepresentNumber.PrintToConsole();
            
            //div
            var c1 = FromArray(4, new byte[] {0,1,0,1}).InverseNumber();
            var c2 = FromArray(4, new byte[] {0,0,1,1}).InverseNumber();
            c1.RepresentNumber.PrintToConsole();
            c2.RepresentNumber.PrintToConsole();
            (c1.Mul(c2)).RepresentNumber.PrintToConsole();
            (c1.Div(c2)).PrintToConsole();
        }
        public static void TestDecimal()
        {
            var c1 =
                ("01100111010100110010101010111011001101101111110011110100000000111001110001110111000111000011111111100001011"
                ).Select(e => (byte) (e - '0')).ToArray();
            ComputerDecimal.GetTail10Base(c1).PrintToConsole();
            var tmp = ComputerDecimal.GetBinaryRepresent(c1, c1, -500);
            ComputerDecimal.Get10BaseRepresent(0, tmp.newE.ToArray(), tmp.newM.ToArray()).PrintToConsole();

            var cd = new ComputerDecimal(8, 23);
            cd.M.Bytes[^1] = 1;
            cd.RepresentString.PrintToConsole();

        }
    }
}