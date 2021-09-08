using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CIExam.FunctionExtension;
using JJ.Framework.Collections;
using CIExam.Math;
using CIExam.Structure;

namespace CIExam.Complier.Calculation
{
    public class SimpleCalculation
    {     
        
        
        private int Calculate(String s)
        {
            var inputString = new StringBuilder(s.Replace(" ", ""));
            //toback
            var backseq = new List<string>();
            var numRe = new Regex(@"\d+");
            var stack = new Stack<char>();
            while (inputString.Length > 0)
            {
                if (inputString[0] == '(')
                {
                    stack.Push('(');
                    inputString.Remove(0, 1);
                }else if(inputString[0] == ')')
                {
                    while (stack.Any() && stack.Peek() != '(')
                    {
                        backseq.Add(stack.Pop().ToString());
                    }
                    if (stack.Any())
                        stack.Pop();
                    inputString.Remove(0, 1);
                }else if (inputString[0] == '-' || inputString[0] == '+')
                {
                    while (stack.Any() && (stack.Peek() == '+' || stack.Peek() == '-'))
                    {
                        backseq.Add(stack.Pop().ToString());
                    }
                    stack.Push(inputString[0]);
                    inputString.Remove(0, 1);
                }
                else
                {
                    var p = numRe.Matches(inputString.ToString()).First(e => e.Index == 0).Value;
                    inputString.Remove(0, p.Length);
                    backseq.Add(p);
                }
            }

            if (stack.Any())
            {
                backseq.Add(stack.Pop().ToString());
            }
            //backseq.PrintEnumerationToConsole();
            
            
            //calc
            var oprand = new Stack<int>();
            foreach (var token in backseq)
            {
                if (token == "+" || token == "-")
                {
                    var n2 = oprand.Pop();
                    var n1 = oprand.Pop();
                    if(token == "+")
                        oprand.Push(n1 + n2);
                    else
                        oprand.Push(n1 - n2);
                }
                else
                {
                    oprand.Push(int.Parse(token));
                }
            }
            //oprand.Peek().PrintToConsole();
            return oprand.Peek();
        }


        public class CalcTreeNode
        {
            public object Data;
            public readonly List<CalcTreeNode> Children = new();
            public bool IsLeaf => !Children.Any();
            public override string ToString()
            {
                if (Data is ValueTupleSlim tupleSlim)
                {
                    return $"Func:{tupleSlim[1]}({tupleSlim[0]})";
                }
                return Data.ToString();
            }
        }

        public static void Test()
        {
            var c = SimpleCalculation.GetCalcTree<int>(new[] {"A", "+", "B", "*", "C"}, new Dictionary<string, int>()
            {
                {"+", 1}, {"*", 0}
            }, new HashSet<string> {"A", "B", "C"}, new Dictionary<string, (int, Func<object[], dynamic>)>()
            {
                {"*", (2, (objs) => (int)objs[0] * (int)objs[1])},
                {"+", (2, (objs) => (int)objs[0] + (int)objs[1])},
            });
            
            var res = SimpleCalculation.CalcWithTree(c, new Dictionary<string, object>
            {
                {"A", 5}, {"B", 4}, {"C", 3}
            });
            $"{res}".PrintToConsole();
        }
        public static dynamic CalcWithTree(CalcTreeNode node, Dictionary<string, object> values)
        {
            if (node.IsLeaf)
                return values[node.Data.ToString() ?? string.Empty];
            var l = node.Children.Select(e => CalcWithTree(e, values));
            var opFunc = (Func<object[], dynamic>)((ValueTupleSlim)node.Data)[2];
            return opFunc.Invoke(l.ToArray());
        }
        //(int, string, Func<object[], dynamic>) <== valuetupleslim
         public static CalcTreeNode GetCalcTree<T>(IEnumerable<string> inputSeq, Dictionary<string, int> priority, HashSet<string> variables,
             Dictionary<string, (int,  Func<object[], dynamic>)> opFunc) 
         {
            var s = new Stack<string>();
            var backExp = new List<string>();
            foreach (var e in inputSeq)
            {
                $"cur opStack ={s.GetMultiDimensionString()}, backExp = {backExp.GetMultiDimensionString()}".PrintToConsole();
                $"cur input = {e}".PrintToConsole();
                switch (e)
                {
                    case "(":
                        s.Push("(");
                        break;
                    case ")":
                    {
                        while (s.Any() && s.Peek() != "(")
                        {
                            backExp.Add(s.Pop());
                        }

                        if (s.Peek() == "(")
                            s.Pop();
                        break;
                    }
                    default:
                    {
                        if (variables.Contains(e)) //操作数
                        {
                            $"{e} is a operand".PrintToConsole();
                            backExp.Add(e);
                        }else if (priority.Keys.Contains(e)) //操作符
                        {
                            $"{e} is a operator".PrintToConsole();
                            var p1 = priority[e];
                            
                            while (s.Any())
                            {
                                var p2 = priority[s.Peek()];
                                if (p1 < p2)
                                {
                                    s.Push(e);
                                    break;
                                }
                                backExp.Add(s.Pop());
                            }
                            if(!s.Any())
                                s.Push(e);
                        }

                        break;
                    }
                }
            }
            while(s.Any())
                backExp.Add(s.Pop());
            $"final >>> opStack ={s.GetMultiDimensionString()}, backExp = {backExp.GetMultiDimensionString()} \n".PrintToConsole();
            var operandStack = new Stack<CalcTreeNode>();
            
            //generation
            $"generate parse Tree".PrintToConsole();
            var len = backExp.Count;
            for (var i = 0; i < len; )
            {
                var cur = backExp[i];
                $"cur = {cur}".PrintToConsole();
                if (priority.ContainsKey(cur)) //如果是运算符的话
                {
                    
                    var opParamCount = (int)opFunc[cur].Item1;
                    $"{cur} is op with {opParamCount} params".PrintToConsole();
                    var paramFeed = new List<CalcTreeNode>();
                    for (var j = 0; j < opParamCount; j++)
                    {
                        var p = operandStack.Pop();
                        $"op children add {p.Data}".PrintToConsole();
                        paramFeed.Add(p);
                    }
                    var resNode = new CalcTreeNode();
                    resNode.Children.AddRange(paramFeed);
                    resNode.Data = new ValueTupleSlim(opFunc[cur].Item1, cur, opFunc[cur].Item2);
                    
                    operandStack.Push(resNode);
                    $"now stack = {operandStack.GetMultiDimensionString()}".PrintToConsole();
                    i ++;
                }
                else
                {
                    $"{cur} is operand".PrintToConsole();
                    var leafNode = new CalcTreeNode {Data = cur};
                    operandStack.Push(leafNode);
                    i++;
                }
            }
            return operandStack.Peek();
        }
        //输入序列，优先级定义，取值，运算符计算委托，反序列化委托
        public static T Calc<T>(IEnumerable<string> inputSeq, 
            Dictionary<string, int> priority, Dictionary<string, int> values, Dictionary<string, (int, Func<object[], T>)> opFunc,
            Func<string, T> converter)
        {
            var s = new Stack<string>();
            var backExp = new List<string>();
            foreach (var e in inputSeq)
            {
                switch (e)
                {
                    case "(":
                        s.Push("(");
                        break;
                    case ")":
                    {
                        while (s.Any() && s.Peek() != "(")
                        {
                            backExp.Add(s.Pop());
                        }

                        if (s.Peek() == "(")
                            s.Pop();
                        break;
                    }
                    default:
                    {
                        if (values.ContainsKey(e)) //操作数
                        {
                            s.Add(e);
                        }else if (priority.Keys.Contains(e)) //操作符
                        {
                            var p1 = priority[e];
                            while (s.Any())
                            {
                                var p2 = priority[s.Peek()];
                                if (p1 < p2)
                                {
                                    s.Push(e);
                                    break;
                                }
                                backExp.Add(s.Pop());
                            }
                            if(!s.Any())
                                s.Push(e);
                        }

                        break;
                    }
                }
            }
            while(s.Any())
                backExp.Add(s.Pop());

            var operandStack = new Stack<T>();
            //calc
            for (var i = 0; i < backExp.Count;)
            {
                var cur = backExp[i];
                if (priority.ContainsKey(cur))
                {
                    var opParamCount = opFunc[cur].Item1;
                    var paramFeed = new List<object>();
                    for(var j = 0; j < opParamCount; j++)
                        paramFeed.Add(operandStack.Pop());
                    var res = opFunc[cur].Item2.Invoke(paramFeed.ToArray());
                    operandStack.Push(res);
                    i += 1 + opParamCount;
                }
                else
                {
                    operandStack.Push(converter.Invoke(cur));
                    i++;
                }
            }
            return operandStack.Peek();
        }
    }
}