using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.Complier.LrParser
{
    public class Lr1Table
    {
        private DataFrame _goto;
        private DataFrame _transition;

        public DataFrame Goto
        {
            get => _goto;
            set => _goto = value;
        }

        public DataFrame Transition
        {
            get => _transition;
            set => _transition = value;
        }

        public void AddRow()
        {
            _goto.AddRow(_goto.Serials.Count);
            _transition.AddRow(_transition.Serials.Count);
        }

        public override string ToString()
        {
            return _goto.ToStringTable() + "\r\n" + _transition.ToStringTable();
        }

        public int RowCount => _goto.Count();
        public Lr1Table(ProducerDefinition definition)
        {
            var terminations = definition.Terminations;
            var nonTerminations = definition.NonTerminationWords;
            _goto = new DataFrame(nonTerminations.ToArray().Prepend("I(X)"));
            _transition = new DataFrame(terminations.ToArray().Prepend("I(X)").Append("$"));
                
            _goto.PrintToConsole();
            _transition.PrintToConsole();
        }
    }
}