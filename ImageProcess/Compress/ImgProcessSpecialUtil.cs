using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;
using CIExam.Structure;

namespace CIExam.ImageProcess.Compress
{
    public class ImgProcessSpecialUtil
    {

        public static void EdgeDegenerate(GreyImage image, int r = 1)
        {
            image.PixelMatrix.Padding(r,r,r,r);
            var edge = ImageProcessUtils.GetEdgePoints(image).edge;
            FunctionExt.ElementInvoke(edge, e =>
            {
                var (x, y) = e;
                var t = image.PixelMatrix[x + r, y + r];
                image.PixelMatrix[(x - r + r)..(x + r + r), (x - r + r)..(x + r + r)] = (0.0).CastToMatrix();
                image.PixelMatrix[x + r, y + r] = t;
            });
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteRow(0);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteRow(image.PixelMatrix.RowsCount - 1);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteCol(0);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteCol(image.PixelMatrix.ColumnsCount - 1);
        }
        public static void EdgeExtend(GreyImage image, int r = 1)
        {
            image.PixelMatrix.Padding(r,r,r,r);
            var edge = ImageProcessUtils.GetEdgePoints(image).edge;
            FunctionExt.ElementInvoke(edge, e =>
            {
                var (x, y) = e;
                var t = image.PixelMatrix[x + r, y + r];
                image.PixelMatrix[(x - r + r)..(x + r + r), (x - r + r)..(x + r + r)] = (1.0).CastToMatrix();
                
            });
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteRow(0);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteRow(image.PixelMatrix.RowsCount - 1);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteCol(0);
            for (var i = 0; i < r; i++)
                image.PixelMatrix.DeleteCol(image.PixelMatrix.ColumnsCount - 1);
        }
        //8个bit平面
        public static IEnumerable<Matrix<byte>> GetBitAspects(Matrix<int> img)
        {
            var bitAspect = Enumerable.Range(0, 7).Select(b =>  img.CastToMatrix(e =>
                (byte)(Convert.ToString(e, 2).PadLeft(8,'0')[b] - '0'))
            ).ToArray();

            return bitAspect;
        }
        
        public static Matrix<dynamic> MedianFilter<T>(Matrix<T> matrix, int wSize) where T: IComparable
        {
            var counter = new List<T>();
            var r = matrix.RowsCount;
            var c = matrix.ColumnsCount;
            if (wSize > r || wSize > c)
                return matrix.SelectMany(e => e).Average(e => (dynamic)e).CastToMatrix();
            
            //hist。如果需要处理非整形数据的话，最好用哈希表惹。。。不过复杂度会上升
            var hist = new SortedList<T, int>(); //内部是改造过的红黑树


            //由于是用hash表实现的。复杂度比较难确定。但是循环次数不会超过r^2
            //在值域有限的情况下（如0-256），复杂度为常数
            
            //要找第K顺序统计量的话只需要稍微更改。
            T GetMedian()
            {
                if (wSize % 2 == 1)
                {
                    var thresh = wSize * wSize / 2;
                    var s = 0;
                    foreach (var p in hist)
                    {
                        s += p.Value;
                        if (s > thresh)
                            return p.Key;
                    }

                    return default;
                }
                else
                {
                    var t1 = wSize * wSize / 2 - 1;
                    var t2 = wSize * wSize/ 2;
                    var s = 0;
                    var num1Get = false;
                    T num1 = default;
                    foreach (var p in hist)
                    {
                        s += p.Value;
                        if (s > t1 && !num1Get)
                        {
                            num1 = p.Key;
                            num1Get = true;
                        }
                        if (s > t2)
                            return ((dynamic)num1 + p.Key) / 2;
                    }

                    return default;
                }
                
            }
            
            var ans = new List<dynamic>();
            
            //总体 O(nm(r + f)) ,n >> r 值域无限下最坏情况 O(nmr^2)，有限情况下O(nmr)
            //使用log n查找结构，在值域无限下，可以达到O(nmrlog r)
            for (var i = 0; i + wSize - 1 < r; i++)
            {
                //O(r^2 + (n - 1)*2r + n*f) = O(r^2 + nr)。n >> r时，为O(n(r+f)) 其中f为中位数求解复杂度。
                for (var j = 0; j + wSize - 1< c; j++)
                {
                    //每行需要初始化
                    if (j == 0)
                    {
                        "newRow init".PrintToConsole();
                        //init
                        hist.Clear();
                        for (var bi = i; bi < i + wSize; bi++)
                        {
                            for (var bj = j; bj < j + wSize; bj++)
                            {
                                if (hist.ContainsKey(matrix[bi, bj]))
                                {
                                    hist[matrix[bi, bj]]++;
                                }
                                else
                                {
                                    hist[matrix[bi, bj]] = 1;
                                }
                            }
                        }
                        hist.PrintEnumerationToConsole();
                       
                    }
                    else
                    {
                        //update
                        ////delete
                        for (var k = 0; k < wSize; k++)
                        {
                            var t = matrix[i + k, j - 1];
                            hist[t]--;
                            if (hist[t] == 0)
                            {
                                hist.Remove(t);
                            }
                        }
                        ////add
                        for (var k = 0; k < wSize; k ++)
                        {
                            var t = matrix[i + k, j + wSize - 1];
                            if (hist.ContainsKey(t))
                            {
                                hist[t]++;
                            }
                            else
                            {
                                hist[t] = 1;
                            }
                        }
                        hist.PrintEnumerationToConsole();
                    }
                    ans.Add(GetMedian());
                    //$"Add {GetMedian()}".PrintToConsole();
                }
            }
            return ans.ToMatrix(r - wSize/2, c - wSize/2);
        }
        
        public IEnumerable<T> GetSpiralOrderEnumerator<T>(Matrix<T> matrix) {
            var r = matrix.RowsCount;
            if(r == 0)
                return new List<T>();
            var c = matrix.ColumnsCount;
            var t = (int)System.Math.Min(System.Math.Ceiling(r / 2.0), System.Math.Ceiling(c / 2.0));
            var res = new List<T>();
            void Search(int d){
                var (sx, sy) = (d, d);
                var l1 = c - 2 * d;
                var l2 = r - 2 * d - 1;
                var l3 = c - 2 * d - 1; //!
                var l4 = r - 2 * d - 2;
            
                for(var i = 0; i < l1; i++){
                    res.Add(matrix[sx, sy + i]);
                }
                for(var i = 0; i < l2 && l1 != 0; i++){
                    res.Add(matrix[sx + i + 1, sy + l1 - 1]);
                }
                for(var i = 0; i < l3 && l2 != 0; i++){
                    res.Add(matrix[sx + l2, sy + l1 - i - 2]);
                }
                for(var i = 0; i < l4 && l3 != 0; i++){
                    res.Add(matrix[sx + l2 - 1 - i, sy]);
                }
            }

            for(var i = 0; i < t; i++){
                Search(i);
            
            }
       
            return res.ToArray();
        }

        public static IEnumerable<T> GetSpiralEnumerable<T>(Matrix<T> matrix)
        {
            IEnumerable<T> Helper(Matrix<T> mat)
            {
                var m = mat.RowsCount;
                var n = mat.ColumnsCount;
                if (m == 0 || n == 0)
                    return Array.Empty<T>();
                if (m == 1)
                    return mat[0];
                if (n == 1)
                    return mat.ColumnsEnumerator.First();
                var ans = new List<T>();
                ans.AddRange(mat[0]); //首行
                ans.AddRange(mat.ColumnsEnumerator.Last().Skip(1)); //最右侧
                ans.AddRange(mat.Last().Take(n - 1).Reverse()); //底部
                ans.AddRange(mat.ColumnsEnumerator.First().Skip(1).Take(m - 2).Reverse()); //左侧
                ans.AddRange(Helper(mat.Skip(1).Take(m - 2).Select(l => l.Skip(1).Take(n - 2)).ToMatrix2D())); //递归
                //如果是给定n，生成对应螺旋矩阵，也是按照类似递归
                return ans;
            }


            return Helper(matrix);
        }
        /*
         * class Solution {
public:
    vector<vector<int>> rotateGrid(vector<vector<int>>& grid, int k) {
        rotate(grid,0,0,k);
        return grid;
    }
    
    void rotate(vector<vector<int>>& grid, int i, int j, int k)
    {
        int m = grid.size(), n = grid[0].size();
        // 结束条件
        if(i==min(m/2,n/2)) return;
        // 计算一层的元素总数，用以减小k值
        int total = (m-i*2)*2+(n-j*2)*2-4;
        for(int l=0;l<k%total;l++) {
            // 执行一次轮转
            int t = grid[i][j];
            for(int k=j;k<n-1-j;k++)
                grid[i][k] = grid[i][k+1];
            for(int k=i;k<m-1-i;k++)
                grid[k][n-1-j] = grid[k+1][n-1-j];
            for(int k=n-1-j;k>j;k--)
                grid[m-1-i][k] = grid[m-1-i][k-1];
            for(int k=m-1-i;k>i+1;k--)
                grid[k][j] = grid[k-1][j];
            grid[i+1][j]=t;
        }
        // 轮转里面的一层
        rotate(grid,i+1,j+1,k);
    }
};

         */

        public static IEnumerable<IEnumerable<T>> GetZEnumerable<T>(Matrix<T> matrix)
        {
            
            var (x, y) = (x: 0, y: 0);
            var list = new List<T> {matrix[0, 0]};
            var r = matrix.RowsCount;
            var c = matrix.ColumnsCount;
            var state = 0; // 0 - 左下走 1 -右上走
           
            var ans = new List<List<T>>();
            void LayerFinishTrigger() {ans.Add(new List<T>(list)); list.Clear();}
            while (x != r - 1 || y != c - 1)
            {
                if (state == 0)
                {
                    var nx = x + 1;
                    var ny = y - 1;
                    if (ny < 0 && nx >= r)
                    {
                        y = 1;
                        state = 1;
                        LayerFinishTrigger();
                    }
                    else if (ny < 0)
                    {
                        x = nx;
                        state = 1;
                        LayerFinishTrigger();
                    }else if (nx >= r)
                    {
                        y = y + 1;
                        state = 1;
                        LayerFinishTrigger();
                    }
                    else
                    {
                        x = nx;
                        y = ny;
                    }
                    list.Add(matrix[x, y]);
                }else if (state == 1)
                {
                    var nx = x - 1;
                    var ny = y + 1;
                    if (nx < 0 && ny >= c)
                    {
                        x = 1;
                        state = 0;
                        LayerFinishTrigger();
                    }
                    else if (nx < 0)
                    {
                        y = ny;
                        state = 0;
                        LayerFinishTrigger();
                    }else if (ny >= c)
                    {
                        x = x + 1;
                        state = 0;
                        LayerFinishTrigger();
                    }
                    else
                    {
                        x = nx;
                        y = ny;
                    }
                    list.Add(matrix[x, y]);
                }
            }

            if (list.Any())
            {
                ans.Add(new List<T>(list));
            }

            return ans;
        }
    }
}