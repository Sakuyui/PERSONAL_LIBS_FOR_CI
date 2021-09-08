using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CIExam.FunctionExtension;

namespace CIExam.Complier
{
    public class Seiki
    {
        public bool IsUtf8(int[] data)
        {
            var n = 0;
            foreach (var t in data) {
                if(n > 0){
                    if(t >> 6 != 2)
                        return false;
                    n--;
                }else if(t >> 7 == 0){
                    n = 0;
                }else if(t >> 5 == 0b110){
                    n = 1;
                }else if(t >> 4 == 0b1110){
                    n = 2;
                }else if(t >> 3 == 0b11110){
                    n = 3;
                }else{
                    return false;
                }
            }
            return n == 0;
        }
        public bool IsUtf8(string input)
        {
            var bits = input.GroupByCount(8);
            if(bits.Any(e => e.Count != 8))
                return false;

            var i = 0;
            var regexes = new Dictionary<int, Regex>()
            {
                {0, new Regex("^0(0|1){7}$")}, {1, new Regex("^110(0|1){5}$")},
                {2, new Regex("^1110(0|1){4}$")}, {3, new Regex("^11110(0|1){3}$")}
            };
            var r = new Regex("10(0|1){6}");
            while (i < bits.Count)
            {
                var cur = bits[i].ConvertToString();
                if (!regexes.Any(e => e.Value.IsMatch(cur)))
                    return false;
                var (key, _) = regexes.First(r => r.Value.IsMatch(cur));
                if (i + key >= bits.Count)
                    return false;
                var processSets = bits.GetRange(i + 1, key).Select(e => e.ConvertToString());
                i += key;
                if (processSets.Any(e => !r.IsMatch(e)))
                    return false;
            }

            return true;
        }
        
        public IEnumerable<string> BraceExpansionII(string expression) {
            //自动机状态
            var state = 1;
            
            //相当于寄存器组，保存当前上下文
            List<string>? ra = null;  //返回值寄存器
            var curSeenStrings = new List<StringBuilder>(); //上下文，{到}间，可能的所有字符串集合
            var layerTotalStrings = new List<string>(); //上下文：上下文，{到}间，可能的所有字符串集合
            curSeenStrings.Add(new StringBuilder());
             
            //堆栈，用来存储递归时存储当前上下文。
            var layerTotalStringStack = new Stack<List<string>>(); //上下文，{到}间，可能的所有字符串集合
            var curSeenStringStack = new Stack<List<string>>(); //上下文：逗号到逗号间可能的所有字符串。
                
            //输入指针
            var pointer = 0;
        
            //辅助函数
            bool IsAlphabet(char c) => c >= 'a' && c <= 'z';

            while (pointer < expression.Length)
            {
                var input = expression[pointer]; //当前输入字符
                
                //自动机初始状态，相当于程序主函数。本题只需要一个状态。
                if (state == 1)
                {
                    //如果需要处理模拟递归调用返回的返回值
                    if (ra != null)
                    {
                        var lastLayerString = curSeenStringStack.Pop();
                        layerTotalStrings = layerTotalStringStack.Pop();
                       
                        /*现在需要 lastLayerString 连接 返回值寄存器ra中的每个值。
                         比如 "ab{c,d}" 递归进行{c,d}前，保存的上下文是 ["ab"]，递归返回的结构为["c", "d"]
                         那么这里就是希望连接成["abc","abd"]
                         */
                        var join = (from s1 in lastLayerString 
                            join s2 in ra on 1 equals 1 select s1 + s2).ToList();
                    
                        /*重设当前所看到的字符串集合。还是"ab{c,d}"的例子。ab之后遇到{进行递归，上下文curSeenStrings保存["ab"]
                         递归{c,d}结束后，已经返回，并获得了新的结果["abc","abd"]，因此进行更新。
                        */
                        curSeenStrings.Clear();
                        curSeenStrings.AddRange(join.Select(e => new StringBuilder(e)));
                        //如果指针到达末尾，跳出
                        if(pointer >= expression.Length - 1)
                            break;
                        pointer++;
                        ra = null;
                        continue;
                    }
                    //当前输入是字母的情况
                    if (IsAlphabet(input))
                    {
                        /*对当前可见字符集合，全部追加该字符。比如"ab{c,d}e"。在完成"ab{c,d}"后，可见字符集合已为["abc","abd"]
                         那么遇到e后，就均追加e，变为["abce","abde"]
                         */
                        foreach (var sBuilder in curSeenStrings)
                        {
                            sBuilder.Append(input);
                        }
                        pointer++;
                    }
                    //当前输入是逗号
                    else if (input == ',')
                    {
                        /*将逗号前已经获取到的字符集合保存。
                         比如"ab{c,d}e,abcd"，在遇到逗号后，把["abce"."abce"]保存，然后情况当前能够看到的字符串集合
                         */
                        layerTotalStrings.AddRange(curSeenStrings.Select(sBuilder => sBuilder.ToString()));
                        curSeenStrings.Clear();
                        curSeenStrings.Add(new StringBuilder());
                        pointer++;
                    }
                    //当前输入是}
                    else if (input == '}')
                    {
                        /*遇到}和上面的遇到逗号差不多，都要先把当前能看到的字符串集合保存，并清空。
                         不同的是。遇到}，代表某层递归的函数已经结束，需要把该函数能够获取到的字符串集合写入返回值。
                         */
                        layerTotalStrings.AddRange(curSeenStrings.Select(sBuilder => sBuilder.ToString()));
                        ra = new List<string>(layerTotalStrings); //写入返回值，结束当前层的函数执行，写入返回值寄存器
                        curSeenStrings.Clear();
                        curSeenStrings.Add(new StringBuilder());
                        //注意，这里不进行输入指针++，这样下一轮循环就能处理返回值。
                    }else if (input == '{')
                    {
                        /*
                         * 相当遇到新的函数调用
                         * 将上下文保存到堆栈，并且清空上下文，为新的"函数调用"准备。
                         */
                        curSeenStringStack.Push(curSeenStrings.Select(s => s.ToString()).ToList());
                        curSeenStrings.Clear();
                        curSeenStrings.Add(new StringBuilder());
                        layerTotalStringStack.Push(new List<string>(layerTotalStrings));
                        layerTotalStrings = new List<string>();
                        pointer++;
                    }
                }
            }

            //获取结果
            return curSeenStrings.Select(e => e.ToString()).Union(layerTotalStrings)
                     .Distinct().OrderBy(s => s);
        }
        
        
        //"3(1(2)(3))"
        public TreeNode StrToTree(string s) {
            var nodeStack = new Stack<(TreeNode node, int depth)>();
            var numStack = new Stack<int>();
            var kkStack = new Stack<int>();
           
            kkStack.Push(0);
            foreach(var c in s.Append(')')){
                if (c is >= '0' and <= '9')
                {
                    numStack.Push(c - '0');
                }else if (c == '(')
                {
                    kkStack.Push(!kkStack.Any() ? 1 : kkStack.Peek() + 1);
                }else if (c == ')')
                {
                    var newNode = (node: new TreeNode(numStack.Pop()),d: kkStack.Pop());
                    if (!nodeStack.Any() || nodeStack.Peek().depth == newNode.d)
                    {
                        //节点栈空，或者和栈顶节点深度相同的情况。直接入栈该节点
                        nodeStack.Push(newNode);
                        
                    }
                    else
                    {

                        if (nodeStack.Any() && nodeStack.Peek().depth < newNode.d )
                        {
                            nodeStack.Push(newNode);
                            continue;
                        }
                        var lrNode = new List<TreeNode> ();
                        if (nodeStack.Any() && nodeStack.Peek().depth == newNode.d + 1)
                        {
                            var leftNode = nodeStack.Pop().node;
                            lrNode.Add(leftNode);
                        }
                        //如果与栈顶节点深度不同。
                        if (nodeStack.Any() && nodeStack.Peek().depth == newNode.d + 1)
                        {
                            lrNode.Add(nodeStack.Pop().node);
                        }

                        newNode.node.left = lrNode.Count == 1? lrNode[0] : lrNode[1];
                        newNode.node.right = lrNode.Count == 1? null : lrNode[0];
                        nodeStack.Push(newNode);
                    }
                    
                }
                
            }

            return nodeStack.Peek().node;
        }
    }
}