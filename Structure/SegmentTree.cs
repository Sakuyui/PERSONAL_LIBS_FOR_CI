using System;
using System.Collections.Generic;
using CIExam.FunctionExtension;
using CIExam.StringProcess;

namespace CIExam.Structure
{
    public class SegmentTreeNode<TVal> : BinaryTreeNode<TVal>
    {
        //节点存储区间
        public int RangeLeft;
        public int RangeRight;

        public TVal Val;
        public SegmentTreeNode<TVal> LeftNode
        {
            get
            {
                Left ??= new SegmentTreeNode<TVal>(RangeLeft, RangeMid);
                return (SegmentTreeNode<TVal>)Left;
            }
        }


        public SegmentTreeNode<TVal> RightNode {
            get
            {
                Right ??= new SegmentTreeNode<TVal>(RangeMid + 1, RangeRight);
                return (SegmentTreeNode<TVal>)Right;
            }
        }
            
        public int RangeMid => RangeLeft + (RangeRight - RangeLeft) / 2;

        public SegmentTreeNode(int rangeLeft, int rangeRight) : base(default)
        {
            RangeRight = rangeRight;
            RangeLeft = rangeLeft;
        }
        
    }
    // public class SegmentTree<TVal>
    // {
    //     public Func<TVal, TVal, TVal> MergeStrategy { get; }
    //     public SegmentTreeNode<TVal> Root = null;
    //     
    //     //合并两个从两个区间获取的值。
    //     /*用线段树统计的东西，必须符合区间加法，否则，不可能通过分成的子区间来得到[L,R]的统计结果。*/
    //     public TVal Merge(TVal v1, TVal v2, Func<TVal, TVal, TVal> mergeStrategy = null)
    //     {
    //         mergeStrategy ??= MergeStrategy;
    //         return mergeStrategy.Invoke(v1, v2);
    //     }
    //
    //     public SegmentTree(Func<TVal, TVal, TVal> mergeStrategy)
    //     {
    //         MergeStrategy = mergeStrategy;
    //     }
    //
    //     public void Init(SegmentTreeNode<TVal> root)
    //     {
    //         Root = root;
    //     }
    //     public void Insert(SegmentTreeNode<TVal> node, int key, TVal val) {
    //         if (node.RangeLeft == node.RangeRight) {
    //             //如果是叶子节点，那就直接更新其值并返回
    //             node.Val = Merge(val, node.Val);
    //             return;
    //         }
    //         
    //         //[l, m,  r] key 若果小于mid,那么要去左边递归
    //         if (key <= node.RangeMid) {
    //             //这货是左子节点范围的，那就去左子节点那边
    //             Insert(node.LeftNode, key, val);
    //         } else {
    //             //这货是右子节点范围的，那就去右子节点那边
    //             Insert(node.RightNode, key, val);
    //         }
    //         //递归回来之后，更新父节点的值（因为下面的节点更新过了）
    //         node.Val = Merge(node.LeftNode.Val, node.RightNode.Val);
    //     }
    //     
    //     public TVal RightRangeQuery(SegmentTreeNode<TVal> node, int key, TVal defaultVal = default) {
    //         //[l, m, r] key已经到r右边了。
    //         //如果node的right都比key小，那就是你了，node
    //         if (node.RangeRight <= key) return node.Val;
    //         //连最左边都比key大，就是key到l的左边了没救了，key你就是无，默认
    //         if (key < node.RangeLeft) return defaultVal;
    //         //如果key在node的left-right之间，那就要综合考虑左区间和右区间的结果
    //         //在左右区间查询并合并结果
    //         
    //         return Merge(RightRangeQuery(node.LeftNode, key), RightRangeQuery(node.RightNode, key));
    //     }
    //
    // }
        public  class SegmentTree<TNodeVal> 
        {
            public class SegmentTreeNode : BinaryTreeNode<(int L, int R, TNodeVal Val)>
            {
                //节点区间
                
                
                public Dictionary<string, dynamic> ParamsMap = new();
                public SegmentTreeNode(int l, int r, TNodeVal val, SegmentTreeNode left, SegmentTreeNode right) : base((l,r,val))
                {
                    // L = l;
                    // R = r;
                    // Val = val;
                    Left = left;
                    Right = right;
                }

                public override string ToString()
                {
                    return $"[{Data.L},{Data.R}, val = {Data.Val}]";
                }
            }
            public delegate void InsertFinishCallBack(SegmentTreeNode root);

            public SegmentTreeNode Root = null;
            public SegmentTreeNode Insert( int l, int r, TNodeVal val, SegmentTreeNode root = null, InsertFinishCallBack callBack = null)
            {
                $"Insert {l},{r} - {val}, from {root}".PrintToConsole();
                if (root == null)
                {
                    var newNode = new SegmentTreeNode(l, r, val, null, null);
                    return newNode;
                }
                //recursive insert
                if (l <= root.Data.L)
                    root.Left = Insert( l, r, val, (SegmentTreeNode) root.Left, callBack);
                else
                    root.Right = Insert( l, r, val, (SegmentTreeNode) root.Right, callBack);

                callBack?.Invoke(root);
                return root;
            }

            public override string ToString()
            {
                return Root == null ? "[]" : Root.PreorderEnumerator.ToEnumerationString();
            }

            public delegate TNodeVal MergeStrategy(TNodeVal left, TNodeVal right, TNodeVal curVal, SegmentTreeNode node);
            public delegate TNodeVal PreSearchStrategy(SegmentTreeNode node);
            public delegate (bool, TNodeVal) FullPruneStrategy(SegmentTreeNode root);
            public delegate (bool, TNodeVal) LeftPruneStrategy(SegmentTreeNode root, TNodeVal curVal);
            public delegate (bool, TNodeVal) RightPruneStrategy(SegmentTreeNode root, TNodeVal curVal, TNodeVal leftVal);
            
            public TNodeVal Query(SegmentTreeNode root, int l, int r, MergeStrategy mergeStrategy, 
                PreSearchStrategy preSearchStrategy, FullPruneStrategy pruneStrategy1 = null, LeftPruneStrategy leftPruneStrategy = null,
                RightPruneStrategy rightPruneStrategy = null)
            {
                 //剪枝策略
                 var t = pruneStrategy1?.Invoke(root) ?? (false, default);
                 if (t.Item1)
                     return t.Item2;
                 
                 // 高度
                 var curVal = preSearchStrategy.Invoke(root);
                
                 // 剪枝
                 var leftPrune = leftPruneStrategy?.Invoke(root, curVal) ?? (false, default);
                 if (leftPrune.Item1)
                     return leftPrune.Item2;
                 var left = 
                     Query((SegmentTreeNode) root.Left, l, r, mergeStrategy, preSearchStrategy, pruneStrategy1, leftPruneStrategy, rightPruneStrategy);
                
                 var rightPrune = rightPruneStrategy?.Invoke(root, curVal, left) ?? (false, default);
                 if (rightPrune.Item1)
                     return rightPrune.Item2;
                 
                 var right = Query((SegmentTreeNode) root.Right,l, r, mergeStrategy, preSearchStrategy, pruneStrategy1, leftPruneStrategy, rightPruneStrategy);

                 return mergeStrategy.Invoke(left, right, curVal, root);
            }


        }
}