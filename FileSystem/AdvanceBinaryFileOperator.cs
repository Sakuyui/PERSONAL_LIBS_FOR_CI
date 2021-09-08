using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CIExam.FunctionExtension;

namespace CIExam.FileSystem
{
    public class AdvanceBinaryFileOperator : IEnumerable<byte>
    {
        private readonly List<byte> _bs = new();
        private readonly string _path;
        private bool _changed = false;
        public IEnumerable<byte> Bytes => _bs;

       
        public AdvanceBinaryFileOperator(string path)
        {
            _path = path;
            Read();
        }

        private void Read()
        {
            _bs.Clear();
            if (!File.Exists(_path))
            {
                var s = File.Create(_path);
                $"create {_path}".PrintToConsole();
                s.Close();
            }
            _path.PrintToConsole();
            _bs.Clear();
            _bs.AddRange(File.ReadAllBytes(_path).ToList());
        }
        public byte this[int pos]
        {
            get => _bs[pos];
            set => _bs[pos] = value;
        }

        public byte[] this[int l, int r]
        {
            get => _bs.Skip(l).Take(r - l + 1).ToArray();
            set
            {
                for (var i = 0; i < value.Length && i < _bs.Count; i++)
                {
                    _bs[l + i] = value[i];
                }
            }
        }
        
        public void SaveChange()
        {
            if(!_changed)
                return;
            File.WriteAllBytes(_path, _bs.ToArray());
            _changed = false;
        }

        public void Clear()
        {
            _bs.Clear();
            _changed = true;
        }

        public IEnumerable<string> GetBitsRepresent()
        {
            return _bs.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'));
        }
        
        public static byte[] BitsToBytes(string bitString)
        {
            var g = bitString.GroupByCount(8);
            var ans = new byte[g.Count];
            foreach (var e in g)
            {
                var b = (byte) 0;
                var mask = 1 << 7;
                for (var i = 0; i < e.Count; i++, mask >>= 1)
                {
                    b |= (byte) (mask & (e[i] - '0'));
                }
                return ans;
            }
            return ans;
        }

        public void Append(IEnumerable<byte> bytes)
        {
            _bs.AddRange(bytes);
            _changed = true;
        }
        public void AppendSingle(byte b)
        {
            _bs.Add(b);
            _changed = true;
        }

        public void Insert(int pos, IEnumerable<byte> bytes)
        {
            _bs.InsertRange(pos, bytes);
            _changed = true;
        }
        public void InsertSingle(int pos, byte b)
        {
            _bs.Insert(pos, b);
            _changed = true;
        }

        public void OverWrite(int index, IEnumerable<byte> bytes)
        {
            var enumerable = bytes as byte[] ?? bytes.ToArray();
            var len = enumerable.Length;
            var target = index + len - 1;
            var diff = target - len;
            if (diff > 0)
            {
                _bs.AddRange(Enumerable.Repeat((byte) 0, diff));
            }

            for (var i = 0; i < len; i++)
            {
                _bs[index + i] = enumerable[i];
            }

            _changed = true;
        }

        public void Replace(int index, int length, IEnumerable<byte> bytes)
        {
            _bs.RemoveRange(index, length);
            _bs.InsertRange(index, bytes);
            _changed = true;
        }
       
        public IEnumerable<byte> GetEnumerableFrom(int begin)
        {
            return _bs.Skip(begin);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return _bs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}