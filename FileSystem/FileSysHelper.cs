using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.FileSystem
{
    public class FileSysHelper
    {
        public static string GetAllBits(IEnumerable<byte> bs, int width = 8)
        {
            return bs.Select(e => Convert.ToString(e, 2).PadLeft(width, '0')).Aggregate("", (a, b) => a + b);
        }

        public static IEnumerable<byte> GetAllBytes(string path)
        {
            return FileSysHelper.GetAllBytes(path);
        }

        public static IEnumerable<byte> To6BitRepresent(byte[] bs)
        {
            //3 byte -> 4byte
            var s = bs.GroupByCount(3);
            if(s[^1].Count < 3)
                s[^1].AddRange(Enumerable.Repeat((byte)0, 3 - s[^1].Count).ToArray());
            var ans = new List<byte>();
            foreach (var g in s)
            {
                var b1 = (byte)(g[0] >> 2);
                var b2 = (byte)((g[0] << 6) | (g[1] >> 4));
                var b3 = (byte)((g[1] << 4) | (g[2] >> 6));
                var b4 = (byte)(0b001111 & g[2]);
                ans.AddRange(new[]{b1, b2, b3, b4});
            }
            return ans;
        }
        public static string GetRangeBits(byte[] bs, int l, int r) // get bit [l, r]
        {
            var byteBegin = l / 8;
            var beginRest = l % 8;
            var byteEnd = r / 8;
            var endRest = r % 8;
            var range = GetAllBits(bs.Skip(byteBegin).Take(byteEnd - byteBegin + 1));
            return range.Skip(beginRest).Take(r - l + 1).Aggregate("",(a,b) => a + b);
        }
        public static string ReadFileAsString(string path)
        {
            return File.ReadAllText(path);
        }

        public static IEnumerable<string> ReadFileAsLines(string path)
        {
            return File.ReadLines(path);
        }

        public static StreamReader OpenFileAsStream(string path)
        {
            return new StreamReader(path);
        }
        
        //随机方式
        public static FileStream OpenFileAsFileStream(string path, FileMode mode)
        {
            FileStream fileStream = new FileStream(path, mode);
            return fileStream;
        }
    }
}