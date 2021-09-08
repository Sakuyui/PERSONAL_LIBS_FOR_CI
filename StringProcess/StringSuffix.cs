using System.Collections.Generic;
using System.Linq;
using CIExam.StringProcess;

namespace CIExam.Structure
{
    public class StringSuffix
    {

        public static List<string> CreateSuffixArray(string s)
        {
            return Enumerable.Range(0, s.Length + 1).Select(e =>
                new string(s.Skip(e).ToArray())).OrderBy(e => e).ToList();
        }
        public static Trie CreateSuffixTrie(string s)
        {
            var t = Enumerable.Range(0, s.Length + 1).Select(e =>
                new string(s.Skip(e).ToArray())).OrderBy(e => e).ToList();
            var trie = new Trie(t);
            return trie;
        }
    }
}