using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CIExam.FunctionExtension;
using CIExam.Structure;

namespace CIExam.MetaHeuristic
{
    public class MetaHeuristicAlgorithms
    {


        public double GetZeroPoint(Func<double, double> method, double s = -1000, double e = 1000, double delta = 0.002)
        {
            var fa = method(s);
            if (fa == 0.0)
                return s;
            var fb = method(e);
            if (fb == 0.0)
                return e;
            if(fa * fb > 0.0)
                return double.NaN;
            double preX = 0;
            for (var i = 0; i < 30; i++)
            {
                var c = (s + e) / 2.0;
                var fc = method(c);
                var v = System.Math.Sqrt(fc * fc - fa * fb);
                if (v == 0)
                    return double.NaN;
                var dx = (c - s) * fc / v;
                if (fa - fb < 0.0)
                    dx = -dx;
                var x = c + dx;
                var fx = method(x);
                if (i > 0)
                {
                    if (System.Math.Abs(x - preX) < delta * System.Math.Max(System.Math.Abs(x), 1.0))
                        return x;
                    preX = x;
                }

                if (fx * fx > 0)
                {
                    if (fa * fx < 0.0)
                    {
                        e = x;
                        fb = fx;
                    }
                    else
                    {
                        s = x;
                        fa = fx;
                    }
                }
                else
                {
                    s = c;
                    e = x;
                    fa = fc;
                    fb = fx;
                }
                
            }

            return s;
        }
        
        /*黄金分割求极值
         通俗点讲就是讲先给定搜索区间比如[a b]，然后另x1 = a + 0.382(b - a),x2 = a + 0.618(b - a)，
         然后把x1和x2代入到函数f(x)中比较f(x1)和f(x2)的大小，如果f(x1)>f(x2)，则让a = x1，否则b = x2，
         然后在新的搜索区间[a b]内，重新找到x1和x2重复以上过程，直到b - a<ξ(这个是给出的最小精度)，然后取a,b的平均值近似代替f(x)min。
         
         */
        
        public static (double mX, double max) GoldDivisionSearchMax(Func<double, double> method, double s = -1000, double e = 1000, double delta = 0.002)
        {
            var r = new Random();
            var a = s;
            var b = e;
            while (true)
            {
                var c = a + 0.382 * (b - a);
                var d = a + 0.618 * (b - a);
                if (System.Math.Abs(c - d) < delta)
                {
                    return ((c + d) / 2, method((c + d) / 2));
                }

                if (method(c) - method(d) < 0) //求最大值的话换成 > 0
                    a = c;
                else
                    b = d;
            }
        }
        
        /*
         * 
  NP = 50; % 种群大小
  L = 16; % 染色体序列长度
  Pc = 0.8; % 交叉概率
  Pm = 0.1; % 变异概率
  G = 50; % 主循环迭代次数
  Xs = 10; % 所求函数的定义域的最大值
  Xx = 0; % 所求函数的定义域的最小值
  S = 10; % 什么意思自己悟吧，我只是不想硬编码成10
         *
         * 
         */
        public static (double mX, double max) NeiboorSearch(Func<double, double> method, double x, int sample = 5, double range = 1.0)
        {
            var r = new Random();
            var res = new List<(double ,double)>();
            for(var i = 0; i < sample; i++)
            {
                var rand = r.NextDouble() * 2 * range - range;
                var xx = x + rand;
                res.Add((xx,method(xx)));
            }

            return res.ArgMax(e => e.Item2).Item2;
        }
        public static (double mX, double max) GetMaxValueYakinamashi(Func<double, double> method)
        {
            var r = new Random();
            var max = method(0);
            var mX = 0.0;
            var t = 100.0;
            var rate = 0.99;
            while(t > 1)
            {
                var rand = r.NextDouble() * 200 - 100;
                var y = method(rand);
                var nb = NeiboorSearch(method,rand);
                if (nb.max > y)
                {
                    y = nb.max;
                    rand = nb.mX;
                }
                
                if (y > max)
                {
                    max = y;
                    mX = rand;
                }
                else
                {
                    var a = System.Math.Exp(-(max - y) / t);
                    a.PrintToConsole();
                    if (a > r.NextDouble())
                    {
                        mX = rand;
                        max = y;
                    }
                }

                t *= rate;
                $"cur t = {t}, max = {max}".PrintToConsole();
            }

            return (mX, max);
        }
        public class KnapsackItem : IComparable<KnapsackItem> {
            public readonly int Weight;
            public readonly int Value;
            public readonly int UnitValue;

            public KnapsackItem(int weight, int value) {
                Weight = weight;
                Value = value;
                UnitValue = weight == 0 ? 0 : value / weight;
            }
            
            public int CompareTo(KnapsackItem knapsackItem) => UnitValue.CompareTo(knapsackItem.UnitValue);
            
        }
        
        private class Node {
            // 当前放入物品的重量
            public readonly int CurrWeight;
            // 当前放入物品的价值
            public readonly int CurrValue;
            // 不放入当前物品可能得到的价值上限
            public int UpBoundValue;
            // 当前操作的索引
            public readonly int Index;

            public Node(int currWeight, int currValue, int index) {
                CurrWeight = currWeight;
                CurrValue = currValue;
                Index = index;
            }
        }
        


        //实际上构建的是一颗子孙节点指向父节点，但是父节点不知道孩纸节点的二叉树，并且不断扩张该二叉树
        public class KnapsackBranchLimit {
            private readonly KnapsackItem[] _bags;
            private readonly int _totalWeight;
            private readonly int _n;
            // 物品放入背包可以获得的最大价值
            public int BestValue;
            
            public KnapsackBranchLimit(KnapsackItem[] bags, int totalWeight) {
                _bags = bags;
                _totalWeight = totalWeight;
                _n = bags.Length;
                // 物品依据[单位重量]价值进行排序
                _bags = bags.OrderByDescending(e => e).ToArray();
            }

            public void Solve()
            {
                //获取上界
                int GetUpBoundValue(Node n) {
                    var surplusWeight = _totalWeight - n.CurrWeight;  //剩余容量
                    var value = n.CurrValue; //当前价值
                    var i = n.Index;

                    while (i < _n && _bags[i].Weight <= surplusWeight) {
                        surplusWeight -= _bags[i].Weight;
                        value += _bags[i].Value;
                        i++;
                    }

                    // 当物品超重无法放入背包中时，可以通过背包剩余容量*下个物品单位重量的价值计算出物品的价值上限
                    if (i < _n) {
                        value += _bags[i].UnitValue * surplusWeight;
                    }

                    return value;
                }
                
                //初始节点
                //优先队列。更改比较策略。大的优先
                var nodeList = new PriorityQueue<double, Node>((d, d1) => d1.CompareTo(d));
                var initNode = new Node(0, 0, 0);
                nodeList.EnQueue(0, initNode);

                // 起始节点当前重量和当期价值均为0
                while (!nodeList.Any())
                {
                    // 取出放入队列中的第一个节点
                    var node = nodeList.DeQueue().item;

                    if (node.UpBoundValue < BestValue || node.Index >= _n) continue; //剪枝
                    
                    // 左节点：该节点代表物品放入背包中，上个节点的价值+本次物品的价值为当前价值
                    var leftWeight = node.CurrWeight + _bags[node.Index].Weight;
                    var leftValue = node.CurrValue + _bags[node.Index].Value;
                    var left = new Node(leftWeight, leftValue, node.Index + 1);

                    // 放入当前物品后可以获得的价值上限。上界函数是利用贪心给出的
                    left.UpBoundValue = GetUpBoundValue(left);

                    // 当物品放入背包中左节点的判断条件为保证不超过背包的总承重
                    if (left.CurrWeight <= _totalWeight && left.UpBoundValue > BestValue)
                    {
                        nodeList.EnQueue(left.UpBoundValue, left);
                        if (left.CurrValue > BestValue)
                        {
                            // 物品放入背包不超重，且当前价值更大，则当前价值为最大价值
                            BestValue = left.CurrValue;
                        }
                    }

                    // 右节点：该节点表示物品不放入背包中，上个节点的价值为当前价值
                    var right = new Node(node.CurrWeight, node.CurrValue, node.Index + 1);

                    // 不放入当前物品后可以获得的价值上限
                    right.UpBoundValue = GetUpBoundValue(right);

                    if (right.UpBoundValue >= BestValue)
                    {
                        // 将右节点添加到队列中
                        nodeList.EnQueue(right.UpBoundValue, right);
                    }
                }
            }

        }
    }
}