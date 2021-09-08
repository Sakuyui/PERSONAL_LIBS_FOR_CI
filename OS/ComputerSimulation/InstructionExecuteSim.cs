using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIExam.os.Cache;
using CIExam.FunctionExtension;

namespace CIExam.OS
{
    //类似编程语言的
    public class InstructionExecuteSim
    {
        private static class PipelineStageStrategies
        {
            public static PipelineStage.PipelineStageProcessStrategy FetchStrategy = objects =>
            {
                //fetch阶段，不管怎样都是返回一个指令。主要的工作就是1. 返回一个指令，2.更新计算机上下文
                var context = (ComputerContext)objects[0];
                var nextStage = (PipelineStage)objects[1];
                //mux branch or previous pc + 1
                var pc = context.IsBranchTarget >= 0 ? context.IsBranchTarget : context.NextPc;
                context.NextPc = pc + 1; //这里按照要求，不管怎样pc都是预先 + 1的。
                
                Instruction GetNextInstruction()
                {
                    if (context.SignalIfClear == 1)
                    {
                        context.SignalIdClear = 0; //清除信号线
                        return new Instruction(Instruction.InstructionType.Nop); //nop指令
                    }
                    
                    throw new NotImplementedException();
                }
               
                var next = GetNextInstruction();
                context.DecodeStage.InstructionInUse = next;
                return next;
            };
            public static PipelineStage.PipelineStageProcessStrategy DecodeStrategy = objects =>
            {
                var context = (ComputerContext)objects[0];
                var nextStage = (PipelineStage)objects[1];
                var curInstruction = context.DecodeStage.InstructionInUse;
                var nextInstruction = (Instruction)objects[2];
                
                //主要进行解码以及寄存器读取。
                if (curInstruction == null || curInstruction.Type == Instruction.InstructionType.Nop)
                    return curInstruction;
                
                //hazard check
                //只有两种可能的冒险 ex-mem mem-ex
                var mem = context.MemStage.InstructionInUse;
                if (mem.Type == Instruction.InstructionType.LoadUse)
                {
                    //这里还缺少判断
                    //必须nop，无法通过forwarding解决
                    context.SignalIfClear = 1;
                    return null;
                }
                
                //register read
                //这里就是根据指令类型进行指令数据的操作惹。先不管
                //curInstruction.Operand[0] = ...
                context.DecodeStage.InstructionInUse = nextInstruction;
                return curInstruction;
            };
            public static PipelineStage.PipelineStageProcessStrategy ExStrategy = objects =>
            {
                var context = (ComputerContext)objects[0];
                var nextStage = (PipelineStage)objects[1];
                var curInstruction = context.ExStage.InstructionInUse;
                var nextInstruction = (Instruction)objects[2];
                //exe
                if (curInstruction == null || curInstruction.Type == Instruction.InstructionType.Nop)
                {
                    return curInstruction;
                }
                if (curInstruction.Type == Instruction.InstructionType.Branch)
                {
                    //如果执行结果是分支的话，那么
                    //context.IsBranchTarget = address;
                }
                else
                {
                    //exe
                }
                
                //考虑forwarding
                //nextInstruction.Operand[0] = xxx //转发,注意此时这条命令还在decode阶段
                
                context.ExStage.InstructionInUse = nextInstruction; //接受前一个stage传来的命令。下个时钟执行
                return curInstruction;
            };
            public static PipelineStage.PipelineStageProcessStrategy MemStrategy = objects =>
            {
                var context = (ComputerContext)objects[0];
                var nextStage = (PipelineStage)objects[1];
                var curInstruction = context.MemStage.InstructionInUse;
                var nextInstruction = (Instruction)objects[2];

                //exe
               
                if (curInstruction == null || curInstruction.Type == Instruction.InstructionType.Nop)
                    return curInstruction;
                if (curInstruction.Type == Instruction.InstructionType.LoadUse)
                {
                    //主要是从ALU或者指令中获取访存位置数据。然后读出数据，写入上下文。
                }
                context.MemStage.InstructionInUse = nextInstruction; //接受前一个stage传来的命令。下个时钟执行
                
                
                
                return curInstruction;
            };
            public static PipelineStage.PipelineStageProcessStrategy WbStrategy = objects =>
            {
                var context = (ComputerContext)objects[0];
                var curInstruction = context.WbStage.InstructionInUse;
                var nextInstruction = (Instruction)objects[1];
                //其实就是写上下文的寄存器惹。没什么特别要注意的
                if (curInstruction == null || curInstruction.Type == Instruction.InstructionType.Nop)
                    return curInstruction;
                
                context.WbStage.InstructionInUse = nextInstruction; //接受前一个stage传来的命令。下个时钟执行
                return null;
            };
        }
        
        private class PipelineStage
        {
            public Instruction InstructionInUse = null;
            //应该提供一些该阶段需要的东西，比如寄存器之类的
            public delegate object PipelineStageProcessStrategy(params object[] objects);

            private readonly PipelineStageProcessStrategy _strategy;
            public PipelineStage(PipelineStageProcessStrategy strategy)
            {
                _strategy = strategy;
            }

            public object DoWork(params object[] objects)
            {
                return _strategy.Invoke(objects);
            }
        }

        private class Instruction
        {
            internal enum InstructionType
            {
                Nop,
                Calc,
                Branch,
                LoadUse
            }
            public object[] Operand = new object[3];
            public InstructionType Type;
            public Instruction(InstructionType type)
            {
                Type = type;
            }
        }

        private class ComputerContext
        {
            public byte SignalIdClear = 0;
            public byte SignalIfClear = 0;
            public int IsBranchTarget = -1;
            public int PcRegister = 0;
            public int NextPc = 0;
            public int[] RegisterFiles = new int[16];
            //实际上指令和数据应该是在同一个内存的。为了省略去解码的操作，这里分开惹
            public Dictionary<int, Instruction> InstructionsMemory = new ();
            public Dictionary<int, Instruction> DataMemory = new ();
            public AbstractCache<int, int> DataCache = CacheBuilder.BuildLruCommonCache<int, int>(128, (key, objects) => 
                ((Dictionary<int, int>)objects[0])[key]);
            public AbstractCache<int, Instruction> InstructionCache = 
                CacheBuilder.BuildLruCommonCache<int, Instruction>(128, (key, objects) => 
                    ((Dictionary<int, Instruction>) objects[0])[key]);
            public readonly PipelineStage FetchStage = new PipelineStage(PipelineStageStrategies.FetchStrategy);
            public readonly PipelineStage DecodeStage = new PipelineStage(PipelineStageStrategies.DecodeStrategy);
            public readonly PipelineStage ExStage = new PipelineStage(PipelineStageStrategies.ExStrategy);
            public readonly PipelineStage MemStage = new PipelineStage(PipelineStageStrategies.MemStrategy);
            public readonly PipelineStage WbStage = new PipelineStage(PipelineStageStrategies.WbStrategy);
        }
        private class Pipeline
        {
           
            private int _clock = 0;
            private ComputerContext _context = new ComputerContext();
            public void Init()
            {
                _clock = 0;
            }

            public void ToPipelineNextClock()
            {
                var fetchResult = _context.FetchStage.DoWork(_context, _context.DecodeStage); //通过fetch获取下一个阶段指令
                var decodeResult = _context.DecodeStage.DoWork(_context, _context.ExStage, fetchResult); //decode应该返回寄存器的读取结果，并且判断冒险之类的，还要考虑forwarding
                var exResult = _context.DecodeStage.DoWork(_context, _context.MemStage, decodeResult); //
                var memResult = _context.MemStage.DoWork(_context, _context.WbStage, exResult); //
                var wbResult = _context.WbStage.DoWork(_context, memResult); //

                _clock++;
            }

           
        }
        private class InstructionPipelineSimulation
        {
            private Pipeline _pipeline;

            public InstructionPipelineSimulation()
            {
                _pipeline = new Pipeline();
            }
        }
        
        
        
        
        
        
        public static void Test()
        {
            var dict = new Dictionary<char, bool>();
            dict['A'] = false;
            dict['B'] = true;
            dict['C'] = true;
            LogicalCalc("B(A+BC+!(A+(B+C)B))(A+B)+C(A+B)", dict);
            "".PrintToConsole();
        }
  
        public static bool LogicalCalc(string desc, Dictionary<char, bool> dict, int layer = 0)
        {
            ("layer ===== " + layer).PrintToConsole();
            if (desc.Length == 1)
            {
                (desc + " = " + dict[desc[0]]).PrintToConsole();
                return dict[desc[0]];
            }
               
            var andItem = new List<string>();
            var cur = "";
            var set = dict.Keys.ToHashSet();
            var s = new Stack<char>();
            foreach (var input in desc)
            {
                if (set.Contains(input))
                {
                    //字符
                    cur += input;
                    continue;
                }

                switch (input)
                {
                    case '+' when !s.Any():
                        andItem.Add(cur);
                        cur = "";
                        break;
                    case '+':
                        cur += '+';
                        break;
                    case '(':
                        s.Push('(');
                        cur += '(';
                        break;
                    case ')':
                        s.Pop();
                        cur += ')';
                        break;
                }
            }

            if (cur != "")
            {
                andItem.Add(cur);
            }
            //大或门结束
            andItem.PrintCollectionToConsole();

            var addResult = false; 
            foreach (var item in andItem)
            {
                var mCur = "";
                var tmpStack = new Stack<char>();
                var mulList = new List<string>();
                foreach (var input in item)
                {
                    if (set.Contains(input))
                    {
                        mCur += input;
                        if (!s.Any())
                        {
                            mulList.Add(mCur);
                            mCur = "";
                        }
                        
                    }else switch (input)
                    {
                        case '(':
                        {
                            if (s.Any())
                            {
                                mCur += '(';
                            }
                            s.Push('(');
                            break;
                        }
                        case ')':
                        {
                            s.Pop();
                            if (s.Any())
                            {
                                mCur += ')';
                            }
                            else
                            {
                                if (mCur != "")
                                {
                                    mulList.Add(mCur);
                                    mCur = "";
                                }
                            }

                            break;
                        }
                        case '+':
                            mCur += "+";
                            break;
                    }
                }

                if (mCur != "")
                {
                    mulList.Add(mCur);
                }
               

                var mulResult = true;
                //计算乘积
                foreach (var t in mulList)
                {
                    var r = LogicalCalc(t, dict, layer + 1);
                    
                    mulResult &= r;
                    if(mulResult == false)
                        break;
                }

                addResult |= mulResult;
                if (addResult)
                {
                    (desc + " = " + true).PrintToConsole();
                    return true;
                }
                    
            }
            
            (desc + " = " + false).PrintToConsole();
            return false;
        }

        /*input : 指令| & !  参数列表(,,,)返回*/
        public bool DecodeBoolExpression(string boolExp)
        {
            boolExp.PrintToConsole();
            //状态机
            var state = 1;
            //register
            bool? ra = null;
            var paramList = new List<bool>();
            char? method = null;
            
            //stack
            var methodStack = new Stack<char>();
            var paramsStack = new Stack<List<bool>>();
            
            //输入指针。。模拟图灵机惹
            var pointer = 0;
            
            //other
            var methodSet = new char[] {'|', '&', '!'}.ToHashSet();
            while (pointer < boolExp.Length)
            {
                var input = boolExp[pointer];
                switch (state)
                {
                    case 1:
                        if (input == 'f')
                        {
                            return false;
                        }else if (input == 't')
                        {
                            return true;
                        }else if (methodSet.Contains(input))
                        {
                            //函数调用
                            state = 2;
                            ("get innovation " + boolExp[pointer]).PrintToConsole();
                            method = input;
                            
                            pointer++;
                        }
                        break;
                    case 2:
                        if (ra == null)
                        {
                            "ra = null, not need recover".PrintToConsole();
                        }
                        else
                        {
                            "recover register".PrintToConsole();
                            paramList = paramsStack.Pop();
                            paramList.Add((bool)ra);
                            ra = null;
                            method = methodStack.Pop();
                            paramList.PrintCollectionToConsole();
                           
                            state = 3;
                            continue;
                        }
                        if (input == '(')
                        {
                            "reg params begin...".PrintToConsole();
                            pointer++;
                            state = 3;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case 3:
                        if (input == ',')
                        {
                            "next param".PrintToConsole();
                            pointer++;
                        }else if(input == 'f' || input == 't')
                        {
                            paramList.Add(input != 'f');
                            ("put param " + input).PrintToConsole();
                            pointer++;
                        }else if(methodSet.Contains(input))
                        {
                            ("method innovation " + input).PrintToConsole();
                            if (method != null)
                            {
                                ("save cur method type = " + method).PrintToConsole();
                                methodStack.Push((char)method);
                            }
                            
                            method = input;

                            pointer++;
                            //save context
                            paramsStack.Push(paramList);
                            paramList = new List<bool>();
                            state = 2;
                        }else if (input == ')')
                        {
                            ("meet ) 帰着 method = " + method).PrintToConsole();
                            pointer++;
                            //计算
                            var t = paramList[0];
                            if (method == '&')
                            {
                               
                                for (var i = 1; i < paramList.Count; i++)
                                {
                                    t &= paramList[i];
                                }
                            }else if(method == '|')
                            {
                                for (var i = 1; i < paramList.Count; i++)
                                {
                                    t |= paramList[i];
                                }
                            }else if (method == '!')
                            {
                                t = !t;
                            }

                            ra = t;
                            ("归着结果 = " + ra + "\n").PrintToConsole();
                            state = 2;
                        }
                        break;
                }
            }
            
            return ra != null && (bool) ra;
        }
        public string DecodeString(string s) {
            var state = 1;
            //寄存器区域
            var numRegister = "";
            var strRegister = "";
            var ra = "";
            
            //堆栈，调用子程序后保留活动记录
            var numSp = new Stack<int>();
            var strSp = new Stack<string>();

            //输入指针。图灵机惹。可以灵活控制
            var pointer = 0;
            while(pointer < s.Length)
            {
                switch (state)
                {
                    //0: 识别指令参数状态
                    case 0 when IsDigit(s[pointer]):
                        numRegister += s[pointer] - '0';
                        pointer++;
                        break;
                    case 0:
                    {
                        if(s[pointer] == '['){
                            ("subroutine entre " + numRegister).PrintToConsole();
                            numSp.Push(int.Parse(numRegister));
                            state = 1;
                            numRegister = "";
                            pointer++;
                        }

                        break;
                    }
                    case 1: //1 : 顺序执行状态
                    {
                       
                        //记录字符
                        if(s[pointer] != ']'){
                            if(IsDigit(s[pointer])){
                                //子程序
                                state = 0;
                                "meet number..subroutine".PrintToConsole();
                                ("save register " + strRegister).PrintToConsole();
                                strSp.Push(strRegister); //保存寄存器
                                strRegister = ""; //清空寄存器
                            }else{
                                strRegister += s[pointer];
                                pointer++;
                            }
                        }else{
                      
                            var repeatCount = numSp.Pop();
                            ra = RepeatString(strRegister, repeatCount);
                            ("Subroutine return " + ra).PrintToConsole();
                            state = 2;
                            pointer++;
                        }
                       
                        break;
                    }
                    case 2: //空转移，寄存器恢复
                        if(ra != ""){
                            "recover context...".PrintToConsole();
                            "get return value ".PrintToConsole();
                            var pre = strSp.Pop();
                            strRegister = pre + ra;
                            ("now = " + strRegister).PrintToConsole();
                            ra = "";
                            state = 1;
                        }
                        break;
                }
            }

            if (strSp.Any())
            {
                strRegister = strSp.Pop();
            }
            (strRegister + ra).PrintToConsole();
            return strRegister + ra;
        }

        public static bool IsDigit(char c){
            return c - '0' >= 0 && c - '0' <= 9;
        }

        public static string RepeatString(string s, int c){
            return Enumerable.Repeat(s, c).Aggregate("", (a, b) => a + b);
        }


    }
}