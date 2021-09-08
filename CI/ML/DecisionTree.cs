using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;
using CIExam.Math;
using CIExam.Structure;

namespace CIExam.CI.ML
{
    // public class DecisionTree
    // {
    //     public class DecisionTreeNode : CommonTreeNode<(int label, int branchAttribution)>
    //     {
    //         public Dictionary<double, DecisionTreeNode> Branch = new Dictionary<double, DecisionTreeNode>(); //分支属性依据值 ->下一个节点
    //         public DecisionTreeNode((int label, int branchAttribution) data) : base(data)
    //         {
    //         }
    //
    //         public DecisionTreeNode() : base(default)
    //         {
    //             
    //         }
    //     }
    //     public static DecisionTreeNode TreeGenerate(IEnumerable<(Vector<double> x, int label)> dateset, HashSet<int> attributionCanSel,
    //         Func<IEnumerable<(Vector<double> x, int label)>, HashSet<int>, int> selAttribution)
    //     {
    //         var node = new DecisionTreeNode();
    //         var data = dateset as (Vector<double> x, int label)[] ?? dateset.ToArray();
    //         if (data.Any() && data.Select(d => d.label).Distinct().Count() == 1)
    //         {
    //             node.Data = (data.First().label, default);
    //             return node;
    //         }
    //
    //         if (!attributionCanSel.Any() || data.Select(d => d.x.SelectByIndexes(attributionCanSel))
    //             .Distinct(new CustomerEqualityComparer<IEnumerable<double>>((t1, t2)
    //                 => t1.SequenceEqual(t2), doubles => doubles.Select(e => e.GetHashCode()).Sum())).Count() == 1)
    //         {
    //             var label = data.GroupBy(d => d.label).OrderByDescending(d => d.Count())
    //                 .First().Key;
    //             node.Data = (label, default);
    //             return node;
    //         }
    //         
    //         //选择一个最优划分属性A
    //         var a = selAttribution.Invoke(data, attributionCanSel);
               //对于属性a的每种取值。
    //         var attributionsValue = data.Select(d => d.x[a]).ToHashSet();
    
    //         foreach (var attribution in attributionsValue)
    //         {
    //             //branch
    //             //var branchNode = TreeGenerate();
    //             var dV = data.Where(d => d.x[a] == attribution);
    //         }
    //         
    //         return null;
    //     }
    // }
}