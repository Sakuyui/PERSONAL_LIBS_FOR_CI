using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CIExam.Math;
using CIExam.FunctionExtension;
using CIExam.InformationThoery;

namespace CIExam.CI
{
    public class Ci2020
    {
        public static Dictionary<char, int> GetEnCodingDict()
        {
            var dict = new Dictionary<char, int>();
            foreach (var c in Enumerable.Range(0, 26))
                dict[(char)('A' + c)] = c;
            foreach (var c in Enumerable.Range(26, 26))
                dict[(char) ('a' + c - 26)] = c;
            foreach (var c in Enumerable.Range(52, 10))
                dict[(char) ('0' + c - 52)] = c;
            dict['@'] = Convert.ToInt32("111110", 2);
            dict['#'] = Convert.ToInt32("111111", 2);

            return dict;
        }

        public static Dictionary<int, char> GetDecodingDict()
        {
            var dict = GetEnCodingDict();
            return dict.ToDictionary(k => k.Value, v => v.Key);
        }

        
        public static void Test2()
        {
            var decodeDict = GetDecodingDict();
            decodeDict.PrintEnumerationToConsole();
            const string testStr = "000001000111000010111111";
            testStr.Length.PrintToConsole();
            
            //Q1
            testStr.Substring(6 - 1, 11).PrintToConsole();
            testStr.GroupByCount(6)
                .Select(e => decodeDict[System.Convert.ToInt32(e.ConvertToString(), 2)])
                .PrintCollectionToConsole();
            
            
            //Q2
            //var file1Text = System.IO.File.ReadAllBytes("f1.text");
            var file1Text = new[] {41, 42, 43, 44, 45, 46, 47, 00, 06, 05, 48}.Select(e => (byte)e).ToArray();
            var byteDecoded = new List<byte>();
            var i = 0;
            while (i < file1Text.Length)
            {
                if (file1Text[i] != 0)
                {
                    byteDecoded.Add(file1Text[i++]);
                }
                else
                {
                    var p = byteDecoded[i + 1];
                    var d = byteDecoded[i + 2];
                    //复号末尾p开始，前面t个复制
                    byteDecoded.AddRange(byteDecoded.TakeLast(p).Take(d));
                    i += 3;
                }
            }
            byteDecoded.PrintCollectionToConsole();
            byteDecoded.Count.PrintToConsole();
            
            
            //Q3 加窗lzw
            var input = byteDecoded;
            var lzwDict = new List<List<byte>>();
            lzwDict.AddRange(Enumerable.Range(0,256).Select(e => new List<byte>{(byte)e}));
            var tempList = new List<byte>();
            var encodeLzw = new List<byte>();
            var sequenceDict = new Dictionary<int, int>();//词典某个词条在哪出现

            i = 0;
            foreach (var e in input)
            {
                
                var iCopy = i;
                
                //消除窗口外的元素
                sequenceDict.RemoveKeys(k => k - iCopy > 255);
                tempList.Add(e);
                //寻找第一个相等序列
                var (index, seq) = lzwDict.FindFirst((d, _) => d.SequenceEqual(tempList));
                //遇到序列不相符，需要加入序列
                
                //确保当前记录的记录仍在窗口内
                if (index < 0 || sequenceDict.ContainsKey(index) && sequenceDict[index] - i >= 255)
                {
                    tempList.RemoveAt(tempList.Count - 1);
                    var (seqIndex, data) = lzwDict
                        .FindFirst((d, _) => d.SequenceEqual(tempList));
                    if (data.Count < 3)
                    {
                        //不如不压缩，直接放入
                        encodeLzw.AddRange(data);
                    }
                    else
                    {
                        var lastAppear = sequenceDict[seqIndex]; //该序列最后一次出现的时间
                        //为序列编码
                        encodeLzw.Add(0);
                        encodeLzw.Add((byte)(i - lastAppear + 1));
                        encodeLzw.Add((byte)(i - lastAppear + 2 - data.Count));
                    }
                    
                    //不存在现在的序列的情况
                    tempList.Add(e);
                    lzwDict.Add(new List<byte>(tempList)); //添加该序列
                    sequenceDict[lzwDict.Count - 1] = i;
                    
                    tempList.Clear();
                    tempList.Add(e);
                    i++;
                }
                else
                {
                    sequenceDict[index] = i;
                }
            }
            
            
            //Q4
            //使用频率解暗号而已
            const string inputEngText = "asdskfgherikngvfdgvbfdg";
            var freq = inputEngText
                .GroupBy(e => e).Select(g => (g.Key, g.Count() / (double) inputEngText.Length));
            //接下来对照频率文件猜就行
            var charDict = Enumerable.Range(0, 26).ToDictionary(e => 'a' + e, e=> 'a' + e);
            //guess and change the mapping relation according the frequency.
            charDict['a'] = 'b';
            


            //Q5 数论相关
            var byteM = new List<List<byte>>();
            
            var eKey = BigInteger.Parse("551263368336670859257571");
            var n = BigInteger.Parse("3858843578360632069557337");
            //破解RSA
            var dKey = GetRsaD(n, eKey);

            
            const string cipherMsg = "1321312 213414";
            var c = cipherMsg.Split(" ").Select(BigInteger.Parse).ToList();


            for (i = 0; i < c.Count; i++)
            {
                var cMsg = c[i]; //加密后的数据
                //解密
                var msg = BigModPow(cMsg, dKey);
                var bs = msg.ToByteArray();
                byteM.Add(bs.GroupByCount(8).Select(e => (byte)Convert.ToInt32(e)).ToList());
                var result = Encoding
                    .Convert(Encoding.UTF8, Encoding.Default, byteM.SelectMany(e => e).ToArray());
            }
            


        }

       
        private static BigInteger GetRsaD(BigInteger n, BigInteger eKey)
        {
            return 1;
        }
        public static BigInteger BigModPow(BigInteger x, BigInteger n)
        {
            if (n == 0)
                return 1;
            if (n == 1)
                return x;
            var t = BigModPow(x, n / 2);
            return n.IsEven ? BigModMultiply(t, t, n) : BigModMultiply(BigModMultiply(t, t, n), x, n);
        }
        public static BigInteger BigModMultiply(BigInteger x, BigInteger y, BigInteger n)
        {
            return ((x % n) * (y % n)) % n;
        }
        public static void Test()
        {
            //输入: 正方形瓷砖大小集合
            var input = "113142421231".Select(e => e - '0').ToList();
            //输入 masu：图形大小
            const int masu = 10;
            
            
            var matrix = new Matrix<int>(int.MaxValue, masu);
            foreach (var c in input)
            {
                //Linq寻找第一个合法行
                var (pX, firstLine) 
                    = matrix.FindFirst((e ,i) => {
                        var left = e.IndexOf(0);
                        var right = left + c;
                        
                        //不能超出边界，并且指定大小子矩阵内元素都要是0
                        var isInEdge = right <= masu && left >= 0;
                        var isNoCollision = matrix[i, i + c - 1, left, right - 1].
                            ElementEnumerator.All(element => element == 0);
                        return isInEdge && isNoCollision; 
                    });
                
                var pY = firstLine.IndexOf(0);
                
                //(pX, pY)为合法的能够开始放砖块的x, y坐标
                //放置砖块，填充子矩阵
                matrix[pX, pX + c - 1, pY, pY + c - 1] = new Matrix<int>(c, c , 1);
            }
            
            var ans = matrix.FindFirst((line, i) => line.All(e => e == 0));
            (input.ToEnumerationString() + "深度为 " + ans.Item1).PrintToConsole();
        }
    }
}