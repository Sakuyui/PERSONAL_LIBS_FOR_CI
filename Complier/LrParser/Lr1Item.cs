using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.Complier
{
    public class Lr1Item : ICloneable
    {
        public string StartWord;
        public List<string> ProduceItems = new();
        public SortedSet<string> SearchWordList = new();

        public int DotPos = 0;
        public Lr1Item(string startWord, List<string> produceItems, List<string> searchWordList)
        {
            StartWord = startWord;
            ProduceItems = produceItems;
            SearchWordList = new SortedSet<string>(searchWordList);
        }

        public bool IsReductionItem()
        {
            return DotPos == ProduceItems.Count;
        }
        public Lr1Item MoveForward()
        {
            if (IsReductionItem())
                return (Lr1Item) Clone();
            var c = (Lr1Item)Clone();
            ("move " + c).PrintToConsole();
            c.DotPos++;
            
            return c;
        }
        public override string ToString()
        {
            var l = ProduceItems.Take(DotPos);
            var r = ProduceItems.Skip(DotPos);
            
            return "[" + StartWord + "] => " + (l.Any() ? l.ToEnumerationString() : "")
                   + "." + (r.Any() ? r.ToEnumerationString() :"")
                   + " , " + SearchWordList.ToEnumerationString();
        }

        public object Clone()
        {
            var clone = new Lr1Item(this.StartWord, new List<string>(ProduceItems),
                new List<string>(this.SearchWordList));
            clone.DotPos = this.DotPos;
            return clone;
        }

        public List<string> GetForwardInput()
        {
            return IsReductionItem() ? SearchWordList.ToList() : ProduceItems.Skip(DotPos + 1).Union(SearchWordList).ToList();
        }
        public override int GetHashCode()
        {
            return ProduceItems.ToEnumerationString().GetHashCode() + SearchWordList.ToEnumerationString().GetHashCode() + DotPos;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Lr1Item item)
            {
                return item.DotPos == DotPos && item.StartWord == StartWord &&
                       item.ProduceItems.SequenceEqual(ProduceItems)
                       && item.SearchWordList.ToHashSet().SequenceEqual(SearchWordList.ToHashSet());
            }

            return false;
        }
    }
}