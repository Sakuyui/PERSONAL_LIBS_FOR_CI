// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using CIExam.FunctionExtension;
// using CIExam.Structure.Graph;
//
// namespace CIExam.Network
// {
//     public class RouterTable:IEnumerable<KeyValuePair<int, RouterTable.Path>>
//     {
//         public int RouterID;
//
//         public override string ToString()
//         {
//             var s = $"[Node :{RouterID}]\n";
//             s += this.Aggregate("", (a, b) => a + $"To:{b.Key}, Via = {b.Value.Via}, Cost = {b.Value.Cost}\n");
//             return s;
//         }
//
//         public struct Path
//         {
//             public int Cost;
//             public int Via;
//         }
//
//         public Dictionary<int, Path> Paths = new Dictionary<int, Path>();
//         public IEnumerator<KeyValuePair<int, Path>> GetEnumerator()
//         {
//             return Paths.GetEnumerator();
//         }
//
//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return GetEnumerator();
//         }
//     }
//     public class NetworkAlgorithms
//     {
//         public void Rip(BaseGraph<RouterTable, int> topology)
//         {
//             var routerTableCopy = topology.Nodes.Select(e => (e, e.Data))
//                 .ToDictionary(kv => kv.e, kv => kv.Data);
//             var i = 0;
//             while (i < 10)
//             {
//                 $"time {i}".PrintToConsole();
//                 //UpData
//                 foreach (var node in topology.Nodes)
//                 {
//                     var nodeIndex = topology[node];
//                     var ext = topology.GetExtendNodes(node);
//                     //从邻居得到表
//                     var tableIn = ext.Select(e => (e,routerTableCopy[e])).ToArray();
//                     foreach (var table in tableIn)
//                     {
//                         foreach (var item in table.Item2)
//                         {
//                             if (!node.Data.Paths.ContainsKey(item.Key))
//                             {
//                                 //理由表不存在到达item的条目，直接加入
//                                 node.Data.Paths[item.Key] = new RouterTable.Path
//                                 {
//                                     Via = topology[table.e],
//                                     Cost = item.Value.Cost + 1
//                                 };
//                             }
//                             else
//                             {
//                                 //松弛
//                                 var selfTableItem = node.Data.Paths[item.Key];
//                                 if (item.Value.Cost + 1 < selfTableItem.Cost)
//                                 {
//                                     selfTableItem.Cost = item.Value.Cost + 1;
//                                     selfTableItem.Via = topology[table.e];
//                                 }
//                             }
//                         }
//                     }
//                 }
//
//                 i++;
//             }
//             
//         }
//     }
// }