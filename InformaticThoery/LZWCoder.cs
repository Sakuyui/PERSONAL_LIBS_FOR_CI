using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.InformationThoery
{
    public class Lz77Coder
    {
        //TODO
        public Lz77Coder()
        {
        }

        public static List<T> DeCode<T>(IEnumerable<Lz77DataNode<T>> lz77DataNodes)
        {
            var ans = new List<T>();
            foreach (var node in lz77DataNodes)
            {
                if(!node.IsSeq)
                    ans.Add(node.ValData);
                else
                {
                    //解码
                    var seq = node.SeqData;
                    var pre = (int)seq[0];
                    var count = (int)seq[1];
                    var next = (int)seq[2];
                    ans.AddRange(ans.Skip(ans.Count - pre).Take(count));
                }
            }

            return ans;
        }
        
        
        public static List<Lz77DataNode<T>> EnCode<T>(IEnumerable<T> data, int wSize = 255, int forwardWinSize = 255)
        {
            //整体结构  |窗口内数据|  当前序列 | 前向缓冲|
            var dataCollection = data.ToList();
            var ans = new List<Lz77DataNode<T>>();
            var win = new Queue<T>(); //与LZW不同，LZ77是加窗的
            var wEnd = -1;
            
            var curPos = 0; //当前纸质
            while (curPos < dataCollection.Count)
            {
                //获取窗口
                var preSeq = dataCollection.Skip(wEnd + 1).Take(curPos - wEnd).ToArray();
                //窗口是否存在 当前得到的序列
                var index = win.FindSubSequence(preSeq);
                
                //前向缓冲已经满时，或者当前序列找不到时，都处理已遍历数据。注意前向缓冲满也能够直接加序列。因为说明至今都是连续匹配成功的
                if (index < 0 || curPos - wEnd == forwardWinSize)
                {
                    if (curPos - wEnd == 1 &&  curPos - wEnd != forwardWinSize)
                    {
                        ans.Add(new Lz77DataNode<T>(dataCollection[curPos]));
                        UpdateWindow(win, wSize, 1, new[] {dataCollection[curPos++]});
                        wEnd++;
                    }
                    else
                    {
                        //存在重复序列。加入元组，并移动窗口
                        //注意LZ77状态是3元组
                        ans.Add(new Lz77DataNode<T>(index, preSeq.Length, dataCollection[curPos]));
                        UpdateWindow(win, wSize, preSeq.Length, dataCollection.SubSequence(wEnd + 1, preSeq.Length).ToArray());
                        wEnd += preSeq.Length;
                    }

                    curPos++;
                }
                else //继续向前，因为想找到最长序列
                {
                    curPos++;
                }
            }

            return ans;
        }

        private static void UpdateWindow<T>(Queue<T> win, int maxsize, int countWantInsert, T[] insertData)
        {
            if (win.Count + countWantInsert < maxsize)
                return;
            var diff = win.Count + countWantInsert - maxsize;
            for (var i = 0; i < diff; i++)
                win.Dequeue();
            for (var i = 0; i < diff; i++)
                win.Enqueue(insertData[i]);
        }

        public class Lz77DataNode<T>
        {
            public bool IsSeq;
            private dynamic _obj;
            public T ValData => !IsSeq ? (T) _obj : default;
            public ValueTupleSlim SeqData => IsSeq ? (ValueTupleSlim) _obj : null;
            public Lz77DataNode(T val)
            {
                IsSeq = false;
                _obj = val;
            }
            public Lz77DataNode(int seqStart, int len, T nextVal)
            {
                IsSeq = true;
                _obj = new ValueTupleSlim(seqStart, len, nextVal);
            }
            
            
           
        }
    }
    
    public class LzwCoder
    {
        public static (string code, List<List<T>> dict) LzwEncode<T>(IEnumerable<T> data, HashSet<T> defaultElementSet = null)
        {
            var dict = new List<List<T>>();
            var enumerable = data as T[] ?? data.ToArray();
            defaultElementSet ??= enumerable.Select(e => e).Distinct().ToHashSet();
            dict.AddRange(defaultElementSet.Select(e => new List<T>{e}));
           
            var ans = "";
            var tempList = new List<T>();
            foreach (var e in enumerable)
            {
                tempList.Add(e);
                var (index, seq) = dict.FindFirst((d, _) => d.SequenceEqual(tempList));
                if (index < 0)
                {
                    tempList.RemoveAt(tempList.Count - 1);
                    var lastInf = dict.FindFirst((d, _) => d.SequenceEqual(tempList));
                    ans += lastInf.index + " ";
                    //不存在现在的序列的情况
                    tempList.Add(e);
                    dict.Add(new List<T>(tempList)); //添加该序列
                    
                    
                    tempList.Clear();
                    tempList.Add(e);
                }
            }
            if(tempList.Any())
                ans += dict.FindFirst((d, _) => d.SequenceEqual(tempList)).index;
            ans.PrintToConsole();
            return (ans, dict);
        }

        public static List<T> Decode<T>(string code, List<List<T>> dict)
        {
            var codeList = code.Trim().Split(" ").Select(int.Parse).ToList();
            var ans = new List<T>();
            foreach (var e in codeList)
            {
                if (e < dict.Count)
                {
                    ans.AddRange(dict[e]);
                }
                else
                {
                    throw new ArithmeticException();
                }
            }
            ans.PrintEnumerationToConsole();
            return ans;
        }
    }
    
    
    
    
    public class LzssCoder{
          //TODO
        public LzssCoder()
        {
        }

        public static void Test()
        {
            var cmpStr = File.ReadAllText("D:\\CIExam\\compression1.txt");
            "压缩".PrintToConsole();
            cmpStr.PrintToConsole();
            "源数据".PrintToConsole();
            var r = LzssCoder.EnCode(cmpStr);
            r.PrintEnumerationToConsole();
            "".PrintToConsole();
            cmpStr.Cast(e => (int)e).PrintEnumerationToConsole();
            "解压".PrintToConsole();
            var r2 = LzssCoder.DeCode(r);
            r2.Cast(e => (int)e).PrintEnumerationToConsole();
        }

        public static List<T> DeCode<T>(IEnumerable<LzssDataNode<T>> lz77DataNodes)
        {
            var ans = new List<T>();
            foreach (var node in lz77DataNodes)
            {
                if(!node.IsSeq)
                    ans.Add(node.ValData);
                else
                {
                    //解码
                    var seq = node.SeqData;
                    var pre = (int)seq[0];
                    var count = (int)seq[1];
                    ans.AddRange(ans.Skip(ans.Count - pre).Take(count));
                }
            }

            return ans;
        }
        
        
        public static List<LzssDataNode<T>> EnCode<T>(IEnumerable<T> data, int wSize = 255, int forwardWinSize = 255, int minLen = 2)
        {
            //整体结构  |窗口内数据|  当前序列 | 前向缓冲|
            var dataCollection = data.ToList();
            var ans = new List<LzssDataNode<T>>();
            var win = new Queue<T>(); //与LZW不同，LZ77是加窗的
            var wEnd = -1;
            
            var curPos = 0; //当前纸质
            while (curPos < dataCollection.Count)
            {
                //"".PrintToConsole();
                //获取窗口
                var forwardWin = dataCollection.Skip(wEnd + 1).Take(curPos - wEnd).ToArray();
                //($"cur pos = {curPos}, cur char = {dataCollection[curPos]},forward win = {forwardWin.ToEnumerationString()}, " +
                //    $"pre buffer = {win.ToEnumerationString()}").PrintToConsole();
                //窗口是否存在 当前得到的序列
                var index = win.BoyerMooreSearch(forwardWin);
                
                //前向缓冲已经满时，或者当前序列找不到时，都处理已遍历数据。注意前向缓冲满也能够直接加序列。因为说明至今都是连续匹配成功的
                if (index < 0 || curPos - wEnd == forwardWinSize)
                {
                    //$"not found seq or forward buffer full".PrintToConsole();
                    
                    if (curPos - wEnd == 1 &&  curPos - wEnd != forwardWinSize)
                    {
                        ans.Add(new LzssDataNode<T>(dataCollection[curPos]));
                        //$"write char = {dataCollection[curPos]}".PrintToConsole();
                        UpdateWindow(win, wSize, 1, new[] {dataCollection[curPos++]});
                        wEnd++;
                    }
                    else
                    {
                        // if (curPos >= 200)
                        //     return ans;
                        var wantInsert = forwardWin[..^1];
                        //$"seq want to insert = {wantInsert.ToEnumerationString()}".PrintToConsole();
                        //存在重复序列。加入元组，并移动窗口
                        //注意和lz77不同，lzss有长度限制
                        if (wantInsert.Length >= minLen)
                        {
                            var i = win.BoyerMooreSearch(wantInsert);
                            
                            //注意LZss状态是2元组
                            ans.Add(new LzssDataNode<T>(win.Count - i, wantInsert.Length));
                            
                            //$"write seq = {win.Count - i},{wantInsert.Length}".PrintToConsole();
                            UpdateWindow(win, wSize, wantInsert.Length, 
                               wantInsert);
                            wEnd += wantInsert.Length;
                            curPos = wEnd + 1;
                        }
                        else
                        {
                            //匹配了，但是不符合最短长度要求，那就前移
                            ans.Add(new LzssDataNode<T>(wantInsert[0]));
                            //$"write char = {wantInsert[0]}, because len < minLen".PrintToConsole();
                            UpdateWindow(win, wSize, 1, new[] {wantInsert[0]});
                            wEnd++;
                        }
                        
                    }
                }
                else //继续向前，因为想找到最长序列
                {
                    curPos++;
                }
            }
            
            
            if (wEnd >= curPos - 1) return ans;
            {
                var wantInsert = dataCollection.Skip(wEnd + 1).Take(curPos - wEnd).ToArray();
                if (wantInsert.Length >= minLen)
                {
                    var i = win.FindSubSequence(wantInsert);
                    ans.Add(new LzssDataNode<T>(win.Count - i, wantInsert.Length));
                }
                else
                {
                    //匹配了，但是不符合最短长度要求，那就直接全塞进去
                    ans.AddRange(wantInsert.Select(e => new LzssDataNode<T>(e)));
                }
            }
            return ans;
        }

        private static void UpdateWindow<T>(Queue<T> win, int maxsize, int countWantInsert, T[] insertData)
        {
            // if (win.Count + countWantInsert < maxsize)
            //     return;
            var diff = win.Count + countWantInsert - maxsize;
            for (var i = 0; i < diff; i++)
                win.Dequeue();
            for (var i = 0; i < countWantInsert; i++)
                win.Enqueue(insertData[i]);
        }

        public class LzssDataNode<T>
        {
            public readonly bool IsSeq;
            private readonly dynamic _obj;
            public T ValData => !IsSeq ? (T) _obj : default;
            public ValueTupleSlim SeqData => IsSeq ? (ValueTupleSlim) _obj : null;
            public LzssDataNode(T val)
            {
                IsSeq = false;
                _obj = val;
            }
            public LzssDataNode(int seqStart, int len)
            {
                IsSeq = true;
                _obj = new ValueTupleSlim(seqStart, len);
            }

            public override string ToString()
            {
                return IsSeq ? SeqData.ToString() : ((int)_obj).ToString();
            }
        }
    }
}