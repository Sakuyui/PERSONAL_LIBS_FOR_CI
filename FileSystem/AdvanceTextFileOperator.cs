using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.FileSystem
{
    public class AdvanceTextFileOperator: IEnumerable<string>
    {
        private readonly List<StringBuilder> _bs = new();
        private readonly string _path;
        private bool _changed = false;


        public static void Test()
        {
            var fw = new AdvanceTextFileOperator("d:\\out1.txt");
            fw.PrintToConsole();
            fw.Insert(0, 1, "12344");
            fw.PrintToConsole();
            fw.OverrideInsert(2, 1, "123444覆盖式写入");
            fw.PrintToConsole();
            fw.DeleteLine(0);
            fw.Insert(4, 8, "就在这画");
            fw.Insert(7, 9, "在不存在的行列画");
            fw.PrintToConsole();
            fw.GetCharMatrix().PrintToConsole();
            (fw.GetCharMatrix() - fw.GetCharMatrix()).Sum(e => ((List<dynamic>)e)
                .Sum(e2 => e2)).PrintToConsole();
            
            // var f = 
            //     AdvanceTextFileWriter.FromMatrix(nums.ToMatrix(3, 3), 
            //         (i, ints, arg3, arg4, arg5) => $" {arg5}({i},{arg4.Count()}) "
            //         , "d:\\file1.txt", (i, ints) => $"+({i}) ", 
            //         (i, ints, arg3) => " <END>");
            
        }
        
        public AdvanceTextFileOperator(string path)
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
            _bs.AddRange(File.ReadAllLines(_path).Select(line => new StringBuilder(line)));
        }

        //可以分行载入，不用想一次载入所有，分段式加载
        public void SaveChange()
        {
            if(!_changed)
                return;
            File.WriteAllLines(_path, _bs.Select(e => e.ToString()).ToArray());
            _changed = false;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _bs.Select(b => b.ToString()).GetEnumerator();
        }

        public override string ToString()
        {
            //System.IO.StreamReader file =
              //1  new System.IO.StreamReader(@"c:\test.txt");
            return _bs.Aggregate("", (a, b) => a + b + "\r\n");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #region Sigal Line Operation
        
        public void RemoveString(int lineCode, int left, int length)
        {
            if (lineCode < _bs.Count)
            {
                this[lineCode].Remove(left, length);
            }
        }
        public StringBuilder AppendString(int lineCode, string data)
        {
            ComplementLines(lineCode);
            var len = data.Length;
            var sb= _bs[lineCode];
            sb.Append(data);
            _changed = true;
            return sb;
        }

        private void ComplementLines(int lineCode)
        {
            var t = lineCode - _bs.Count + 1;
            //防止行数不够
            if (lineCode < _bs.Count) return;
            
            for (var i = 0; i < t; i++)
            {
                _bs.Add(new StringBuilder());
            }
        }
        //覆盖模式的插入
        public StringBuilder OverrideInsert(int lineCode, int begin, string data)
        {
            ComplementLines(lineCode);

            var len = data.Length;
            var sb= _bs[lineCode];
            if (begin + len >= sb.Length)
            {
                sb.Append(' ', begin + len - sb.Length);
            }

            sb.Remove(begin, len);
            sb.Insert(begin, data);
            _changed = true;
            return sb;
        }
        #endregion
        
        public Matrix<char> GetCharMatrix()
        {
            var r = _bs.Count;
            var c = _bs.Select(e => e.Length).Max();
            var mat = new Matrix<char>(r, c);
            for (var i = 0; i < r; i++)
            {
                for (var j = 0; j < _bs[i].Length; j++)
                {
                    mat[i, j] = _bs[i][j];
                }
            }
            return mat;
        }

        //从矩阵创建文件，非常灵活。注意当使用midProcess时，rowAggregateFunc将会无效。
        public static AdvanceTextFileOperator FromMatrix<T>(Matrix<T> mat, Func<int, List<T>, 
                StringBuilder, IEnumerable<T>, T, string> midProcess, string fileName,
            Func<int, List<T>, string> preProcess = null,
            Func<int, List<T>, StringBuilder, string> postProcess = null)
        {
            var rowCount = mat.RowsCount;
            var lines = new List<StringBuilder>();
            var rows = mat.RowsEnumerator.ToList();
            var file = new AdvanceTextFileOperator(fileName);
            for (var i = 0; i < rowCount; i++)
            {
                var row = mat[i].ToList();
                var sb = new StringBuilder(preProcess?.Invoke(i, row) ?? "");
                var element = new List<T>();
                for (var j = 0; j < row.Count; j++)
                {
                    var mid = midProcess.Invoke(i, row, sb, element, row[j]);
                    sb.Append(mid);
                    element.Add(row[j]);
                }
                var post = postProcess?.Invoke(i, row, sb);
                sb.Append(post);
                file.AppendLine(sb.ToString());
            }
            file.SaveChange();
            return file;
        }
        public static AdvanceTextFileOperator FromMatrix<T>(Matrix<T> mat, Func<string, T, string> aggregateFunc, string fileName,
            Func<int, List<T>, string> preProcess = null,
            Func<int, List<T>, StringBuilder, string> postProcess = null)
        {
            var rowCount = mat.RowsCount;
            var rows = mat.RowsEnumerator.ToList();
            var file = new AdvanceTextFileOperator(fileName);

            var lines = mat.RowsEnumerator.Select((row, index) =>
                row.Aggregate(preProcess?.Invoke(index, row) ?? "", aggregateFunc.Invoke)
            );
            lines.ElementInvoke(line => file.AppendLine(line));
            file.SaveChange();
            return file;
        }
        //插入模式
        public StringBuilder Insert(int lineCode, int begin, string data)
        {
            ComplementLines(lineCode);
           
            var sb= _bs[lineCode];
            if (begin > sb.Length)
            {
                sb.Append(' ', begin - sb.Length);
            }
            sb.Insert(begin, data);
            _changed = true;
            return sb;
        }

        #region Line Operation
        public void DeleteLine(int lineCode)
        {
            if(lineCode < _bs.Count)
                _bs.RemoveAt(lineCode);
            _changed = true;
        }

        public void AppendLine(string line)
        {
            _bs.Add(new StringBuilder(line));
            _changed = true;
        }

        public void InsertLine(string line, int pos)
        {
            
            _bs.Insert(pos, new StringBuilder(line));
            _changed = true;
        }
        #endregion


        
        public void Draw(int sRow, int sCol, int eRow, int eCol, List<string> data)
        {
            for (var i = sRow; i <= eRow; i++)
            {
                var s = data[i];
                if (s.Length > eCol - sCol + 1)
                {
                    s = s[..(eCol - sCol + 1)];
                }
                else if(s.Length < eCol - sCol + 1)
                {
                    var diff = (eCol - sCol + 1) - s.Length;
                    s += Enumerable.Repeat('0', diff).ConvertToString();
                }
                OverrideInsert(sRow, sCol, s);
            }

            _changed = true;
        }
        public StringBuilder this[int lineCode] => _bs[lineCode];
        public char this[int lineCode, int columnCode] => _bs[lineCode][columnCode];
        //子区域处理的this
    }
}