using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;
using CIExam.Math;
using CIExam.Structure.Graph;

namespace CIExam.InformationThoery
{
    
    public class HuffmanEnCoder
    {
        public static Dictionary<string, T> Encode<T>(IEnumerable<T> dataSource)
        {
            var enumerable = dataSource as T[] ?? dataSource.ToArray();
            var n = (double)enumerable.Length;
            var freq = 
                enumerable.GroupBy(e => e)
                    .Select(g => (g.Key, g.Count() / n))
                    .ToList();
            freq.PrintToConsole();
            var p =
                new PriorityQueue<double, BinaryTreeNode<(object data, double p, string code)>>();
            foreach (var (key, item2) in freq)
            {
                var graphNode = new BinaryTreeNode<(object data, double p, string code)>(
                    (key, item2, "")
                    );
                p.EnQueue(item2, graphNode);
            }

            foreach (var e in p)
            {
                (e.Data.p +" -> " + e.Data.data + (e.IsLeaf()? " leaf" : "")).PrintToConsole();
            }

            while (p.Count() > 1)
            {
                var first = p.DeQueue();
                var second = p.DeQueue();
                var binaryTreeNode = new BinaryTreeNode<(object data, double p, string code)>(
                    (null, first.priority + second.priority, "")
                    );
                binaryTreeNode.Left = first.item;
                binaryTreeNode.Right = second.item;
                p.EnQueue(binaryTreeNode.Data.p, binaryTreeNode);
                
            }

            var root = p.DeQueue();
            LabelTree(root.item);
            
            var s = root.item.PreorderEnumerator.
                Where(e => e.Data.data != null).
                Select(e => e.Data.data + " code = " + e.Data.code);
            foreach (var str in s)
            {
                str.PrintToConsole();
            }
         
            return root.item.PreorderEnumerator.Where(e => e.Data.data != null).ToDictionary(
                (k => k.Data.code), (v => (T)v.Data.data)
                );
        }

        public static List<T> Decode<T>(string input, Dictionary<string, T> dict)
        {
            if (dict == null || !IsRealtimeCode(dict))
                return null;
            var buffer = "";
            var list = new List<T>();
            foreach (var t in input)
            {
                if (dict.Keys.Contains(buffer))
                {
                    list.Add(dict[buffer]);
                    buffer = "";
                }

                buffer += t;
            }

            if (!buffer.Equals(""))
            {
                if (dict.Keys.Contains(buffer))
                {
                    list.Add(dict[buffer]); ;
                }
            }
            return list;
        }
        public static bool IsRealtimeCode<T>(Dictionary<string, T> dict)
        {
            if (dict.Keys.ToList().GroupBy(e => e).Select(e => e.Count()).Max() > 1)
                return false;
            var codes = dict.Keys.Select(e => e).ToList();
            var root = new BinaryTreeNode<(string code, bool isDataNode)>(("", false));
            foreach(var code in codes)
            {
                var curNode = root;
                var tmpCode = "";
                foreach (var c in code)
                {
                    if (c == '1')
                    {
                        if (curNode.Left != null)
                        {
                            curNode = curNode.Left;
                            tmpCode += "1";
                        }
                        else
                        {
                            tmpCode += "1";
                            curNode.Left = new BinaryTreeNode<(string code, bool isDataNode)>((tmpCode, 
                                    code.Equals(tmpCode)
                                    ));
                            curNode = curNode.Left;
                        }
                    }else
                    {
                        if (curNode.Right != null)
                        {
                            curNode = curNode.Right;
                            tmpCode += "0";
                        }
                        else
                        {
                            tmpCode += "0";
                            curNode.Right = new BinaryTreeNode<(string code, bool isDataNode)>((tmpCode, 
                                    code.Equals(tmpCode)
                                ));
                            curNode = curNode.Right;
                        }
                    }
                }

               
            }

          
            return  ! root.PreorderEnumerator.Any(e => e.Data.isDataNode && !e.IsLeaf());;
          
        }
        public static void LabelTree(BinaryTreeNode<(object data, double p, string code)> root, string curCode = "", bool isLeftLabelOne = true)
        {
            root.Data.code = curCode;
            if (root.Left != null)
            {
                LabelTree(root.Left, curCode + (isLeftLabelOne ? "1" : "0"));
            }

            if (root.Right != null)
            {
                LabelTree(root.Right, curCode + (isLeftLabelOne ? "0" : "1"));
            }
            
        }
    }
}