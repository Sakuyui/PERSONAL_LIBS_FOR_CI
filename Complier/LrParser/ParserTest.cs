using System.Collections.Generic;
using System.Text.RegularExpressions;
using CIExam.FunctionExtension;

namespace CIExam.Complier.LrParser
{
    public class FormalLanguageParserTest
    {


        
        public static void Test2()
        {
            //注意，不能有左递归
            var grammars = new []
            {
                "S->EXP","EXP->(L LEXP)|(A AEXP)|NUM"
                ,"L->let", "LEXP->LF|LF LEXP"
                ,"A->add|mult", "LF->VNAME EXP"
                , "AEXP->EXP EXP", "VNAME->ID"
            };
            
            var terminations = "let|add|mult| |(|)|NUM|ID";
            //G=(N,P,S,T) 非终结符，产生式，开始符号，终结符
            var grammarSet = new ProducerDefinition(grammars, terminations, "S");
            grammarSet.NonTerminationWords.PrintEnumerationToConsole();
            grammarSet.Terminations.PrintEnumerationToConsole();
            
            
            //需要提供CFG文法和词条匹配策略。由于词条匹配本质上是对正则语言的匹配。所以仅需要使用正则表达式进行匹配
            var p = new GrammarParser(grammarSet).SetTokenParseStrategy(new[]
                {
                    new SingleTokenParseStrategy("SPACE", new Regex(" ")),
                    new SingleTokenParseStrategy("let", new Regex("let")),
                    new SingleTokenParseStrategy("add", new Regex("add")),
                    new SingleTokenParseStrategy("mult", new Regex("mult")),
                    new SingleTokenParseStrategy("LBRACE", new Regex("\\(")),
                    new SingleTokenParseStrategy("RBRACE", new Regex("\\)")),
                    new SingleTokenParseStrategy("ID", new Regex("[a-z]+")),
                    new SingleTokenParseStrategy("NUM", new Regex("(\\-?[0-9]+)(.[0-9]+)?")),
                }).ParseForTokens("(let x 2 (mult x (let x 3 y 4 (add x y))))");
            
            p.Parse();
        }
    
        public static void Test()
        {
            // var grammars = new []{"S->A","A->B|B<A'>","<A'>->-B<A'>|+B<A'>|B"
            //     , "B->C|C<B'>", "<B'>->*C<B'>|/C<B'>|C", "C->ID"};
            var grammars = new []{"S->S-T|T","T->T*F|F","F->-F","F->ID"};
            var terminations = "*|-|ID";
            //G=(N,P,S,T) 非终结符，产生式，开始符号，终结符
            var grammarSet = new ProducerDefinition(grammars, terminations, "S");
            grammarSet.NonTerminationWords.PrintEnumerationToConsole();
            grammarSet.Terminations.PrintEnumerationToConsole();
            var p = new GrammarParser(grammarSet)
                .SetTokenParseStrategy(new[]
                {
                    new SingleTokenParseStrategy("*", new Regex("(\\*)")),
                    new SingleTokenParseStrategy("-", new Regex("(\\-)")),
                    new SingleTokenParseStrategy("ID", new Regex("((\\-?[0-9]+)(\\.[0-9]+)?)"),
                        token =>
                        {
                            token["value"] = double.Parse(token.ParsedStr);
                        })
                }).ParseForTokens("1.45*5");
            
            //p.InputList.PrintCollectionToConsole();
            p.Parse();
        }
        public static void Test4()
        {
            //注意，不能有左递归
            var grammars = new []{"S->S+E|S-E|E","E->E*F|E/F|F","F->ID"};
            var terminations = "ID|*|+|/|-";
            
            //G=(N,P,S,T) 非终结符，产生式，开始符号，终结符
            var grammarSet = new ProducerDefinition(grammars, terminations, "S");
            grammarSet.NonTerminationWords.PrintEnumerationToConsole();
            grammarSet.Terminations.PrintEnumerationToConsole();
            var p = new GrammarParser(grammarSet)
                .SetTokenParseStrategy(new[]
                {
                    new SingleTokenParseStrategy("ID", new Regex("[0-9]+")),
                    new SingleTokenParseStrategy("+", new Regex("\\+")),
                    new SingleTokenParseStrategy("*", new Regex("\\*")),
                    new SingleTokenParseStrategy("-", new Regex("\\-")),
                    new SingleTokenParseStrategy("/", new Regex("\\/"))
                }).ParseForTokens("9*7-5");

            var reductions = new Dictionary<string, GrammarParser.ReductionStrategy>
            {
                {
                    "S->S+E", (obj) =>
                    {
                        var left = (int) obj[0];
                        var right = (int) obj[2];
                        return new Token("S", TokenType.N, left + right + "");
                    }
                },
                {
                    "S->S-E", (obj) =>
                    {
                        var left = (int) obj[0];
                        var right = (int) obj[2];
                        return new Token("S", TokenType.N, left - right + "");
                    }
                },
                {
                    "E->E*F", (obj) =>
                    {
                        var left = (int) obj[0];
                        var right = (int) obj[2];
                        return new Token("E", TokenType.N, left * right + "");
                    }
                },
                {
                    "E->E/F", (obj) =>
                    {
                        var left = (int) obj[0];
                        var right = (int) obj[2];
                        return new Token("E", TokenType.N, left / right + "");
                    }
                },
                {
                    "S->E", (obj) =>
                    {
                        var op = (int) obj[0];
                        return new Token("S", TokenType.N, op + "");
                    }
                },
                {
                    "E->F", (obj) =>
                    {
                        var op = (int) obj[0];
                        return new Token("E", TokenType.N, op + "");
                    }
                },
                {
                    "F->ID", (obj) =>
                    {
                        var op = (int) obj[0];
                        return new Token("F", TokenType.N, op + "");
                    }
                },
            };
            p.Parse(reductions);
        }
        
        
        public static void Test3()
        {
            
            var grammars = new []{"S->L=R","S->R","L->*R","L->ID","R->L"};
            var terminations = "ID|*|=";
            //G=(N,P,S,T) 非终结符，产生式，开始符号，终结符
            var grammarSet = new ProducerDefinition(grammars, terminations, "S");
            grammarSet.NonTerminationWords.PrintEnumerationToConsole();
            grammarSet.Terminations.PrintEnumerationToConsole();
            var p = new GrammarParser(grammarSet)
                .SetTokenParseStrategy(new[]
                {
                    new SingleTokenParseStrategy("ID", new Regex("[a-z]+")),
                    new SingleTokenParseStrategy("=", new Regex("=")),
                    new SingleTokenParseStrategy("*", new Regex("\\*"))
                }).ParseForTokens("*c=b");
            
            //p.InputList.PrintCollectionToConsole();
            p.Parse();
        }
    }
}