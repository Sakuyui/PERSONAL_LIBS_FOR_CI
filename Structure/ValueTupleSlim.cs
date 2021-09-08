using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Structure
{
    public class ValueTupleSlim : IEnumerable<object>
    {
        public readonly ValueList<dynamic> Data;
        public int Count => Data.Count;
        public ValueTupleSlim(params object[] objs)
        {
            Data = new ValueList<dynamic>();
           
            Data.AddRange(objs);
            
          
        }

        public static ValueTupleSlim FromList<T>(IEnumerable<T> list)
        {
            var tp = new ValueTupleSlim();
            foreach (var e in list)
            {
                tp.Add(e);
            }

            return tp;
        }

        public void Add(dynamic d)
        {
            Data.Add(d);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public override bool Equals(object? obj)
        {
            return Data.Equals(((ValueTupleSlim) obj)?.Data);
        }

        public dynamic this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public override string ToString()
        {
            return Data.ToEnumerationString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}