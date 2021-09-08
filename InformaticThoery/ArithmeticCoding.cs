using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.InformationThoery
{
    public class ArithmeticCoding
    {
        public static Dictionary<T, (double, double)> GetFreqDictionary<T>(IEnumerable<T> data)
        {
            var enumerable = data as T[] ?? data.ToArray();
            
            var freq =
                from g in
                    from num in enumerable
                    group num by num
                let n = enumerable.Length
                select (g.Key, (double)g.Count() / n);

            var freqRange = 
                freq.ResultSaveAggregate((r, e) => (e.Key, r.Item3, r.Item3 + e.Item2), (default(T), 0.0, 0.0));
            
            
            return freqRange.ToDictionary(key => key.Item1, v => (v.Item2, v.Item3));
        }

        public static double Encode<T>(Dictionary<T, (double, double)> dictionary, List<T> input)
        {
            
            if (input.Count == 0)
                return 0;
            var cur = dictionary[input[0]];
       
            for (var i = 1; i < input.Count; i++)
            {
                var c = input[i];
                var range = dictionary[c];
                var diff = cur.Item2 - cur.Item1;
                
                var left = cur.Item1;
                cur.Item1 = left + range.Item1 * diff;
                cur.Item2 = left + range.Item2 * diff;
                
            }

            return new Random().NextDouble() * (cur.Item2 - cur.Item1) + cur.Item1;
        }

        public static List<T> Decode<T>(Dictionary<T, (double, double)> dictionary, double code, int t)
        {
            var newDic = dictionary
                .Select(e =>
                    (key : e.Key, possibility: e.Value.Item2 - e.Value.Item1,range: e.Value))
                .ToDictionary(k => k.key, v => (v.possibility,v.range));
            if(t == 0)
                return new List<T>();
            var s = newDic.
                First(e => e.Value.range.Item1 <= code && e.Value.range.Item2 > code);
            
            
            var tuple = s.Value.range;
            var ans = new List<T>();
            ans.Add(s.Key);
            for (var i = 1; i < t; i++)
            {
                var diff = tuple.Item2 - tuple.Item1;
                var percentage = (code - tuple.Item1) / diff;
                var element = newDic
                    .First(e =>
                        e.Value.range.Item1 <= percentage && e.Value.range.Item2 > percentage);
                ans.Add(element.Key);
                var left = tuple.Item1;
                tuple.Item1 = left + diff * element.Value.range.Item1;
                tuple.Item2 = left + diff * element.Value.range.Item2;
            }
            
            return ans;
        }
        
    }
}