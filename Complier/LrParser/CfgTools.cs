using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using JJ.Framework.Collections;

namespace CIExam.Complier.LrParser
{
    public class CfgTools
    {
         public static void GetFirstSetDfs(Dictionary<string, HashSet<string>> dict, string item, 
             Dictionary<string, List<List<string>>> _produceMapping, ProducerDefinition definitions, HashSet<string> finished = null, 
             Stack<string> path = null) 
         {
            //("process : " + item).PrintToConsole();
            
            path ??= new Stack<string>();
            finished ??= new HashSet<string>();
            
            if (finished.Contains(item) || path.Contains(item))
            {
                //(item + " has been processed").PrintToConsole();
                return;
            }
            path.Push(item);
            //$"First:  put {item}".PrintToConsole();
            var t = item;
            if (definitions.Terminations.Contains(t) || t.Equals("ε"))
            {
                //"终结符直接返回".PrintToConsole();
                foreach (var node in path)
                {
                    if(!definitions.Terminations.Contains(node))
                        dict[node].Add(t);
                    
                }

                path.Pop();
                return;
            }
            if (definitions.NonTerminationWords.Contains(t))
            {
                var first = new HashSet<string>();
                var produces = _produceMapping[t];
                //produces.PrintMultiDimensionCollectionToConsole();
                foreach (var p in produces) //每个产生式
                {
                    foreach (var t1 in p)
                    {
                        GetFirstSetDfs(dict, t1,_produceMapping, definitions, finished, path);
                        if (definitions.Terminations.Contains(t1) || !dict[t1].Contains("ε"))
                        {
                            break;
                        }
                    }
                }
                //first.PrintEnumerationToConsole();
            }
            else
            {
                throw new ArithmeticException();
            }

            //$"Pop: {path.Peek()}".PrintToConsole();
            finished.Add(path.Pop());
        }

         public static HashSet<string> GetSequenceFirstSet(ProducerDefinition definition, List<string> input)
         {
             if (input.Count <= 0)
                 return null;
             if (input[0] == "$")
                 return new HashSet<string> {"$"};
             var f = GetFirstSet(definition);
             var ans = new HashSet<string>();
             foreach (var t in input)
             {
                 if (!f[t].Contains("ε"))
                 {
                     ans.AddRange(f[t]);
                     return ans;
                 }
                 ans.AddRange(f[t].Except("ε"));
             }

             return ans;
         }
        public static Dictionary<string, HashSet<string>> GetFirstSet(ProducerDefinition definition)
        {
            /*
             * （1）若X 是终结符或ε，则First (X) = {X}。
（2）若X 是非终结符，则对于每个产生式X→X1X2. . . Xn ，
First (X)都包含了First(X1) - {ε}。
若对于某个i < n ，所有的集合First (X1), . . . , First (Xi) 都包括了。则First (X) 也包括了First (X i + 1 ) -{ε}。若所有集合First (X1), . . . , First (Xn)包括了ε
，则First (X)也包括ε。
             */

            var dict = new Dictionary<string, HashSet<string>>();
            foreach (var s in definition.ProduceMapping.Keys)
            {
                dict[s] = new HashSet<string>();
            }

            var path = new Stack<string>();
            var finish = new HashSet<string>();
            foreach (var sWord in definition.ProduceMapping.Keys)
            {
                GetFirstSetDfs(dict, sWord, definition.ProduceMapping, definition, finish, path);
            }
            //dict.Select(kv => kv.Key + " => " + kv.Value.ToEnumerationString()).PrintEnumerationToConsole();
            foreach (var t in definition.Terminations.Where(nonT => !dict.ContainsKey(nonT)))
            {
                dict[t] = new HashSet<string> {t};
            }
            
            //dict.Select(e => (e.Key, e.Value.GetMultiDimensionString())).PrintMultiDimensionCollectionToConsole();
            return dict;
        }
    }
}