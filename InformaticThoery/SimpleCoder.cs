using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CIExam.Math;
using CIExam.Math.NumSimulation;
using CIExam.FunctionExtension;

namespace CIExam.InformaticThoery
{
    public static class SimpleCoder
    {

        public static void MultiplyCipher()
        {
            
            //P盒是位置变换。 8 bit -> 8 bit
            byte[] PBox(IReadOnlyList<byte> input, IEnumerable<byte> permutation) => 
                permutation.Select(index => input[index]).ToArray();
        
            //S盒是置换。输入3bit。输出3bit。过程是3 bit ->编码8bit -> 位置置换 -> 编码3bit
            byte[] SBox(IReadOnlyList<int> input, IEnumerable<byte> permutation) //编码-> 置换 -> 解码
            {
                var encode = Enumerable.Range(0, 1 << 3)
                    .Select(e => Convert.ToString(e, 2).PadLeft(3, '0'))
                    .Select(b => b[0] - '0' == input[0] && b[1] - '0' == input[1] && b[2] - '0' == input[2] ? 1 : 0)
                    .Aggregate("", (a, b) => a + b);
                return Convert.ToInt32(encode, 2).ToString().Select(e => (byte) (e - '0')).ToArray();
            }
            
        }
        
        //密码链：缺点：必须整个数据块到达才行。如果流模式的话可以选择密码反馈模式
        public static IEnumerable<byte> CipherChainEncipher(IEnumerable<byte> input, IEnumerable<byte> initVec, int k)
        {
            var res = new List<byte>();
            var cur = initVec.ToArray();
            var block = input.GroupByCount(8); //8bit分组
            //补全8
            if (block[^1].Count < 8)
            {
                block[^1].AddRange(Enumerable.Repeat((byte)0, 8 - block[^1].Count));
            }
            
            //异或完后，还要置换加密。也可以是其他加密方式
            byte ReplaceCipher(byte b)
            {
                if (b <= 'z' && b >= 'a')
                {
                    return (byte) ('a' + (b + k - 'a') % 26);
                }
                return (byte) ('A' + (b + k - 'A') % 26);
            }

            // byte ReplaceCipherDecode(byte b)
            // {
            //     if (b <= 'z' && b >= 'a')
            //     {
            //         return (byte) ('a' + (b - k - 'a') % 26);
            //     }
            //     return (byte) ('A' + (b - k - 'A') % 26);
            // }
            
            foreach (var o1 in block.Select(vec => vec.Zip(cur, (a, b) => (byte) (a ^ b)).Select(ReplaceCipher).ToList()))
            {
                res.AddRange(o1);
                cur = o1.ToArray();
            }
            //解密:c0解密 -> 和初始向量异或, c1解密 -> 和c0异或,...
            
            return res;
        }
        //为了处理流密码，可以用反馈模式。但是也有确定。一位密文错，后面全部影响。可以用流密码模式，其只受初始向量和密钥影响。不受密文错误影响，
        //流密码模式方式为：首先IV初始向量和密钥加密，得到一个密钥，加密当前的传输数据。这个密钥又用以加密输出密钥，继续生成第二块
        
        // public static IEnumerable<byte> CipherFeedbackMode(IEnumerable<byte> input, ulong initVec, int k)
        // {
        //     var res = new List<byte>();
        //     var register64 = initVec;
        //     
        //     
        //     //异或完后，还要置换加密。也可以是其他加密方式
        //     ulong EnCipher(ulong b)
        //     {
        //         //可以是任意算法，对64bit寄存器加密
        //         const ulong key = 48484848;
        //         return b ^ key;
        //     }
        //
        //     foreach (var b in input)
        //     {
        //         //流模式，注意输入是二进制数据。
        //         //1.首先加密寄存器,并获取最左边字节
        //         var encipher = EnCipher(register64) >> 63;
        //         //和当前bit异或
        //         var ci = encipher ^ b;
        //         //寄存器左移一位，放入新ci
        //         register64 <<= 1;
        //         register64 |= ci;
        //     }
        //     //解密: 和加密完全相同的过程。只不过输入的b变成了c
        //     
        //     return res;
        // }
        
        
        //生日攻击: 意味着N个输入与k个输出存在对应关系时。平均只需要生成 n > sqrt(k)条消息。
        //随机生成消息，sqrt(k)条里有一条是对的可能性很大
        
        
        //破解摘要:
        //计数器模式：前面几种方式都不支持随机访问。。计数器采用的是IV递增。   E(IV + i)后与Pi异或，得到ci
        //可能受到密码重用攻击 - 密钥和随机向量应该独立选取
        public static List<byte>[] ConvolutionEncode(IEnumerable<byte> input, int registerCount, List<List<int>> connection)
        {
           
            var outPutCount = connection.Count;
            var data = input.ToArray();
            var register = new byte[data.Length + 1];
            var res = new List<byte>[outPutCount].Select(e => new List<byte>()).ToArray();

            byte GetOutput(IEnumerable<int> connect)
            {
                var t = register.SelectByIndexes(connect).Aggregate((a, b) => (byte) (a ^ b));
                return t;
            }

            void GetOutputs()
            {
                for (var i = 0; i < outPutCount; i++)
                {
                    res[i].Add(GetOutput(connection[i]));
                }
            }
            GetOutputs();
            foreach (var t in data)
            {
                for (var j = registerCount; j >= 1; j--)
                {
                    register[j] = register[j - 1];
                }
                register[0] = t;
                GetOutputs();
            }

            return res;
        }

        
        //增量压缩编码
        public static IEnumerable<T> DeltaEncode<T>(IEnumerable<T> data)
        {
            var input = data.ToArray();
            if (input.Length == 0)
                return ArraySegment<T>.Empty;
            var res = new List<T> {input.First()};
            var pre = input.First();
            for (var i = 1; i < input.Length; i++)
            {
                var delta = (dynamic)input[i] - (dynamic)pre;
                res.Add(delta);
                pre = input[i];
            }
            
            return res;
        }

        public static Matrix<ulong> GetCrcGMatrix(ulong g)
        {
            var hOne = BitAlgorithms.GetHighestOne(g) + 1; //最高x次数 + 1  这里是相当于k
            var gMat = new Matrix<ulong>(hOne, 1);
            var cur = g << (hOne - 1);
            for (var i = 0; i < hOne; i++, cur >>= 1)
            {
                gMat[i, 0] = cur;
            }

            return gMat;
        }


        public static Matrix<ulong> GetCrcH(ulong g, int n)
        {
            //首先求出$h^*(x)=(x^n-1)/g(x)$   （n,k）循环码
           
            //注意x^n - 1必定能被g整除
            var p1 = new Polynomial {[n] = 1, [0] = 1};
            var p2 = new Polynomial();
            while (g != 0)
            {
                var lastOne = BitAlgorithms.GetLowestOne(g);
                p2[lastOne] = 1;
                g &= g - 1;
            }
            var hStar = (p1 / p2).syo;
            var k = BitAlgorithms.GetHighestOne(g) + 1;
            var hs = hStar.Dict.Aggregate((ulong) 0, (a, b) => a | ((ulong) 1 << b.Key));
            var hMat = new Matrix<ulong>(n - k, 1);
            for (var i = 0; i <= n - k - 1; i++, hs <<= 1)
            {
                hMat[n - k - i - 1, 0] = hs;
            }
            return hMat;
        }
        public static IEnumerable<byte> CrcCheck(byte[] polynomialG,byte[] amari, byte[] frame, Dictionary<int, int> errorPositionMap)
        {
            var gN = polynomialG.Length;
            var fN = frame.Length;
            var g = ComputerInt.FromArray(System.Math.Max(gN, fN + gN - 1), polynomialG);
            var f = ComputerInt.FromArray(System.Math.Max(gN, fN + gN - 1), frame);
            var r = ComputerInt.FromArray(System.Math.Max(gN, fN + gN - 1), amari);
            
            var t = ((f << gN - 1) + r).Div(g);
            if (t.amari.RepresentNumber == 0)
                return frame;
            var error = errorPositionMap[(int)t.amari.RepresentNumber];
            frame[error] = (byte)(1 - frame[error]);
            return frame;
        }
        
        public static IEnumerable<byte> CrcEnCode(byte[] polynomialG, byte[] frame)
        {
            var gN = polynomialG.Length;
            var fN = frame.Length;
            var g = ComputerInt.FromArray(System.Math.Max(gN, fN + gN - 1), polynomialG);
            var f = ComputerInt.FromArray(System.Math.Max(gN, fN + gN - 1), frame);
            f <<= gN - 1; //数据要左移生成多项式的最高次数
            var res = f.Div(g); //多项式除法 - 用二进制位模拟
            return res.amari.Bytes;
        }
        
        
        public static IEnumerable<int> HammingDeCode(IEnumerable<int> data, int k)
        {
            var input = data.Prepend(0).ToList();
            var res = new int[input.Count - k - 1];
            
            var checkBitPos = Enumerable.Range(0, k).Select(e => 1 << e).ToList();
            var dataBitPos = Enumerable.Range(1, k + res.Length).Except(checkBitPos).ToList();
            $"data bit pos = {dataBitPos.ToEnumerationString()}".PrintToConsole();
            //copy
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = input[dataBitPos[i]];
            }
            $"tmp res = {res.ToEnumerationString()}".PrintToConsole();
            
            //check
            var checkStr = checkBitPos.Select((pos, index) => {
                
                var lastOne = BitAlgorithms.GetLowestOne((ulong)pos);
                
                //var data = input.SelectByIndexes(dataBitPos).ToArray();
   
                var bitsIndexes = Enumerable.Range(1, data.Count()).Select(e => e.Bin()
                        .PadLeft((int) System.Math.Ceiling(System.Math.Log2(data.Count())) + 1, '0'))
                    .Where(e => e[e.Length - lastOne - 1] == '1');
                var bits = bitsIndexes.Select(e => Convert.ToInt32(e, 2));
                $"check bits = {bits.ToEnumerationString()}".PrintToConsole();
                $"check data = {input.SelectByIndexes(bits).ToEnumerationString()}".PrintToConsole();
                var b = input.SelectByIndexes(bits).Aggregate(0, (b1, b2) => b1 ^ b2);
                
                
                b.PrintToConsole();
                return (char)(b + '0');
                
            }).Reverse().ConvertToString();
            $"check str = {checkStr}".PrintToConsole();
            var checkResult = Convert.ToInt32(checkStr, 2);
            input[checkResult] = input[checkResult] == 1 ? 0 : 1;
            return input.SelectByIndexes(dataBitPos);
            if (!dataBitPos.Contains(checkResult)) 
                return res;
            {
                var index = dataBitPos.IndexOf(checkResult);
                res[index] = res[index] == 1 ? 0 : 1;
            }

            return res;
        }
        public static IEnumerable<int> HammingEnCode(IEnumerable<int> data)
        {
            var input = data as int[] ?? data.ToArray();
            var n = input.Length;

            static int Calc1(int k) => (int)BigInteger.Pow(new BigInteger(2), k) - k;
            int FindMinK()
            {
                //2^k - k >= n + 1;
                var t = n + 1;
                var l = 0;
                var r = n;
                while (l <= r)
                {
                    var m = (r - l) / 2 + l;
                    var c = Calc1(m);
                    if (c < t)
                    {
                        l = m + 1;
                    }
                    else
                    {
                        r = m - 1;
                    }
                }
                return l;
            }
            //确定要添加的位数
            var k = FindMinK();
            var res = Enumerable.Repeat(-1, n + k + 1).ToArray();
            var cPos = Enumerable.Range(0, k).Select(e => 1 << e).ToList();
            var dPos = Enumerable.Range(1, n + k).Except(cPos).ToList();
            var dBitSet = dPos.Select(e => e.Bin().PadLeft(k, '0')).ToList();
            //首先按顺序放数据
            dPos.ElementInvoke((pos,i)=> res[pos] = input[i]);
            //res.PrintEnumerationToConsole();
            //接下来就是处理校验位惹
            for (var i = 0; i < k; i++)
            {
                var onePos = i;
                var selDataIndexes = dBitSet.Select((pos, index) => (pos, index))
                    .Where(e => e.pos[e.pos.Length - i - 1] == '1').Select(e => e.index);
                var selData = dBitSet.SelectByIndexes(selDataIndexes).Select(e => Convert.ToInt32(e, 2));
                
                //$"{selData.ToEnumerationString()},{res.SelectByIndexes(selData).ToEnumerationString()}".PrintToConsole();
                
                var ci = res.SelectByIndexes(selData).Aggregate(0, (a, b) => a ^ b);
                
                res[1 << i] = ci;
            }

            //"".PrintToConsole();
            //"".PrintEnumerationToConsole();
            //res.PrintEnumerationToConsole();
            //"".PrintToConsole();
            //"".PrintEnumerationToConsole();
            return res.Skip(1);
        }
        
        
        //哥伦布编码Colomb/Rice 适合变化范围大的编码。
        //Rice是范围为2的整数次方时的特殊情况
        /**
         * Example:
Encode the 8-bit value 18 (0b00010010) when K = 4 (M = 16)
S & (M - 1) = 18 & (16 - 1) = 0b00010010 & 0b1111 = 0b0010
S >> K = 18 >> 4 = 0b00010010 >> 4 = 0b0001 (10 in unary)
         **/
        //RICE其实是瞬时码。每遇到一个1就是新的一个码，然后遇到0就说明接下来是余数，但是余数位数是确定的。
        public static string RiceGolombEncode(IEnumerable<int> nums, int k)
        {
            var m = (1 << k) - 1; //掩码
            (from x in nums select  x >> k).PrintEnumerationToConsole();
            return 
                (from num in nums 
                    let s = num & m 
                    let w = (uint)num >> k 
                select 
                    (w == 0 ? "" :Enumerable.Repeat('1', (byte)w).ConvertToString()) + "0" + s.Bin().PadLeft(k, '0'))
                .Aggregate( (a, b) => a + b);
        }

        public static IEnumerable<int> RiceDecode(string code, int k)
        {
            var sb = new StringBuilder();
            var state = 0;
            var s = 0;
            var rest = 0;
            var ans = new List<int>();
            var m = 1 << k;
            foreach (var c in code)
            {
                switch (state)
                {
                    case 0 when c == '1':
                        s++;
                        break;
                    case 0 when c == '0':
                        state = 1;
                        rest = k;
                        sb.Clear();
                        break;
                    case 1:
                    {
                        sb.Append(c);
                        rest--;
                        if (rest != 0) continue;
                        s = 0;
                        ans.Add(s * m + Convert.ToInt32(sb.ToString(),2));
                        break;
                    }
                }
            }

            return ans;
        }
        
        public static string GolombEncode(IEnumerable<int> nums, int m)
        {
            return
                (from num in nums
                    let q = num / m //商
                    let w = num % m //余数
                    let len = (int)System.Math.Ceiling(System.Math.Log2(m))
                select 
                    Enumerable.Repeat('1', q).ConvertToString() + "0" + w.Bin().PadLeft(len,'0'))
                .Aggregate((a, b) => a + b);
            //1的数量代表是商，遇到一个0后，后面的数固定是由m决定的位数
        }
        
        
        public static IEnumerable<int> ToBaseK(int n, int k)
        {
            if (n == 0)
                return new List<int> {0};
            var res = new List<int>();
            while (n != 0)
            {
                var r = (n % k + System.Math.Abs(k)) % System.Math.Abs(k);
                res.Add(r);
                n -= r;
                n /= k;
            }
            res.Reverse();
            
            return res;
        }
        
        public static string ToBaseNeg2(int n) {
            var nums = ToBaseK(n, -2);
            string res;
            return nums.Aggregate("", (a, b) => a + b);
        }
        
    }
    public class RunLenCoder
    {
     
        public static string CommonEnCode<T>(IEnumerable<T> data)
        {
            var output = "";
            var arr = data as T[] ?? data.ToArray();
            if (!arr.Any())
                return output;
            
            
            var f = arr.First();
            var c = 1;
            for (var i = 1; i < arr.Length; i++)
            {
                if (arr[i].Equals(f))
                {
                    c++;
                }
                else
                {
                    //output
                    output += c + " " + f + " ";
                    f = arr[i];
                    c = 1;
                }
            }

            
            if (c != 0)
            {
                output += c + " " + f;
            }
            return output.Trim();
        }

        public static List<T> DeCode<T>(string code, Func<string, T> selFunc)
        {
            var ans = code.Split(" ").GroupByCount(2)
                .Select(e => Enumerable.Repeat(selFunc.Invoke(e[1]), int.Parse(e[0])));
            
            return ans.SelectMany(e => e).ToList();
        }
        
        
        
    }
}