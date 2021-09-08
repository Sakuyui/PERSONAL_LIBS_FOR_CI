using System.Collections.Generic;
using CIExam.Complier.LrParser;

namespace CIExam.Complier
{
    public class Token
    {
        public readonly string ParsedStr;
        //attribution
        private readonly Dictionary<string, object> _attributions = new();
        public readonly string TokenName;
        public Token(string name, TokenType type, string parsedStr)
        {
            ParsedStr = parsedStr;
            TokenName = name;
        }

        public override string ToString()
        {
            return "[" + TokenName + "] " + ParsedStr;
        }

        public object this[string name]
        {
            get => _attributions.ContainsKey(name) ? _attributions[name] : null;
            set => _attributions[name] = value;
        }
    }
}