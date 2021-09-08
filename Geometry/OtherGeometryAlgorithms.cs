using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJ.Framework.Collections;
using CIExam.FunctionExtension;
using CIExam.Structure;

namespace CIExam.Geometry
{
    public class OtherGeometryAlgorithms
    {
        public int RectangleArea(int[][] rectangles) {
            //离散化
            var xs = rectangles.SelectMany(e => new[] {e[0], e[2]}).Distinct().OrderBy(e => e).ToList();
            var ys = rectangles.SelectMany(e => new[] {e[1], e[3]}).Distinct().OrderBy(e => e).ToList();
            
            var dictX = xs.Select((e, i) => (e, i))
                .ToDictionary(kv => kv.e, kv => kv.i);//source int -> new int
        
            var dictY = ys.Select((e, i) => (e, i))
                .ToDictionary(kv => kv.e, kv => kv.i);//source int -> new int
            
            var grid = new bool[dictX.Count].Select(_ => new bool[dictY.Count]).ToArray();
            foreach (var rec in rectangles)
                for (var x = dictX[rec[0]]; x < dictX[rec[2]]; ++x)
                for (var y = dictY[rec[1]]; y < dictY[rec[3]]; ++y)
                    grid[x][y] = true;

            long ans = 0;
            for (var x = 0; x < grid.Length; ++x)
            for (var y = 0; y < grid.Length; ++y)
                if (grid[x][y])
                    ans += (long) (xs[x + 1] - xs[x]) * (ys[y + 1] - ys[y]);

            ans %= 1000000007;
            return (int) ans;

            
        }
        public static void Test()
        {
            var testData1 = new[]
                {new[] {2, 9, 10}, new[] {3, 7, 15}, new[] {5, 12, 12}, new[] {15, 20, 10}, new[] {19, 24, 8}}
                .Select(e => (e[0],e[1],e[2])).ToList();
            var res = GetRectangleEdges(testData1);
            res.PrintEnumerationToConsole();
            GetRectangleEdgesScanLine(testData1).PrintEnumerationToConsole();
        }
        
        
        public List<List<int>> GetSkyline(int[][] buildings)
        {
            if (buildings == null || buildings.Length == 0)
                return new List<List<int>>();
            return Solve(buildings, 0, buildings.Length - 1);
        }
        private static List<List<int>> Solve(IReadOnlyList<int[]> buildings, int l, int r)
        {
            var res = new List<List<int>>();
            //原子问题
            if(l == r)
            {
                res.Add(new List<int> {buildings[l][0], buildings[l][2]}); 
                res.Add(new List<int> {buildings[l][1], 0});
                return res;
            }
            //分治
            var mid = l + (r - l) / 2;
            var left = Solve(buildings, l, mid);
            var right = Solve(buildings, mid + 1, r);

            int i = 0, j = 0;
            int leftY = 0, rightY = 0, curY = 0;
            while(i < left.Count && j < right.Count)
            { 
                var pL = left[i];
                var pR = right[j];
                var x = 0;
                if(pL[0] < pR[0])
                {
                    x = pL[0];
                    leftY = pL[1];
                    i++;
                }
                else if(pL[0] > pR[0])
                {
                    x = pR[0];
                    rightY = pR[1];
                    j++;
                }
                else
                {
                    x = pL[0];
                    leftY = pL[1];
                    rightY = pR[1];
                    i++;
                    j++;
                }
                var maxY = System.Math.Max(leftY, rightY);
                if (curY == maxY) 
                    continue;
                var x1 = x;
                res.Add(new List<int> {x1, maxY});
                curY = maxY;
            }
            while(i < left.Count)
            {
                res.Add(left[i]);
                i++;
            }
            while(j < right.Count)
            {
                res.Add(right[j]);
                j++;
            }
            return res;
        }

        /*扫描线 + 优先队列*/
        /*很巧妙的做法，利用了 muliset 这一数据结构自动排序的特性。先比较 first，哪个小则排在前；first 相等则 second小的排在前。
         而 first 这里表示横坐标，second 为负时，表示建筑的左侧在这一位置，其绝对值表示建筑在的高度；second 为正时，表示建筑的右侧在这一位置。
         遍历时，首先会取出横坐标小的点。如果2个点横坐标相等，会先取出 second 小的点，对于负数来说，其实就是高度更高的建筑。
         也就是说，两个点上有高度不同的建筑，会先取高的出来放入高度集合，集合中高度最大值和之前高度不同，就直接放入结果。后面更低高度的建筑加入并不会改变最大高度。
如果second为正，表示建筑物在此处结束，需要把相应高度从高度集合中删除。有相同建筑同时在此结束，则会先让较低的建筑离开，因为它们不会改变最大高度。只有当最高的建筑物离开时，才进行改变。
如果一个位置既有建筑物进来，又有建筑物离开，会先选择进来的。*/
        
        /*天际线只在建筑的左右点发生，用最小堆记录每个建筑变化的地方(按x坐标排序): 高度为负数是加入了建筑，为正数是删除建筑。(也可以不用堆，直接加入最后排序)
如果加入了的新建筑是最高的，那么天际线发生了变化；
同理，如果删除了一个建筑以后，最高的高度变小了，天际线发生了变化。
为了统计每个点当时所有建筑里最高的高度，我使用了SortedDict充当Counter。实际上也可以直接使用SortedList,就不需要统计高度的个数了，每次判断最高高度是否发生了变化即可。
*/
        public static IEnumerable<(int x, int y)> GetRectangleEdgesScanLine(List<(int f, int t, int height)> rectangles)
        {
            var all = new SortedSet<(int k, int v)>(new CustomerComparer<(int k, int v)>(
                (t1, t2) => t1.k == t2.k ? t1.v.CompareTo(t2.v) : t1.k.CompareTo(t2.k)));
            
            var res = new List<List<int>>();
            foreach (var e in rectangles) {
                all.Add((e.f, -e.height)); // critical point, left corner
                all.Add((e.t, e.height)); // critical point, right corner
            }
            var heights = new SortedSet<int>{0}; // 保存当前位置所有高度。
            var last = new List<int>{0, 0}; // 保存上一个位置的横坐标以及高度
            
            foreach (var p in all) {
                if (p.v < 0) heights.Add(-p.v); // 左端点，高度入堆
                else
                {
                    heights.Remove(p.v);
                } // 右端点，移除高度

                // 当前关键点，最大高度
                var maxHeight = heights.Reverse().First();
            
                // 当前最大高度如果不同于上一个高度，说明这是一个转折点
                if (last[1] == maxHeight) continue;
                // 更新 last，并加入结果集
                last[0] = p.k;
                last[1] = maxHeight;
                res.Add(new List<int>(last));
            }

            return res.Select(l => (l[0], l[1])).ToList();
        }
        //分治法实现矩形轮廓，需要排序，总体O(nlog n)
        public static List<(int f, int t, int height)> GetRectangleEdges(List<(int f, int t, int height)> rectangles, bool sorted = false)
        {
            if (rectangles.Count is 0 or 1)
                return rectangles;
            
            if (!sorted)
            {
                "first...sort".PrintToConsole();
                rectangles = rectangles.OrderBy(rect => rect.f).ToList();
            }
            //divide
            
            var m = rectangles.Count >> 1;
            var lData = rectangles.Take(m).ToList();
            var rData = rectangles.Skip(m).ToList();
            $"divide with m = {m}, with ldata = {lData.ToEnumerationString()}, rdata = {rData.ToEnumerationString()}".PrintToConsole();
            
            var left = GetRectangleEdges(lData,  true);
            var right = GetRectangleEdges(rData, true);
            
            $"try conqour {left.ToEnumerationString()} with {right.ToEnumerationString()}".PrintToConsole();
            //conquor
            var lpt1 = 0;
            var lpt2 = 0;
            var res = new List<(int f, int t, int height)>();

            var visit1 = new bool[left.Count];
            var visit2 = new bool[right.Count];
            
            while (lpt1 < left.Count && lpt2 < right.Count)
            {
                $"cur try {left[lpt1]}, {right[lpt2]}".PrintToConsole();
                //(lpt1, lpt2).PrintToConsole();
                //case 1 不相交 2情况
                var l = left[lpt1];
                var r = right[lpt2];
                if (l.f >= r.t) //r完全在f前面   rf rt lf lt
                {
                    "not cross".PrintToConsole();
                    res.Add(r);
                    lpt2++;
                    continue;
                }
                if (r.f >= l.t) // l完全在f前面 lf lt rf rt
                {
                    "not cross".PrintToConsole();
                    res.Add(l);
                    lpt1++;
                    continue;
                }
                //case 2 部分2当前线段被完全包含在当前线段 2情况
                if (r.f >= l.f && r.t <= l.t)
                {
                    $"fully contains 2".PrintToConsole();
                    if (r.height <= l.height)
                    {
                        lpt2++;
                    }
                    else
                    {
                        visit1[lpt1] = true;
                        res.Add((l.f, r.f ,l.height));
                        res.Add((r.f, r.t, r.height));
                        left[lpt1] = (r.t, l.t, l.height);
                        lpt2++;
                    }
                    continue;
                }
                if (r.f <= l.f && r.t >= l.t)
                {
                    if (r.height <= l.height)
                    {
                        lpt1++;
                    }
                    else
                    {
                        visit1[lpt2] = true;
                        res.Add((r.f, l.f ,r.height));
                        res.Add((l.f, l.t, l.height));
                        left[lpt2] = (l.t, r.t, r.height);
                        lpt2++;
                    }
                    continue;
                }
                //case 3 包含一部分 2情况
                if (r.f <= l.t && r.t >= l.t)  //lf rf lt rt
                {
                    $"partial contain L,R".PrintToConsole();
                    if (r.height <= l.height)
                    {
                        $"add {l.f},{l.t},{l.height}".PrintToConsole();
                        $"add {l.t},{r.t},{r.height}".PrintToConsole();
                        res.Add((l.f, l.t, l.height));
                        res.Add((l.t, r.t, r.height));
                        lpt1++;
                        lpt2++;
                    }
                    else
                    {
                        //visit1[lpt1] = true;
                        res.Add((l.f, r.f ,l.height));
                        res.Add((r.f, r.t, r.height));
                        //left[lpt1] = (l.t, r.f, l.height);
                        lpt1++;
                        lpt2++;
                    }
                    continue;
                }
                if (r.f <= l.f && r.t >= l.f && l.t >= r.t) //r.f l.f r.t l.t
                {
                    $"partial contain R,L".PrintToConsole();
                    if (l.height <= r.height)
                    {
                        res.Add((r.f, r.t, r.height));
                        res.Add((r.t, r.t, l.height));
                        lpt1++;
                        lpt2++;
                    }
                    else
                    {
                        //visit1[lpt1] = true;
                        res.Add((r.f, l.f ,r.height));
                        res.Add((l.f, l.t, l.height));
                        //left[lpt1] = (l.t, r.f, l.height);
                        lpt1++;
                        lpt2++;
                    }
                    continue;
                }
                
            }
            while(lpt1 < left.Count)
                res.Add(left[lpt1++]);
            while(lpt2 < right.Count)
                res.Add(right[lpt2++]);
            $"Conquor res = {res.ToEnumerationString()}\n".PrintToConsole();
            return res;
        }
        
    }
}