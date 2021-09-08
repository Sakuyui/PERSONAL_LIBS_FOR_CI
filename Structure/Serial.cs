using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Math
{
    public class Serial : IEnumerable<object>
    {
        
        public Dictionary<string, object> DataMap {get; private set; } = new Dictionary<string, object>();
        public List<string> ColumnNames => DataMap.Keys.ToList();
        public List<object> Values => DataMap.Values.ToList();

        public IEnumerable<Tuple<string, object>> Tuples => 
            (from e in DataMap.Keys select new Tuple<string, object>(e, DataMap[e])).ToList();

        public Serial(IEnumerable<string> columnNames)
        {
            foreach (var s in columnNames)
            {
                DataMap[s] = null;
            }
        }
        public static Serial operator +(Serial s1, Serial s2)
        {
            if(s1.Count() != s2.Count()) throw new ArithmeticException("长度不匹配: " + s1.Count() + " != " + s2.Count());
            var result = new Serial(s1.ColumnNames);
            foreach (var col in s1.ColumnNames)
            {
                try
                {
                    result[col] = (dynamic) s1[col] + (dynamic) s2[col];
                }
                catch (Exception e)
                {
                    result[col] = "NaN";
                }
            }

            return result;
        }
        public Serial Map(Func<string, object, object> lambdaFunc)
        {
            var s = new Serial(ColumnNames.ToArray());
            foreach (var e in s.ColumnNames)
            {
                s[e] = lambdaFunc.Invoke(e, this[e]);
            }
            return s;
        }
        public Serial() { }
        public Serial(Dictionary<string, object> dictionary)
        {
            DataMap = dictionary;
        }
        public object this[string columnName]
        {
            get => DataMap[columnName];
            set => DataMap[columnName] = value;
        }

        public object this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }
        public Serial this[IEnumerable<string> columns]
        {
            get
            {
                var s = new Serial();
                foreach (var k in columns)
                {
                    if (DataMap.Keys.Contains(k)) s[k] = DataMap[k];
                }

                return s;
            }
        }

      

        public IEnumerator<object> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}