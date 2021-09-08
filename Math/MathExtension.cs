using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using JJ.Framework.Collections;
using CIExam.FunctionExtension;


namespace CIExam.Math
{
    public class AdvanceRandom
    {
        public BigInteger Seed;
        public BigInteger UX0 = BigInteger.Zero;
        public BigInteger ULambda = 17;
        public readonly BigInteger C = 139;
        public readonly BigInteger M = 256;

        public static double RandXPower2(int x)
        {
            //在0,x)上，概率是x^2
            var r = new Random();
            return (r.NextDouble() * x) * (r.NextDouble() * x);
        }
        public static IEnumerable<T> RandomNoRepeatSelection<T>(IEnumerable<T> enumerable, int k)
        {
            var input = enumerable.ToArray();
            return GetRandomIndexes(input.Length).Take(k).Select(e => input[e]);
        }
        public static IEnumerable<int> GetRandomIndexes(int length)
        {
            var arr = Enumerable.Range(0, length).ToArray();
            var r = new Random();

            void Swap(int i, int j)
            {
                var t = arr[i];
                arr[i] = arr[j];
                arr[j] = t;
            }
            
            for (var i = 0; i < length; i++)
            {
                var rand = r.Next(0, length - 1 - i);
                Swap(rand, length - i - 1);
                yield return arr[i];
            }
            
        }
        public AdvanceRandom(BigInteger seed)
        {
            Seed = seed;
            UX0 = (ULambda * seed + C) % M;
        }
        public double NextRandomU()
        {
            UX0 = (ULambda * UX0 + C) % M;
            return (double) (UX0) / (double) M;
        }
        //正态分布
        public double NextRandomN()
        {
            var u = System.Math.Sqrt(-2 * System.Math.Log(NextRandomU()));
            var cos = System.Math.Cos(2 * System.Math.PI * NextRandomU());
            return u * cos;
        }
        public double NextRandomN(BigInteger u, BigInteger sigma, int n)
        {
            var s = 0.0;
            for (var i = 0; i < n; i++)
            {
                s += NextRandomU();
            }

            return (double)u + (double)sigma * ((s - n / 2.0) / System.Math.Sqrt(n / 12.0));
        }
    }
    public static class MathExtension
    {

       

        public static string ConvertBase(this BigInteger num, int sourceBase, int targetBase)
        {
            var dict = Enumerable.Range(0, 9).ToDictionary(k => '0' + k, v => v);
            FunctionExt.ElementInvoke(Enumerable.Range(0,26), e =>
            {
                dict.Add('a' + e, e);
                dict.Add('A' + e, e);
            });
            
            var sum = new BigInteger();
            var source = num.ToByteArray();
            var base1 = BigInteger.One;
            for (var i = source.Length - 1; i >= 0; i--)
            {
                var realBit = dict[source[i]];
                sum += realBit * base1;
                base1 *= sourceBase;
            }


            var decodeDict = Enumerable.Range(0, 26).ToDictionary(k => k, v =>
            {
                if (v is >= 0 and <= 9)
                    return '0' + v;
                return 'A' + v - 10;
            });

            var stack = new Stack<int>();
            while (sum != 0)
            {
                stack.Add(decodeDict[(int)(sum % targetBase)]);
                sum /= targetBase;
            }

            var sb = new StringBuilder();
            while (stack.Any())
                sb.Append(stack.Pop());
            return sb.ToString() == "" ? "0" : sb.ToString();
        }
        public static BigInteger ModInv(BigInteger n, BigInteger p)
        {
            //a * a^{p - 2} == 1 (mod p)
            return BigModPow(n, p - 2, p);
        }
        
        public static BigInteger ModInvWithExtGcd(BigInteger n, BigInteger p)
        {
            ExtGcd(n, p, out _, out var x, out _);
            return (x % p + p) % p;
        }
        public static void ExtGcd(BigInteger a, BigInteger b, out BigInteger g, out BigInteger x, out BigInteger y)
        {
            if (b == 0)
            {
                g = a;
                x = 1;
                y = 0;
            }
            else
            {
                ExtGcd(b, a % b, out g, out y, out x);
                y -= x * (a / b);
            }
        }
       
        public static BigInteger Sqrt(this BigInteger n)
        {
            if (n == 0) return 0;
            if (n <= 0) throw new ArithmeticException("NaN");
            var bitLength = Convert.ToInt32(System.Math.Ceiling(BigInteger.Log(n, 2)));
            var root = BigInteger.One << (bitLength / 2);

            while (!IsSqrt(n, root))
            {
                root += n / root;
                root /= 2;
            }

            return root;

        }
        public static BigInteger BigPow(BigInteger x, BigInteger n)
        {
            if (n == 0)
                return 1;
            if (n == 1)
                return x;

            var t = BigPow(x, n / 2);
            return n.IsEven ? t * t : x * t * t;
        }

        public static T Max<T>(params T[] objs) where T : IComparable
        {
            if (objs.Length == 0)
                throw new Exception();
            var max = objs[0];
            for (var i = 1; i < objs.Length; i++)
            {
                if (objs[i].CompareTo(max) > 0)
                    max = objs[i];
            }

            return max;
        }
        
        
       
        public static bool MillerRabin(BigInteger n)
        {
            if (n.IsEven || n < 3)
                return false;
            
            //对a^u a^u2^2 a^u2^2 ...检验
            //u = (p - 1) / 2^t
            //初始 u = p - 1, t = 0;  ===> (p - 1)/1
            var u = n - 1;
            var t = 0;
            
            //确定序列最大长度。
            while (u.IsEven)
            {
                u /= 2; t++;
            }
            //选择的底数
            var ud = new []{2,325,9375,28178,450775,9780504,1795265022}; //也可以随机底数。不过在longlong范围内，使用这几个底数能够保证正确
            
            foreach(var a in ud) //底数遍历在外层
            {
                //a^u
                //初始化a ^ u
                var v = BigModPow(a, u, n);
                if (v == 1 || v == n- 1 || v == 0)
                    continue; //验证成功，下一个底数
                
                //序列遍历在里程循环
                for (var j = 1; j <= t; j++) //对序列每个指数判断。注意，需要从1开始
                {
                    v = BigModMultiply(v, v, n); //指数倍增
                    if (v == n - 1 && j != t)
                    {
                        //在非最后一次出现 p - 1，只是跳出循环，直接下一个底数。说明成功
                        v= 1;
                        break;
                    }
                    
                    //能执行到这里只可能是没p - 1。
                    if (v == 1) 
                        return false; //这里代表前面没有出现n - 1这个解，二次检验失败
                }
                
                //必须全1，或者在非最后一次出现p - 1
                if(v != 1)
                    return false; //Fermat检验
            }
            return true;
        }

        private static readonly Random Random = new Random();
        public static BigInteger RandomIntegerBelow(BigInteger n) {
            var bytes = n.ToByteArray ();
            
            BigInteger r;

            do {
                Random.NextBytes (bytes);
                bytes [^1] &= 0x7F; //force sign bit to positive
                r = new BigInteger (bytes);
            } while (r >= n);

            return r;
        }


        public static void Test()
        {
            byte[] ToBytes(BigInteger num, int fromBase = 2)
            {
                if (num == 0)
                    return new byte[] {0};
                var s = new Stack<byte>();
                while (num != 0)
                {
                    var rem = (int)(num % fromBase);
                    num /= fromBase;
                    s.Push((byte) rem);
                }
                return s.ToArray();
            }
            BytesMultiply(2, 64, new []
            {
                ToBytes((1 << 1) + 1),  ToBytes((1 << 2) + 1),
                ToBytes((1 << 4) + 1), ToBytes((1 << 8) + 1),
                ToBytes((1 << 16) + 1)
            }).PrintEnumerationToConsole();
        }
        public static byte[] BytesMultiply(int fromBase, int maxLen = 64, params byte[][] nums)
        {
            var n = nums.Length;
            if (n == 0)
                return new byte[] {0};
            if (n == 1)
                return nums.First();
            var len = nums.Select(e => e.Length).Max();
            $"len = {len}".PrintToConsole();
            var l = BytesMultiply(fromBase, maxLen, nums.Take(n >> 1).ToArray());
            var r = BytesMultiply(fromBase, maxLen,nums.Skip(n >> 1).ToArray());
            return UnSignBytesMultiplication(l, r, System.Math.Max(l.Length, r.Length), fromBase);
        }
        static BigInteger GetRepresent(byte[] num, int fromBase = 2, bool isUnSign = false)
        {
            var sum = BigInteger.Zero;
            var t = 1;
            for (var i = 0; i < num.Length; i++)
            {
                sum += num[num.Length - i - 1] * t;
                t *= fromBase;
            }

            if (!isUnSign && num[0] != 0)
            {
                sum -= BigInteger.Pow(fromBase, num.Length);
            }

            return sum;
        }
        static byte[] ToBytes(BigInteger num, int fromBase = 2)
        {
            if (num == 0)
                return new byte[] {0};
            var s = new Stack<byte>();
            while (num != 0)
            {
                    
                var rem = (int)(num % fromBase);
                    
                num /= fromBase;
                s.Push((byte) rem);
            }
                
            return s.ToArray();
        }
        public static byte[] UnSignBytesMultiplication(byte[] x, byte[] y, int n = 64, int fromBase = 2, int maxLen = 64)
        {
            if (x.Length != maxLen)
            {
                x = Enumerable.Repeat((byte) 0, maxLen - x.Length).Concat(x).ToArray();
            }
            if (y.Length != maxLen)
            {
                y = Enumerable.Repeat((byte) 0, maxLen - y.Length).Concat(y).ToArray();
            }

        
            
            if (x.All(e => e == 0) || y.All(e => e == 0) || !x.Any() || !y.Any())
                return new byte[]{0};
            var sign = x[0] == 1 && y[0] == 0 || x[0] == 0 && y[0] == 1 ? 1 : -1;
            var xNum = BigInteger.Abs(GetRepresent(x));
            var yNum = BigInteger.Abs(GetRepresent(y));
            
            if (n == 1)
            {
                $"return = {ToBytes(x[^1] * y[^1]).GetMultiDimensionString()}".PrintToConsole();
                return ToBytes(x[^1] * y[^1]);
            } 
            
            var a =  ToBytes(GetRepresent(x) / BigInteger.Pow(fromBase, n >> 1));
            var b =  ToBytes(GetRepresent(x) % BigInteger.Pow(fromBase, n >> 1));
            var c =  ToBytes(GetRepresent(y) / BigInteger.Pow(fromBase, n >> 1));
            var d = ToBytes(GetRepresent(y) %  BigInteger.Pow(fromBase, n >> 1));

            var ac = GetRepresent(UnSignBytesMultiplication(a, c, n >> 1));
            var bd = GetRepresent(UnSignBytesMultiplication(b, d, n >> 1));
            var abcd =
                GetRepresent(UnSignBytesMultiplication(ToBytes(GetRepresent(a) + GetRepresent(b)), 
                        ToBytes(GetRepresent(d) + GetRepresent(c)), n >> 1)) - ac - bd;
            abcd.PrintToConsole();
            var res = ToBytes(ac * BigInteger.Pow(fromBase, n) + abcd * BigInteger.Pow(fromBase, n >> 1) + bd);
            res.PrintMultiDimensionCollectionToConsole();
            return res;
        }

        public static IEnumerable<int> IntervalPrimes(int start, int end)
        {
            var memo = new bool[end + 1].Select(_ => true).ToArray();
            memo[0] = false;
            memo[1] = false;

            for (var i = 2; i * i <= end; i++)
            {
                for (var j =  2 * i; j <= end; j += i)
                {
                    memo[j] = false;
                }
            }

            return memo.Select((e, i) => (e, i)).Where(e => e.e && e.i >= start).Select(e => e.i);
        }
        public static long BitMultiplication(long x, long y, int n = 64, int fromBase = 10)
        {
            int Sign(long num) => num >> 63 == 1 ? 1 : -1;
            var sign = Sign(x) * Sign(y);

            x = System.Math.Abs(x);
            y = System.Math.Abs(y);
            if (x == 0 || y == 0)
                return 0;
            if (n == 1)
                return sign * x * y;
            
            var a = (long) (x / System.Math.Pow(fromBase, n >> 1));
            var b = (long) (x % System.Math.Pow(fromBase, n >> 1));
            var c = (long) (y / System.Math.Pow(fromBase, n >> 1));
            var d = (long) (y % System.Math.Pow(fromBase, n >> 1));

            var ac = BitMultiplication(a, c, n >> 1);
            var bd = BitMultiplication(b, d, n >> 1);
            var abcd = BitMultiplication(a + b, d + c, n >> 1) - ac - bd;

          

            return (long) (sign * (ac * System.Math.Pow(fromBase, n) + abcd * System.Math.Pow(fromBase, n >> 1) + bd));
        }

        public static BigInteger PrimeFactorization(BigInteger n)
        {
            if (n == 4) 
                return 2;
            if (MillerRabin(n)) //特判质数
                return n;
            while (true)
            {
                var c = RandomIntegerBelow(n - 1) - Random.Next(0, 1); // 随机数
                ("use rand " + c).PrintToConsole();
                BigInteger F(BigInteger x) { return (x * x + c) % n; }  //辅助函数 
                
                //init
                var t = F(0);
                var r = F(F(0));
                while (t != r) //在环上移动，直到出现在相对于模运算距离相等的两个数
                {
                    var d = Gcd(BigInteger.Abs(t - r), n); 
                    if (d > 1) //找到第一个约数。 （因为只是分解两数相乘，直接返回）
                        return d;
                    //在floyd环上移动
                    t = F(t);
                    r = F(F(r));
                }
            }
            
        }
        public static BigInteger Gcd(BigInteger a, BigInteger b)
        { 
            //归约节点
            if (a < b)   return Gcd(b,a);
            if (b == 0) return a;

            return a.IsEven switch {
                true when b.IsEven 
                    => 2 * Gcd(a >> 1, b >> 1),
                true => Gcd(a >> 1, b),
                _ => b.IsEven ? Gcd(a, b >> 1) : Gcd((a + b) >> 1, (a - b) >> 1)
            };
        }


        public static BigInteger Lcm(IEnumerable<int> nums)
        {
            
            if (!nums.Any())
                return 1;
            if (nums.Count() == 1)
                return nums.First();
            var n = nums.Count();
            var l = Lcm(nums.Take(n / 2));
            var r = Lcm(nums.Skip(n / 2));
            if (l == 1)
                return r;
            if (r == 1)
                return l;
            return l * r / Gcd(l, r);
        }
        //chuyu ai rem: rem
        public static BigInteger ChineseRemainderTheorem(IEnumerable<(int ai, int rem)> equations)
        {
            var input = equations.ToArray();
            var lcm = Lcm(input.Select(e => e.ai));
            var m = input.Select(e => e.ai).Aggregate(BigInteger.One, (a, b) => a * b);
            var baseDivsM = (from e in input select (Mi: lcm / e.ai, div: e.ai)).ToArray();
            var mInv = from e in baseDivsM select ModInv(e.Mi, e.div);
            
            var bigM = input.Zip(baseDivsM.Zip(mInv, (mi, mir) => mi.Item1 * mir)
                , (b, tm) => b.rem * tm)
                .Aggregate(BigInteger.Zero, (a, b) => a + b);
           
            return bigM % m;
        }
        
        // public static BigInteger ModFact(int n, int p, out int e)
        // {
        //     e = 0;
        //     if (n == 0)
        //         return 1;
        //     var res = ModFact(n / p, p, out e);
        //     e += n / p;
        //     //if (n / p % 2 != 0) return res * (p - Fact(n % p)) % p; //fact是预处理的表
        //     return res * fact[n % p] % p;
        // }
        public static BigInteger ModCombination(int n, int k, int p)
        {
            if (n < 0 || k < 0 || n < k) return 0;
            int e1, e2, e3;

            BigInteger Fact(int n) => n <= 1 ? 1 : ((n % p) * (Fact(n - 1) % p)) % p;
            var a1 = Fact(n);
            var a2 = Fact(k);
            var a3 = Fact(n - k);
            //if (e1 > e2 + e3) return 0;
            return a1 * ModInv(a2 * a3 % p, p) % p;
        }

        //find min x : a^x == b (mod p)
        public static BigInteger DiscreteTimeLogSolve(BigInteger a, BigInteger b, BigInteger p)
        {
            if(p == 1) return b == 0 ? (a != 1) ? 1 : 0 : -1;
            if(b == 1) return a != 0 ? 0 : 1;
            if(a % p == 0) return b == 0 ? 1 : -1;
            
            var m = p.Sqrt() + 1;
            var dict = new Dictionary<BigInteger, BigInteger>();
            var d = BigInteger.One;
            var @base = BigInteger.One;
            //全部塞进hash
             for (var i = 0; i < m; i++)
             {
                 if (dict.ContainsKey(@base))
                     dict[@base] = BigInteger.Min(dict[@base], i);
                 else
                     dict[@base] = i;
                 @base = (@base * a) % p;
             }
            
             for (var i = 0; i < m; i++)
             {
                 var x = ModInvWithExtGcd(d, p);
                 x = (x * b % p + p) % p;
                 if (dict.ContainsKey(x))
                 {
                     $"test {a}^{dict[x] + i * m} % {p} = {BigInteger.ModPow(a,dict[x] + i * m, p)}".PrintToConsole();
                     return dict[x] + i * m;
                 }
                 d = (d * @base) % p;
             }
            
             
             return -1;
            // var dict = new Dictionary<BigInteger, BigInteger>();
            // for (var i = 0; i < m; i++)
            // {
            //     var t = BigInteger.ModPow(a, i, p);
            //     dict[t] = i;
            // }
            // var d = BigInteger.ModPow(a, m, p);
            // var cur = BigInteger.One;
            // for (var i = 0; i <= m; i++)
            // {
            //    
            //     var y = ModInvWithExtGcd(cur, p);
            //     if (dict.ContainsKey(y))
            //         return (i * m + dict[y] );
            //     cur *= d;
            // }
            //
            // return -1;
        }
        public static BigInteger BigModPow(BigInteger x, BigInteger n, BigInteger mod)
        {
            if (x == 0)
                return 0;
            if (n == 0)
                return 1;
            if (n == 1)
                return x % mod;
            var t = BigModPow(x, n / 2, mod);
            return (n.IsEven ? BigModMultiply(t, t, mod) : BigModMultiply(BigModMultiply(t, t, mod), x, mod));
        }
        public static BigInteger BigModMultiply(BigInteger x, BigInteger y, BigInteger mod)
        {
            return ((x % mod) * (y % mod)) % mod;
        }
        private static bool IsSqrt(BigInteger n, BigInteger root)
        {
            BigInteger lowerBound = root*root;
            BigInteger upperBound = (root + 1)*(root + 1);

            return (n >= lowerBound && n < upperBound);
        }
    }
}