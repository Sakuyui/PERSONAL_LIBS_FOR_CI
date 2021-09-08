using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CIExam.FunctionExtension;

namespace CIExam.Math
{
    public static class DataFrameTest
    {
        public static void Test()
        {
            var df = new DataFrame(new[] {"cab", "cb","aa"});
            df.AddRow("11","22","33");
            df.PrintToConsole();
            
        }
    }
    
    
    
    public class DataFrame : IEnumerable<Serial>
    {
      
        
        
        
        private List<string> _columnNames;
        public List<string> ColumnNames => _columnNames;
        public List<Serial> Serials = new List<Serial>();


        public Dictionary<string, int> _colNameMapping = new Dictionary<string, int>();
        public Dictionary<string, object> MappingDict
        {
            get
            {
                var map = new Dictionary<string, object>();
                foreach (var k in _columnNames)
                {
                    map[k] = null;
                }
                return map;
            }
        }
        
        public void RemoveAt(int i)
        {
            Serials.RemoveAt(i);
        }
        //index
        public DataFrame(IEnumerable<string> columnNames)
        {
            var enumerable = columnNames as string[] ?? columnNames.ToArray();
            if (enumerable.GroupBy(g => g).Where(s => s.Count() > 1).ToList().Count > 0)
            {
                throw new Exception("DataFrame can't have more than one columns with the same name.");
            }
            _columnNames = new List<string>(enumerable);
           UpdateColNames();
        }

        private void UpdateColNames()
        {
           
            _colNameMapping.Clear();
            for (var i = 0; i < _columnNames.Count; i++)
            {
                _colNameMapping[_columnNames[i]] = i;
            }

            foreach (var s in Serials)
            {
                var keys = s.ColumnNames;
                for (var j = 0; j < _columnNames.Count; j++)
                {
                    var tData = s.DataMap[keys[j]];
                    var oldName = keys[j];
                    
                    s.DataMap[_columnNames[j]] = tData;
                    if (_columnNames[j] != oldName)
                        s.DataMap.Remove(oldName);
                }
            }

           
        }

        public void AddRow<T>(IEnumerable<T> data)
        {
            var m = MappingDict;
            var d = data.ToArray();
            var index = 0;
            var arr = data.ToArray();
            for (var i = 0; i < arr.Length && i < _columnNames.Count; i++)
            {
                var c = _columnNames[i];
                m[c] = arr[i];
            }
         
            Serials.Add(new Serial(m));
        }

        public void AddRow(params object[] data)
        {
            //(_columnNames.Count, data.Length).PrintToConsole();
            //if(data.Length != _columnNames.Count) throw new ArithmeticException();
            var m = MappingDict;
            var index = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var c = _columnNames[i];
                m[c] = data[i];
            }
            
            Serials.Add(new Serial(m));
        }

        public void AddRow(Dictionary<string, object> map)
        {
            var set = new HashSet<string>(_columnNames);
            var s = new Serial(map.Keys);
            
            foreach (var k in map.Keys)
            {
                if (set.Contains(k))
                {
                    s[k] = map[k];
                }
                else
                {
                    //出现新列
                    s[k] = map[k];
                    AddColumn(k);
                }
            }
            Serials.Add(s);
        }

        public override string ToString()
        {
            return ToStringTable();
        }

        public IEnumerable<List<object>> ColumnsDataEnumerator()
        {
            return _columnNames.Select(t => this.Select(s => s[_colNameMapping[t]]).ToList());
        }
        public IEnumerable<(string cName, List<object> data)> ColumnsEnumerator()
        {
            return _columnNames.Select(s => (s, this.Select(serial => serial[_colNameMapping[s]]).ToList()));
        }
        public string ToStringTable()
        {
            
            var maxColumnsWidth  = 
                Serials.Count != 0?
                ColumnsDataEnumerator()
                .Select(e => e.Select(s => s?.ToString()?.Length ?? 4).ToList())
                .Select(e => e.Max()).ToList() : _columnNames.Select(e => e.Length).ToList();
            

            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            
            //header
            // Print splitter
            for (var colIndex = 0; colIndex < _columnNames.Count; colIndex++)
            {
                // Print cell
                var cell = _columnNames[colIndex];
                cell = cell.PadRight(maxColumnsWidth[colIndex]);
                sb.Append(" | ");
                sb.Append(cell);
            }

            // Print end of line
            sb.Append(" | ");
            sb.AppendLine();
            sb.AppendFormat(" |{0}| ", headerSpliter);
            sb.AppendLine();

            if (this.Serials.Count == 0)
                return sb.ToString();
            for (var rowIndex = 0; rowIndex < Serials.Count; rowIndex++)
            {
                for (var colIndex = 0; colIndex < _columnNames.Count; colIndex++)
                {
                    // Print cell
                    var cell = Serials[rowIndex][_columnNames[colIndex]]?.ToString() ?? "null";
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void AddRow(Serial s)
        {
            var d = s.Tuples;
            var map = new Dictionary<string, object>();
            foreach (var t in d)
            {
                map[t.Key] = t.Val;
            }
            AddRow(map);
        }

        public void AddColumnWithCollection(string cName, IEnumerable<object> objects)
        {
            var i = 0;
            foreach (var s in Serials)
            {
                var enumerable = objects as object[] ?? objects.ToArray();
                s[cName] = enumerable[i++];
            }
            
            if (!_columnNames.Contains(cName))
            {
                _columnNames.Add(cName);
            }
        }
        public void AddColumn(string cName, object fill = null)
        {
            foreach (var s in Serials)
            {
                s[cName] = fill;
            }

            if (!_columnNames.Contains(cName))
            {
                _columnNames.Add(cName);
            }
        }
        public void AddColumn(string cName, Func<Serial, object> fullStrategy)
        { 
            if (!_columnNames.Contains(cName))
            {
                _columnNames.Add(cName);
            }
            foreach (var s in Serials)
            {
                s[cName] = fullStrategy.Invoke(s);
            }

           
        }
        public int GetColumnIndex(string columnName)
        {
            return ColumnNames.BinarySearch(columnName);
        }


        public delegate object MapFunction(int index, Serial s);

        public void AddColumn(string cName, MapFunction mapFunction)
        {
            if (!_columnNames.Contains(cName))
            {
                _columnNames.Add(cName);
            }
            for (var i = 0; i < Serials.Count; i++)
            {
                var s = Serials[i];
                s[cName] = mapFunction(i, s);
            }
        }

        public IEnumerable<Serial> this[Range range] => 
            this.SelectByIndexes(Enumerable.Range(range.Start.Value, range.End.Value).ToArray());

        public Serial this[int index]
        {
            get => (index < Serials.Count) ? Serials[index] : null;
            set
            {
                var keys1 = new HashSet<string>(value.ColumnNames);
                var keys2 = new HashSet<string>(_columnNames);
                if (keys1.SetEquals(keys2))
                {
                    Serials[index] = value;
                }else{throw new ArithmeticException();}
            }
        }

        public DataFrame this[Func<Serial, bool> select] =>
            //行条件选择
            this.ConditionSelect(this.ConditionFindWithBoolResult(select)).ToDataFrame(_columnNames.ToArray());

        public DataFrame this[IEnumerable<string> columns]
        {
            get
            {
                IEnumerable<string> collection = columns as string[] ?? columns.ToArray();
                var keys1 = new HashSet<string>(collection);
                var keys2 = new HashSet<string>(_columnNames);
                var finalKeys = keys1.Intersect(keys2);
                var df = new DataFrame(finalKeys);
                foreach (var s in Serials)
                {
                    df.AddRow(s[collection]);
                }

                return df;
            }
        }

        public DataFrame this[string col]
        {
            get => this[new[] {col}];
            set
            {
                if (value == null)
                {
                    AddColumnWithCollection(col, Enumerable.Repeat<object>(null, Serials.Count));
                    return;
                }
                if(value.Serials.Count != Serials.Count || value._columnNames.Count > _columnNames.Count)
                    throw new ArithmeticException();
                AddColumnWithCollection(col, value.ColumnsDataEnumerator().ToList()[0]);
            }
        }

        public DataFrame this[IEnumerable<int> rows] => this.SelectByIndexes(rows.ToArray()).ToDataFrame(_columnNames.ToArray());

        public IEnumerator<Serial> GetEnumerator()
        {
            return ((IEnumerable<Serial>) Serials).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            
            return GetEnumerator();
        }
        public DataFrame ResetColNames(IEnumerable<string> cols)
        {
            var enumerable = cols as string[] ?? cols.ToArray();
            if (enumerable.Length != _columnNames.Count)
                return this;

            var colNames = enumerable.ToArray();
            for (var i = 0; i < colNames.Length; i++)
            {
                _columnNames[i] = colNames[i];
            }
            UpdateColNames();
            return this;
        } 
    }
}