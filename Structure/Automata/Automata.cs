using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CIExam.FunctionExtension;

namespace CIExam.Structure.Automata
{
    public class Automata
    {
        public readonly Dictionary<int, AutomataNode> AutomataNodesMapping = new();
        private AutomataContext _automataContext = new();

        public readonly Dictionary<int, Dictionary<int, AutomataTransitionEdge>> EdgeMapping = new();
        
        public int NodeCount => AutomataNodesMapping.Count;

        public Automata(Action<AutomataContext> initAutomata = null)
        {
            initAutomata?.Invoke(_automataContext);
        }
        public void AddNode(int nodeCode, AutomataNode automataNode)
        {
            AutomataNodesMapping.Add(nodeCode, automataNode);
        }

        public void AddEdge(AutomataTransitionEdge edge)
        {
            var f = edge.From;
            var t = edge.To;
            if (EdgeMapping.ContainsKey(f))
            {
                var tMap = EdgeMapping[f];
                tMap.Add(t, edge);
            }
            else
            {
                var l = new Dictionary<int, AutomataTransitionEdge>();
                l[t] = edge;
                EdgeMapping[f] = l;
            }
        }
        
        public object BeginParse(List<InputItem> items, Func<AutomataContext, HashSet<int>, bool> stopJudeFunc)
        {
            var status = AutomataNodesMapping.Where(e => e.Value.IsStart)
                .Select(e => e.Key).ToHashSet();

            while (!stopJudeFunc.Invoke(_automataContext, status) && _automataContext.InputPointer < items.Count)
            {
                var pointer = _automataContext.InputPointer;
                var curInput = items[pointer];
                ("cur input = " + curInput.Item + ",s = " + status.ToEnumerationString()).PrintToConsole();
                var nextStatus = new HashSet<int>();
                
                //遍历每个状态，如果是确定性图灵机的话，只存在一个 （目前只支持确定性图灵机）
                foreach (var edgeFrom in status)
                {
                    var trans = status;
                    
                    //遍历每个周围边，看是否可迁移
                    foreach (var edgeTo in 
                        from edgeTo in trans 
                        let flag = 
                            EdgeMapping[edgeFrom][edgeTo]
                            .EdgeStrategy
                            .Invoke(_automataContext, EdgeMapping[edgeFrom][edgeTo], curInput) 
                        where flag 
                        select edgeTo)
                    {
                        nextStatus.Add(edgeTo);
                        break; //目前只支持确定性图灵机、若在一条边上迁移后，不再考虑其他边迁移的可能。
                    }

                    if (_automataContext.BranchSignal != 0)
                    {
                        _automataContext.InputPointer ++; //无分支，指针继续向前
                    }
                    else
                    {
                        _automataContext.BranchSignal = 0; //存在分支，重设分支信号，下次输入听取当前指针位置
                    }
                }
                status.Clear();
                status.UnionWith(nextStatus);
                ("status to " + status.ToEnumerationString()).PrintToConsole();
                ("Input Pointer = " + _automataContext.InputPointer).PrintToConsole();
                if (!status.Any())
                {
                    //异常，对于输入没有可迁移至的状态。
                    //更新自动机上下文。设置异常信号
                    _automataContext.ExceptionSignal = 1;
                    _automataContext.KvMemory["_exception_msg"] = "Exception: no reachable node";
                }
            }

            return _automataContext;
        }

        public void ClearContext()
        {
            _automataContext.KvMemory.Clear();
            _automataContext.SimpleMemory.Clear();
            _automataContext.StackMemory.Clear();
        }
    }

    public delegate object AutomataNodeStrategy(AutomataContext context, int nodeSource);
    
    //进行边迁移，可以在边上进行相应操作。如果对于某个输入不可迁移，直接返回false。
    public delegate bool AutomataEdgeStrategy(AutomataContext context, AutomataTransitionEdge edgeSource, InputItem item);
    
    public class AutomataNode
    {
        public bool IsFinished;
        public AutomataNodeStrategy NodeStrategy;
        public bool IsStart;
        public AutomataNode( AutomataNodeStrategy automataNodeStrategy, bool isFinished = false, bool isStartNode = false)
        {
            IsFinished = isFinished;
            NodeStrategy = automataNodeStrategy;
            IsStart = isStartNode;
        }

       
    }
    //迁移边
    public class AutomataTransitionEdge
    {
        public int From;
        public int To;
        public AutomataEdgeStrategy EdgeStrategy; //迁移策略委托
        
        public AutomataTransitionEdge(int from, int to, AutomataEdgeStrategy edgeStrategy)
        {
            EdgeStrategy = edgeStrategy;
            From = from;
            To = to;
        }
    }

    //自动机上下文
    public class AutomataContext
    {
        public readonly Dictionary<string, object> KvMemory = new();
        public readonly Stack<object> StackMemory = new();
        public readonly List<object> SimpleMemory = new();
        public int InputPointer = 0; //可以更改输入指针，真正图灵完备。相当于可以更改PC寄存器
        
        //信号组
        public byte BranchSignal = 0; //分支信号
        public byte ExceptionSignal = 0; //异常信号
        
        //寄存器组
        public byte InterruptRegister = 0; //中断寄存器
        
    }
    
    //自动机输入类。任何输入必须封装在该类对象实例中
    public class InputItem
    {
        public readonly object Item;

        public InputItem(object item)
        {
            Item = item;
        }
    }
}