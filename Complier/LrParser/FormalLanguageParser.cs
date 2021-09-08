using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CIExam.FunctionExtension;

namespace CIExam.Complier.LrParser
{
    public class GrammarParser
    {
        private readonly ProducerDefinition Definitions;

        public bool CykJudge(string input)
        {
            var n = input.Length;
            var cykTable = new List<List<string>>();
            var ts = input.Select(e => e + "").ToList();

            string GetProductLeftSymbol(string t)
            {
                var s = new HashSet<string>();
                foreach (var g in Definitions.Grammars.Where(g => g.ProduceItem == t))
                {
                    s.Add(g.LeftSymbol);
                }
                return s.Any() ? s.Aggregate((a, b) => a + "|" + b) : "";
            }

            var initList = ts.Select(GetProductLeftSymbol).ToList();
            initList.PrintEnumerationToConsole();
            cykTable.Add(initList);
            
            var times = n - 1;
            for (var i = 0; i < times; i++)
            {
                var last = cykTable[^1];
                var curLayer = new List<string>();
                $"step : {i}, last layer = {last.GetMultiDimensionString()}".PrintToConsole();
                for (var j = 0; j < n - 1 - i; j++)
                {
                    //填充当前层的第J项。但是没这么容易
                    var join = new List<string>();
                    for (var k = 1; k <= i + 1; k++)
                    {
                        //第i层代表符号长i + 1。 所以符号长k 的 和符号长 i + 2 - k的连接
                        var joinLeft = cykTable[k - 1][j]; //符号长为k的层，从当前项开始能够解析的符号
                        var joinRight = cykTable[i + 1 - k][j + k]; //符号长为k的层，从当前项过去k个字符后开始能够解析的符号
                        $"join {joinLeft} with {joinRight}".PrintToConsole();
                        var tmpJoin = (from x in joinLeft.Split("|")
                            join y in joinRight.Split("|")
                                on 1 equals 1
                            select x + y);
                        join.AddRange(tmpJoin);
                    }
                    $"join in layer {i + 2},{j} = {join.GetMultiDimensionString()}".PrintToConsole();
                  
                    join.PrintEnumerationToConsole();
                    
                    var item = join.Select(GetProductLeftSymbol).Where(e => e != "")
                        .Select(e => e.Split("|")).SelectMany(e => e).Distinct().OrderBy(c => c);
                    $"items = {item.GetMultiDimensionString()}".PrintToConsole();
                    curLayer.Add(item.Any() ? item.Aggregate((a, b) => a + "|" + b) : "");
                    "".PrintToConsole();
                }
                cykTable.Add(curLayer);
            }
            $"final layer = {cykTable[^1].GetMultiDimensionString()}".PrintToConsole();
            return cykTable[^1].SelectMany(e => e.Split("|")).Contains(Definitions.StartWord);
        }
        public GrammarParser(ProducerDefinition definitions)
        {
            Definitions = definitions;
        }

        private List<SingleTokenParseStrategy> _tokenParseStrategy;

        public GrammarParser SetTokenParseStrategy(IEnumerable<SingleTokenParseStrategy> strategy)
        {
            _tokenParseStrategy = strategy.ToList();
            
            return this;
        }

        //首先分割
       

        //private Dictionary<string, List<List<string>>> _produceMapping;
        public delegate object ReductionStrategy(params object[] parameters);

       
        public void Parse(Dictionary<string, ReductionStrategy> reductionStrategies = null)
        {
            reductionStrategies ??= new Dictionary<string, ReductionStrategy>();
            if(InputList.Count == 0)
                return;
            Definitions.AddGrammar(new ProducerDefinitionItem("<S'>->"+Definitions.StartWord));
            Definitions.InitProduceMapping();
            
            "==========================================P SET===================================".PrintToConsole();
            foreach (var s in Definitions.ProduceMapping)
            {
                ("[" + s.Key +"] => " + s.Value.Select(e => e.ToEnumerationString()).ToEnumerationString()).PrintToConsole();
            }

            "==========================================FIRST SET===================================".PrintToConsole();
            var dict = new Dictionary<int, ProjectSet>();
            
            var firstSet = CfgTools.GetFirstSet( Definitions);
            
            "============================================I(0)=======================================".PrintToConsole();
            var curPSet = new ProjectSet(new []{new Lr1Item("<S'>", 
                new List<string>(new []{Definitions.StartWord}),
                new List<string>(new []{"$"}))});
            curPSet.PrintToConsole();
            curPSet.ApplyClosure(Definitions);
            curPSet.GetProjectItemsDesc().PrintToConsole();
            
            var projects = new Dictionary<int, ProjectSet> {{0,curPSet}};
            var changed = true;
            var memo = new HashSet<ProjectSet>();
            var lr1Table = new Lr1Table(Definitions);
            lr1Table.AddRow();
            var products = Definitions.Grammars.Select((e, i)=> (i, e))
                .ToDictionary(k => k.i, v => v.e);
            
            
            while (changed)
            {
                changed = false;
                var curProjects = new Dictionary<int, ProjectSet>(projects);
                var count = curProjects.Count;
                foreach (var p in curProjects.Where(p => !memo.Contains(p.Value)))
                {
                    MoveProject(p, projects, lr1Table, products);
                    memo.Add(p.Value);
                }

                if (projects.Count != count)
                    changed = true;
            }
            
            "==============================Project sets===================".PrintToConsole();
            projects.Count.PrintToConsole();
            projects.ElementInvoke(delegate(KeyValuePair<int, ProjectSet> pair)
            {
                var ps = pair.Value.GetProjectItemsDesc();
                $"I({pair.Key}): {ps}".PrintToConsole();
            });

            
            
            "==============================Project sets===================".PrintToConsole();

            lr1Table.PrintToConsole();
            
            "============================== Begin Parse ===================".PrintToConsole();
            
            var codeStack = new Stack<string>();
            codeStack.Push("$");
            InputList.PrintCollectionToConsole();
            var stateStack = new Stack<int>();
            stateStack.Push(0);
            var input = InputList.ToList();
            input.Add(new Token("$", TokenType.T,"$"));
            var times = 90;
            while (times > 0)
            {
                "".PrintToConsole();
                
                ("code peek = " + input.First().TokenName + " , state = " + stateStack.Peek()).PrintToConsole();
                var t = lr1Table.Transition[stateStack.Peek()][input.First().TokenName] + "";
                if (t == "ACC")
                {
                    "================== ACC!!! =================".PrintToConsole();
                    return;
                }
                if (t[0] == 's')
                {
                    //shift
                    var nextState = int.Parse(t[1..]);
                    $"shift to {nextState}".PrintToConsole();
                    stateStack.Push(nextState);
                    codeStack.Push(input.First().TokenName);
                    input.RemoveAt(0);
                    $"code Stack = {codeStack.GetMultiDimensionString()}".PrintToConsole();
                    $"state Stack = {stateStack.GetMultiDimensionString()}".PrintToConsole();
                }else if (t[0] == 'r')
                {
                    //reduction 规约后一定对应一个新状态
                    var grammarCode = int.Parse(t[1..]);
                    $"reduction by {grammarCode}".PrintToConsole();
                    Definitions.Grammars[grammarCode].PrintToConsole();
                    
                    var g = Definitions.Grammars[grammarCode];
                    
                    var sb = new StringBuilder(g.ProduceItem);
                    var list = new List<string>();
                    //处理单条产生式
                    var kSet = Definitions.Terminations.Union(Definitions.NonTerminationWords).ToHashSet(null);
                    while (sb.Length > 0)
                    {
                        var found = kSet.Where(s => sb.ToString()
                                .IndexOf(s, StringComparison.Ordinal) == 0).ArgMax(e => e.Length).Item2;
                        list.Add(found);
                        sb.Remove(0, found.Length);
                    }
                    list.PrintEnumerationToConsole();
                
                    foreach(var s in list)
                    {
                        codeStack.Pop();
                        stateStack.Pop();
                    }
                    codeStack.Push(g.LeftSymbol);
                    var gotoNext = lr1Table.Goto[stateStack.Peek()][codeStack.Peek()].ToString();
                    
                    $"goto => {stateStack.Peek()}".PrintToConsole();
                    stateStack.Push(int.Parse(gotoNext));


                    
                    $"code Stack = {codeStack.GetMultiDimensionString()}".PrintToConsole();
                    $"state Stack = {stateStack.GetMultiDimensionString()}".PrintToConsole();
                }

                times--;
            }
        }

   
        public readonly List<Token> InputList = new();
        public GrammarParser ParseForTokens(string input)
        {
            ("input: " + input).PrintToConsole();
            "===========================================Parse Tokens==================================".PrintToConsole();
            if (_tokenParseStrategy == null)
                return null;
            var sb = new StringBuilder(input);
            InputList.Clear();
            
            while (sb.Length > 0)
            {
                Regex targetR = null;
                //sb.PrintToConsole();
                foreach (var t in _tokenParseStrategy)
                {
                    if (!t.Regex.IsMatch(sb.ToString())) continue;
                    var mc = t.Regex.Matches(sb.ToString(), 0);
                    if (mc.All(c => c.Index != 0)) continue;
                    var m = mc.Where(e => e.Index == 0)
                        .OrderByDescending(e => e.Length)
                        .First();
                        
                    targetR = t.Regex;
                    //("[" + t.TokenName + "] match at " + m.Index + " [" + m.Value+"]").PrintToConsole();
                    var parseName = t.TokenName;
                    sb.Remove(0, m.Value.Length);
                    var parsedToken = new Token(parseName, TokenType.T, m.Value);
                    InputList.Add(parsedToken);
                    t.Action?.Invoke(parsedToken);
                    break;
                }
                if(targetR == null)
                    throw new ArgumentException("parse Error: unexpected token");
                
            }
            InputList.Select(e => e.TokenName).PrintCollectionToConsole();
            
            return this;
        }

       
        public void MoveProject(KeyValuePair<int, ProjectSet> projectSet, Dictionary<int, ProjectSet> result, Lr1Table table, 
            Dictionary<int, ProducerDefinitionItem> definitionItems)
        {
           
            var cSet = projectSet.Value.Where(e => !e.IsReductionItem())
                .Select(e => e.ProduceItems[e.DotPos]).ToHashSet(null);
           
            if(!cSet.Any())
                return;
            var curId = projectSet.Key;
            
            $">> From I({curId}) Move".PrintToConsole();
           
            //projectSet.Value.PrintToConsole();
            cSet.PrintEnumerationToConsole();

            //解决规约填表
            if (projectSet.Value.Any(p => p.IsReductionItem()))
            {
                var reduction= projectSet.Value.Where(p => p.IsReductionItem()).ToList();
                foreach (var r in reduction)
                {
                    var forwardSearch = r.SearchWordList;
                    
                    foreach (var f in forwardSearch)
                    {
                        if (f == "$" && r.StartWord == "<S'>")
                        {
                            table.Transition[curId][f] = "ACC";
                        }
                        else
                        {
                            $"cur = {r}".PrintToConsole();
                            table.Transition[curId][f] = "r" + definitionItems.First(d =>
                            {
                                var p = d.Value;
                                if (r.StartWord != p.LeftSymbol)
                                    return false;
                                //$"true with {p}".PrintToConsole();
                                //$"now {p.ProduceItem.GetMultiDimensionString()}, {r.ProduceItems[0].GetMultiDimensionString()}".PrintToConsole();
                                return p.ProduceItem == r.ProduceItems.Aggregate("",(a, b) => a + b);
                            }).Key;
                        }
                      
                    }
                    
                  
                }
            }
            
            //move
            var pSet = projectSet.Value.Where(p => !p.IsReductionItem()).ToList();

            foreach (var c in cSet)
            {
                //$"input - {c}".PrintToConsole();
                var items = pSet.Where(item => item.ProduceItems[item.DotPos] == c)
                    .Select(item => item.MoveForward()).ToList();
                
                
                var ps = new ProjectSet(items);
                
                ps.ApplyClosure(Definitions);
               
                //判重
                var newCode = result.Count;
                if (!result.Values.Contains(ps))
                {
                   
                    result.Add(newCode, ps);
                    table.AddRow();
                }
                else
                {
                    newCode = result.First(e => e.Value.Equals(ps)).Key;
                    //code.PrintToConsole();
                }
                
                //处理移进和Goto
                if (Definitions.NonTerminationWords.Contains(c))
                {
                    table.Goto[curId][c] = newCode;
                }else if (Definitions.Terminations.Contains(c))
                {
                    table.Transition[curId][c] = "s" + newCode;
                }
                
                //存在规约项目，那么要填表
                if (ps.Any(p => p.IsReductionItem()))
                {
                    //需要规约的项目
                    var reduction= ps.Where(p => p.IsReductionItem()).ToList();
                    
                    //对于项目集内每个项
                    foreach (var r in reduction)
                    {
                        //$"reduction {r}".PrintToConsole();
                        var forwardSearch = r.SearchWordList;
                        foreach (var f in forwardSearch)
                        {
                            
                            //$"{f}".PrintToConsole();
                            var k = definitionItems.First(d =>
                            {
                                var p = d.Value;
                                if (r.StartWord != p.LeftSymbol)
                                    return false;
                                //$"a: {p.ProduceItem}, b:{r.ProduceItems.Aggregate("", (a, b) => a + b)}"
                                //    .PrintToConsole();
                                return p.ProduceItem.Equals(r.ProduceItems.Aggregate("", (a, b) => a + b));
                            }).Key;
                            
                            //"归约" + definitionItems[k] + " " + k + " " + f).PrintToConsole();
                            table.Transition[newCode][f] = r.StartWord == "<S'>" ? "ACC": "r" + k;
                        }
                    }
                }
            }
          
        }
    }

    public enum TokenType
    {
        T,
        N
    }
}