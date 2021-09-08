using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Complier
{
    public class ProducerDefinitionItem
    {
        public readonly string LeftSymbol;
        public readonly string ProduceItem;
        public ProducerDefinitionItem(string parseString)
        {
            var exp = parseString.Split("->");
            LeftSymbol = exp[0];
            ProduceItem = exp[1];
            if (exp.Length != 2)
                throw new ArgumentException("expression error");
        }

        public override string ToString()
        {
            return LeftSymbol + " -> " + ProduceItem;
        }

        private ProducerDefinitionItem(string left, string right)
        {
            LeftSymbol = left;
            ProduceItem = right;
        }

        public static IEnumerable<ProducerDefinitionItem> ParseProducerExpression(string expression)
        {
            var p = expression.Split("->");
            if (p.Length != 2)
                throw new ArgumentException("expression error");
            var ps = p[1].Split("|");

            return ps.Select(t => new ProducerDefinitionItem(p[0], t)).ToList();
        }
    }
}