
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CIExam.os.Cache;
using JJ.Framework.Collections;
using JJ.Framework.Text;
using CIExam.FunctionExtension;

namespace CIExam.Math.NumSimulation
{
    //浮点数能否被精确表示？
    
    public class ComputerDecimal : ICloneable
    {
        public byte S; //符号位
        public ComputerInt E; //指数
        public ComputerInt M; //尾数
        public readonly ComputerInt Bias; //IEEE754偏置

        public string RepresentString
        {
            get
            {
                var e = E.Bytes;
                var m = M.Bytes;
                if (E.RepresentNumber != 0)
                {
                    var tmp = GetBinaryRepresent(new byte[] {1}, m, E.RepresentNumber - Bias.RepresentNumber);
                    
                    return Get10BaseRepresent(S, tmp.newE.ToArray(), tmp.newM.ToArray());
                }
                else
                {
                    if (M.RepresentNumber == 0)
                    {
                        return "0";
                    }
                    var tmp = GetBinaryRepresent(new byte[] {0}, m, - Bias.RepresentNumber + 1);
                    return Get10BaseRepresent(S, tmp.newE.ToArray(), tmp.newM.ToArray());
                }
                
            }
        }
        public ComputerDecimal(int eLen, int mLen)
        {
            E = new ComputerInt(eLen);
            M = new ComputerInt(mLen);
            var b = BigInteger.Pow(2, eLen - 1) - 1;
            Bias = ComputerInt.FromBigNum(eLen, b);
        }

        public ComputerDecimal(int eLen, int mLen, double data)
        {

            M = new ComputerInt(mLen) {IsUnSign = true};
            var b = BigInteger.Pow(2, eLen - 1) - 1;
            Bias = ComputerInt.FromBigNum(eLen, b);
            Bias.IsUnSign = true;
            var s = data + "";
            var (zPart, fracPart) = (s.Split(".")[0], s.Split(".")[1]);
            
            var zPartBinary = Convert.ToString(long.Parse(zPart), 2);
            var leftShiftLen = zPartBinary.Length - 1;
            var e = leftShiftLen + b;
            
            //var t = ComputerInt.FromBigNum(eLen, e) + Bias;
            //e.PrintToConsole();
            E = ComputerInt.FromBigNum(eLen, e);
            E.IsUnSign = true;
            var m = double.Parse("0." + fracPart);
            var rem = mLen - leftShiftLen;
            var mByte = zPartBinary.Skip(1).Select(c => (byte) (c - '0')).ToList();
           
            for (var i = 0; i < rem; i++)
            {
                m *= 2;
                mByte.Add((byte) (m >= 1 ? 1 : 0));
                if (m >= 1)
                    m -= 1;
            }
            
            for (var i = 0; i < mLen; i++)
            {
                M[i] = mByte[i];
            }
            //E.RepresentNumber.PrintToConsole();
        }

        public override string ToString()
        {
            return E.RepresentNumber != 0 ? 
                $"(-1)^({S}) x (1.{M}) x 2^({E - Bias})" :
                $"(-1)^({S}) x (0.{M}) x 2^({E - Bias + new ComputerInt(E.MaxLen, 1)})";
        }

        public ComputerDecimal InverseNumber()
        {
            var clone = Clone() as ComputerDecimal;
            clone.S = (byte) (1 - clone.S);
            return clone;
        }
        
        //使用cache 利用时间空间局部性 加速2乘方计算
        private static readonly AbstractCache<int, BigInteger> PowResultCache =
            CacheBuilder.BuildLruCommonCache<int, BigInteger>(50);
        //return 2^n
        public static BigInteger Pow2N(int n)
        {
            //尝试直接从缓存中获取数据，如果不存在数据的话，调用数据获取委托
            return PowResultCache.Access(n,null, (_,_) => BigInteger.Pow(2, n));
        }

        public ComputerDecimal Mul(ComputerDecimal op1, ComputerDecimal op2)
        {
            var ab = op1.M * op2.M;
            var m = ab + op1.M + op2.M;
            var e = op1.E + op2.E;
            var s = op1.S ^ op2.S;
            var res = new ComputerDecimal(op1.E.MaxLen, op1.M.MaxLen * 2)
            {
                M = m,
                E = e,
                S = (byte) s
            };
            return res;
        }
        
        
        public static ComputerDecimal operator +(ComputerDecimal op1, ComputerDecimal op2)
        {
            var i1 = op1.Clone() as ComputerDecimal;
            var i2 = op2.Clone() as ComputerDecimal;
            var e1 = i1.E.RepresentNumber;
            var e2 = i2.E.RepresentNumber;
            var diffE = e1 - e2;
            //对阶码
            if (diffE > 0)
            {
                i2.E += ComputerInt.FromBigNum(i2.E.Bytes.Length, diffE);
                i2.M >>= (int)diffE;
            }else if (diffE < 0)
            {
                diffE = BigInteger.Abs(diffE);
                i1.E += ComputerInt.FromBigNum(i1.E.Bytes.Length, diffE);
                i1.M >>= (int)diffE;
            }

            var isOverflow = i1.M.IsOverflowAddWith(i2.M);
            var m = i1.M + i2.M;
            var e = e1 > e2 ? op1.E.Clone() as ComputerInt : op2.E.Clone() as ComputerInt;
            var s = 0;
            if (isOverflow)
            {
                if(op1.S == op2.S)
                    e += ComputerInt.FromBigNum(e.Bytes.Length, 1);
                else
                {
                    var abs1 = op1.M.Abs().RepresentNumber;
                    var abs2 = op2.M.Abs().RepresentNumber;
                    if (abs1 > abs2)
                        s = op1.S;
                    else if (abs2 > abs1)
                        s = op2.S;
                }
            }
            var res = new ComputerDecimal(op1.E.Bytes.Length, op1.M.Bytes.Length)
            {
                M = m,
                E = e,
                S = (byte)s
            };
            return res;
        }
        
        //输入 指数字节二进制，尾数字节二进制，2的指数
        public static (List<byte> newE, List<byte> newM) GetBinaryRepresent(byte[] integerPart, byte[] mPart, BigInteger pow2)
        {
            var newE = new List<byte>();
            var newM = new List<byte>();
            if (pow2 > 0)
            {
                newE.AddRange(integerPart);
                if (pow2 < mPart.Length)
                {
                    newE.AddRange(mPart.Take((int)pow2));
                    newM.AddRange(mPart.Skip((int)pow2));
                }
                else
                {
                    newE.AddRange(mPart);
                    if (pow2 > mPart.Length)
                    {
                        newE.AddRange(Enumerable.Repeat((byte)0, (int)pow2 - mPart.Length));
                    }
                }
            }
            else if(pow2 < 0)
            {
                var abs = -pow2;
                if (abs >= integerPart.Length)
                {
                    if(abs > integerPart.Length)
                        newM.AddRange(Enumerable.Repeat<byte>(0, (int) (abs - integerPart.Length)));
                    newM.AddRange(integerPart);
                    newM.AddRange(mPart);
                }
                else
                {
                    newE.AddRange(integerPart.Take((int) (integerPart.Length - abs)));
                    newM.AddRange(integerPart.Skip((int) (integerPart.Length - abs)));
                    newM.AddRange(mPart);
                }
            }
            else
            {
                newE.AddRange(integerPart);
                newM.AddRange(mPart);
            }

            return (newE, newM);
        }

        public static string Get10BaseRepresent(byte s, byte[] integerPart, byte[] tailPart)
        {
            var intPart = ComputerInt.FromArray(integerPart.Length + 1, integerPart).RepresentNumber;
            var tail = GetTail10Base(tailPart);
            return (s == 1 ? "-" : "") + intPart + "." + tail;
        }
        public static string GetTail10Base(byte[] tail, int mLen = 308)
        {
            if (!tail.Any())
                return "0";
            //tail.PrintMultiDimensionCollectionToConsole();
            var up = BigInteger.Zero;
            var bottom = BigInteger.Zero;
            var oneList = tail.ConditionSelect((e, _) => e == 1, (_, i) => i + 1);
            //oneList.PrintMultiDimensionCollectionToConsole();
            foreach (var index in oneList)
            {
                if (up == 0)
                {
                    up = 1;
                    bottom = BigInteger.Pow(2, index);
                }
                else
                {
                    //模拟浮点相加
                    var c = BigInteger.Pow(2, index);
                    up = up * c + bottom;
                    bottom *= c;
                }
            }
            //(up.ToString(), bottom.ToString()).PrintToConsole();
           
            var sb = new StringBuilder();
            var amari = up * 10;
            //除法
            for (var i = 0; i < mLen && amari != 0; i++)
            {
                //$"{amari} / {bottom}".PrintToConsole();
                var s = amari / bottom;
                amari %= bottom;
                amari *= 10;
                sb.Append(s.ToString());
            }
            return sb.ToString();
        }

        public string GetRepresentString()
        {
            var s = S == 0 ? "" : "-";
            if (M.RepresentNumber != 0)
            {
                //规格化数
                //表示长尾数可麻烦了，需要转换为指数运算。
            }
            return s;
        }


        public object Clone()
        {
            var res = new ComputerDecimal(E.Bytes.Length, M.Bytes.Length)
            {
                E = ComputerInt.FromArray(E.Bytes.Length, E.Bytes),
                M = ComputerInt.FromArray(M.Bytes.Length, M.Bytes),
                S = S
            };
            return res;
        }
    }
}