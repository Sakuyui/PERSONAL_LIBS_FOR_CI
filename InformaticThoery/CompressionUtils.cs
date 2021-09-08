using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CIExam.InformationThoery;
using CIExam.os.Cache;
using CIExam.Structure;
using EasyCompressor;
using CIExam.FunctionExtension;

namespace CIExam.InformaticThoery.Text
{
    public class CompressionUtils
    {
        public class TunstallTreeNode<T>
        {
            public double Possibility;
            public TunstallTreeNode(double p)
            {
                Possibility = p;
            }
            public Dictionary<T, TunstallTreeNode<T>> Children = new Dictionary<T, TunstallTreeNode<T>>();

        }
        
        public static TunstallTreeNode<T> GetTunstallTree<T>( int n, IEnumerable<T> input)
        {
            var data = input as T[] ?? input.ToArray();
            var alphabet = data.Select(e => e).Distinct().ToHashSet();
            var m = alphabet.Count;
            var root = new TunstallTreeNode<T>(1.0);
            var leafNode = new PriorityQueue<double, TunstallTreeNode<T>>();
            leafNode.EnQueue(1.0, root);
            
            //统计各字母频率
            var freq = data.GroupBy(e => e).Select(g => (g.Key, g.Count() / data.Length))
                .ToDictionary(kv => kv.Key, kv => kv.Item2);
            
            while (leafNode.Count() + m - 1 <= n) //叶子树 + m - 1 <= n
            {
                //选出概率最大的
                var maxPNode = leafNode.DeQueue();
                
                //分支
                var children = alphabet.Select(c => (c, new TunstallTreeNode<T>(maxPNode.priority * freq[c])));
                foreach (var c in children)
                {
                    maxPNode.item.Children[c.c] = c.Item2;
                    leafNode.EnQueue(c.Item2.Possibility, c.Item2);
                }
               
            }

            return root;
        }
        public static double GetEntropy<T>(IEnumerable<T> input)
        {
            var enumerable = input as T[] ?? input.ToArray();
            var n = enumerable.Length;
            return enumerable.GroupBy(e => e).Select(g => g.Count() / (double) n).Select(e => -e * System.Math.Log2(e)).Sum();
        }
        //1. bzip2 => BWT + RLE(行程编码) + 霍夫曼
        //2. FSE熵编码 
        
        
        //对二元进行编码
        public static string Fse(string data)
        {
            var dict = new[] {
                    (1, new[] {2, 3, 5}), (2, new[] {4, 6, 10}), (3, new[] {7, 8, 15}),
                    (4, new[] {9, 11, 20}), (5, new[] {12, 14, 25}), (6, new[]{13,17,30}),
                    (7, new[]{16,21}), (8, new[]{18,22}), (9, new[]{19,26}), (10, new[]{23,28}), (11, new[]{24})
                }.Select(e => from s in e.Item2 select (s, e.Item1)).SelectMany(e => e)
                    .ToDictionary(k => k.s, v => v.Item2);
            
            return "";
        }
       
        public static void Test()
        {
            var sc = new CompressionUtils();
            sc.BwTransform("abracadabra");

            var bw = sc.BwTransform("tyltylmytyl");
            $"BW => {bw.ToEnumerationString()}".PrintToConsole();
            //sc.BwDecode(t).PrintEnumerationToConsole();
            //var bw1 = sc.BwTransform("tyltylmytyl");
            //bw1.PrintCollectionToConsole();
            var charTable = Enumerable.Range(0, 26).Select(e => (char) ('a' + e));
            var mtf1 = sc.MoveToFrontTransform("bananaaa", charTable);
            var mtf2 = sc.MoveToFrontTransform(bw, charTable);
            $"mtf => {mtf2.ToEnumerationString()}".PrintToConsole();
            //sc.MoveToFrontDecode(mtf2 ,charTable).PrintCollectionToConsole();
            //huffman
            var a = HuffmanEnCoder.Encode(mtf2);
            a.PrintCollectionToConsole();
        }

        public List<T> MoveToFrontDecode<T>(IEnumerable<int> seq, IEnumerable<T> valueSpace)
        {
            var cache = new LruList<T>();
            valueSpace.Reverse().ElementInvoke(e => cache.Write(e));

            return seq.Select(s => cache.ReadByIndex(s)).ToList();
        }
        public List<int> MoveToFrontTransform<T>(IEnumerable<T> enumerable, IEnumerable<T> valueSpace)
        {
            var cache = new LruList<T>();
            valueSpace.Reverse().ElementInvoke(e => cache.Write(e));
            var seq = new List<int>();
            
            foreach (var e in enumerable)
            {
                var index = cache.IndexOf(e);
                cache.ReadByIndex(index);
                
                seq.Add(index);
            }
            return seq;
        }
        
        
        
        
        
        //L配列だけ戻ります
        public List<T> BwTransform<T>(IEnumerable<T> input) where T : IComparable
        {
            var data = input as List<T> ?? input.ToList();
            var sb = new List<T>(data.ToArray());
            
            var rotateList = new List<List<T>> {new(data)};
          
            for (var i = 1; i < data.Count; i++)
            {
                rotateList.Add(new List<T>(RotateLeftShift(sb)));
            }
            //"Original >>".PrintToConsole();
            //rotateList.PrintCollectionToConsole();
            var sorted = rotateList.OrderBy(s => s, 
                new CustomerComparer<List<T>>(delegate(List<T> t1, List<T> t2)
            {
                var n1 = t1.Count;
                var n2 = t2.Count;
                if (n1 < n2)
                    return -1;
                if (n1 > n2)
                    return 1;
                for (var i = 0; i < n1; i++)
                {
                    if(t1[i].Equals(t2[i]))
                        continue;
                    return t1[i].CompareTo(t2[i]);
                }
                return 0;
            })).ToList();
            //"Sorted >>".PrintToConsole();
            //sorted.PrintCollectionToConsole();
            
            var f = sorted.Select(s => s[0]).ToList();
            //"f >> ".PrintToConsole();
            //f.PrintCollectionToConsole();
            var l = sorted.Select(s => s[^1]).ToList();
            //"l >> ".PrintToConsole();
            //l.PrintCollectionToConsole();
            
            return l;
            
        }

        //Ｌ配列に基づいて、源文字列を復号。
        public IEnumerable<T> BwDecode<T>(List<T> l)
        {
            //前缀和，用来辅助
            //为了实现更广泛的字符集。这里使用离散化
            //离散化，并且映射成字典
            "===============================decode============================".PrintToConsole();
            l.PrintCollectionToConsole();
            //离散化
            //|K|log |K|
            var charSet = l.Distinct().OrderBy(c => c).ToList()
                .Select((c, i) => (c, i)).ToList(); //独立で現れた符号を抽出
            //Map (char -> new index) 離散化を行う
            var map = charSet.ToDictionary(k => k.c, v => v.Item2);
            
           
            //计数
            var counter = new int[map.Count];
            
            //O(n)
            foreach (var t in l)
            {
                counter[map[t]]++;
            }
            counter.Select((e, i) => (charSet[i].c, e)).PrintCollectionToConsole();
            "construct next array >> ".PrintToConsole();

            //前缀和 O(n)，类似搞计数排序
            for (var i = 1; i < counter.Length; i++)
            {
                counter[i] += counter[i - 1];
            }
            var indexMap = new Dictionary<T, List<int>>();
            
            //O(n) 同じ接頭辞をレコード
            for (var i = 0; i < l.Count; i++)
            {
                var c = l[i];
                if (!indexMap.ContainsKey(c))
                {
                    indexMap[c] = new List<int>();
                }
                indexMap[c].Add(i);
            }
            indexMap.Select(e => (e.Key, e.Value.ToEnumerationString())).PrintEnumerationToConsole();
            var next = new int[l.Count];
            
            charSet.PrintEnumerationToConsole();

            //这里的charset其实按顺序填完整的话就是F数组惹
            //其实下面的代码就是按顺序填完。填完就是 # l mm ttt yyyy。但是填如next的是各字符在L中的对应位置。然后要满足i < j下next[i]<next[j]的需求
            //O(n)
            //循環１：離散化された符号を巡回する
            for (var i = 0; i < counter.Length; i++)
            {
                //这样能够记录从哪开始填写，类似计数排序
                var begin = i == 0 ? 0 : counter[i - 1];
               
                $"ps. {charSet[i].c}".PrintToConsole();
                var list = indexMap[charSet[i].c];
                for (var j = 0; j < list.Count; j++)
                {
                    //put next[begin] to next [begin + j] with list
                    next[begin + j] = list[j];
                }
            }
            next.PrintCollectionToConsole();

            var pos = next[0];
            
            //解码

            return next.Select(t => l[pos = next[pos]]).ToList();
        }

        private StringBuilder StringRotateRightShift(StringBuilder sb)
        {
            if (sb.Length <= 1)
                return sb;
            sb.Insert(0, sb[^1]);
            sb.Remove(sb.Length - 1, 1);
            return sb;
        }
        private StringBuilder StringRotateLeftShift(StringBuilder sb)
        {
            if (sb.Length <= 1)
                return sb;
            sb.Append(sb[0]);
            sb.Remove(0, 1);
            return sb;
        }
        private List<T> RotateRightShift<T>(List<T> sb)
        {
            if (sb.Count <= 1)
                return sb;
            sb.Insert(0, sb[^1]);
            sb.RemoveAt(sb.Count - 1);
            return sb;
        }
        private List<T> RotateLeftShift<T>(List<T> sb)
        {
            if (sb.Count <= 1)
                return sb;
            sb.Add(sb[0]);
            sb.RemoveAt(0);
            return sb;
        }
    }
}