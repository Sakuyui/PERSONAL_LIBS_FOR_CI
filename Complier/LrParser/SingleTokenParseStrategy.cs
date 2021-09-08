using System;
using System.Text.RegularExpressions;
using CIExam.Structure;

namespace CIExam.Complier
{
    public class SingleTokenParseStrategy : ValueTupleSlim
    {
        public string TokenName
        {
            get => (string) this[0];
            private init => this[0] = value;
        }

        public Regex Regex
        {
            get => (Regex) this[1];
            private init => this[1] = value;
        }

        public Action<Token> Action
        {
            get => (Action<Token>) this[2];
            private init => this[2] = value;
        }

        public SingleTokenParseStrategy(string tName, Regex r, Action<Token> action = null):base(null, null, null)
        {
            TokenName = tName;
            Regex = r;
            Action = action;
        }
        
    }
}