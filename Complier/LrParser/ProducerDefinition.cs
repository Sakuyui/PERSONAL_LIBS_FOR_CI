using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CIExam.FunctionExtension;

namespace CIExam.Complier
{
    public class ProducerDefinition : IEnumerable<ProducerDefinitionItem>
    {
        public string StartWord = "";
        public readonly List<ProducerDefinitionItem> Grammars = new();

        public readonly Dictionary<string, List<ProducerDefinitionItem>> StartWordsMapping =
            new();

        public readonly HashSet<string> Terminations = new();
        public HashSet<string> NonTerminationWords => StartWordsMapping.Keys.ToHashSet();

        public Dictionary<string, List<List<string>>> ProduceMapping;
        public List<ProducerDefinitionItem> this[string nT] => StartWordsMapping[nT];
        public void AddTerminations(string expression)
        {
            foreach (var e in expression.Split("|"))
                Terminations.Add(e);
        }

        public void InitProduceMapping()
        {
            ProduceMapping = SplitProduceWord();
        }
        
        
        private Dictionary<string, List<List<string>>> SplitProduceWord()
        {
            var res = new Dictionary<string, List<List<string>>>();
            var kSet = Terminations.Union(NonTerminationWords).ToHashSet(null);
            kSet.PrintCollectionToConsole();
            
            //对每条产生式处理，转换为列表
            foreach (var p in this)
            {
                var sb = new StringBuilder(p.ProduceItem);
                var list = new List<string>();
                //处理单条产生式

                while (sb.Length > 0)
                {
                    
                    var found = kSet.Where(s => sb.ToString()
                        .IndexOf(s, StringComparison.Ordinal) == 0).ArgMax(e => e.Length).Item2;
                    list.Add(found);
                    sb.Remove(0, found.Length);
                }
                
                
                //list.PrintEnumerationToConsole();
                if (!res.ContainsKey(p.LeftSymbol))
                    res[p.LeftSymbol] = new List<List<string>>();
                res[p.LeftSymbol].Add(list);
            }

            return res;
        }


        public ProducerDefinition(IEnumerable<string> grammarsDefinition, string terminationsExp, string startWord)
        {
            AddGrammars(grammarsDefinition.SelectMany(ProducerDefinitionItem.ParseProducerExpression));
            Grammars.PrintEnumerationToConsole();
            AddTerminations(terminationsExp);
            StartWord = startWord;
        }
        public ProducerDefinition(string grammarDefinition)
        {
            AddGrammar(new ProducerDefinitionItem(grammarDefinition));
        }
        public ProducerDefinition(){}

        public void AddGrammar(ProducerDefinitionItem item)
        {
            Grammars.Add(item);
            if (!NonTerminationWords.Contains(item.LeftSymbol))
                StartWordsMapping[item.LeftSymbol] = new List<ProducerDefinitionItem>();
            StartWordsMapping[item.LeftSymbol].Add(item);
        }
        public void AddGrammars(IEnumerable<ProducerDefinitionItem> item)
        {
            foreach(var g in item)
                AddGrammar(g);
        }

        public IEnumerator<ProducerDefinitionItem> GetEnumerator()
        {
            return Grammars.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}