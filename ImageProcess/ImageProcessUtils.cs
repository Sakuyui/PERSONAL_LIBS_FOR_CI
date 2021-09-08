using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using CIExam.InformationThoery;
using CIExam.Math;
using CIExam.Network;
using CIExam.Structure;
using CIExam.FunctionExtension;
using CIExam.Geometry;

namespace CIExam.ImageProcess
{
    public enum ConnectedType
    {
        EightConnect,
        FourConnectLrud
    }
    public static class ImageProcessUtils
    {
        
        public static Matrix<double> WalshTransform(Matrix<int> mat)
        {
            var n = mat.RowsCount;
            var kernel = Cdma.GenerateWalshCode(n).ToMatrix2D();
            var wht = (kernel * mat * kernel).CastToMatrix(e => (double) (int) e / (n * n));
            
            //wht.PrintToConsole();
            //(kernel * wht.CastToMatrix(e => (int)e) * kernel).PrintToConsole();
            
            return wht;
        }
        public static Dictionary<T, int> GetHistogramAdvance<T>(IEnumerable<T> elements)
        {
            var histogram = new Dictionary<T, int>();
            //计算直方图
            var enumerable = elements as T[] ?? elements.ToArray();
            var pixelCount = enumerable.Length;
            foreach (var c in enumerable)
            {
                if (histogram.ContainsKey(c))
                {
                    histogram[c] = histogram[c] + 1;
                }
                else
                {
                    histogram[c] = 1;
                }
            }

            return histogram;
        }
        
        public static int[] GetHistogram(IEnumerable<int> image, int rangeL = 0, int rangeR = 255)
        {
            var histogram = new int[rangeR - rangeL + 1];
            //计算直方图
            var pixelCount = image.Count();
            foreach (var c in image)
            {
                histogram[c - rangeL]++;
            }

            return histogram;
        }

        public static double[] GetCdf(IEnumerable<int> distribution, int rangeL = 0, int rangeR = 255)
        {
            var hist = GetHistogram(distribution, rangeL, rangeR);
            var count = hist.Length;
            var cdf = new double[rangeR - rangeL + 1];
            for (var i = rangeL; i <= rangeR; i++)
            {
                var sum = 0;
                for (int j = 0; j <= i; j++)
                    sum += hist[j];
                cdf[i] = sum * 1.0 / count;
            }

            return cdf;
        }
        public static List<int> HistogramSpecification(IEnumerable<int> input, float[] zHist,  int rangeL = 0, int rangeR = 255)
        {
            var src = input.ToArray();
            var dst = new List<int>(src);

            var count = src.Length;

            var hist = GetHistogram(src);
            //各级像素数目
            var s  =  new double[rangeR - rangeL + 1];
            
            var s2Z = new Dictionary<int, int>(); //S到Z的映射
            var r2Z = new Dictionary<int, int>(); //r到Z的映射
            

            //归一化累加直方图
            var cdf = GetCdf(src);
            

            //根据sumhist建立均衡化后灰度级数组S
            for (var i = rangeL; i <= rangeR; i++)
                s[i - rangeL] = (rangeR - rangeL) * cdf[i - rangeL] + 0.5;

            //根据zsumhist建立均衡化后灰度级数组G
            var g = new double[256];
            var cdf2 = new double[256];

            
            for (var i = rangeL; i <= rangeR; i++)
            {
                float sum = 0;
                for (var j = 0; j <= i; j++)
                    sum += zHist[j];
                cdf2[i] = sum;
            }

            for (var i = 0; i < 256; i++)
                g[i] = cdf2[i] * (rangeR - rangeL) + 0.5; //查找映射

            //令G(Z)=S 建立S->Z的映射表
            for (var i = 0; i < 256; i++)
            {
                for (var j = 1; j < 256; j++)
                {
                    if (!(System.Math.Abs(s[i] - g[j - 1]) < System.Math.Abs(s[i] - g[j]))) 
                        continue;
                    
                    s2Z[(int)s[i]] = j - 1;
                    break;
                }
            }

            s2Z[(int)s[rangeR]] = (int)g[rangeR];

            //建立r->z映射，即为原像素到规定化像素的映射
            for (var i = 0; i < 256; i++)
                r2Z[i] = s2Z[(int)s[i]];

            //重建图像
            for (var i = 0; i < count; i++)
            {
                dst[i] = r2Z[src[i]];
            }

            return dst;
        }

        public static int[] EqualizeHistogram(IEnumerable<int> sourceDistribution, int rangeL = 0, int rangeR = 255)
        {
            // 计算直方图
            var distribution = sourceDistribution as int[] ?? sourceDistribution.ToArray();
            var h1 = GetHistogram(distribution, rangeL, rangeR);
            IEnumerable<int> dstDistribution;
            // 计算分布函数(也就是变换函数f(x))
            var numberOfPixel = distribution.Length;
            var lut = new int[256];
            lut[0] = (int)(1.0 * h1[0] / numberOfPixel * (rangeR - rangeL));
            var sum = h1[0];
            
            for (var i = 0; i <= 255; ++i)
            {
                sum += h1[i];
                lut[i] = (int)(1.0 * sum / numberOfPixel * (rangeR - rangeL));
            }

            // 灰度变换
            var dataOfDst = new int[distribution.Count()];
            for (var i = 0; i <= numberOfPixel - 1; ++i)
                dataOfDst[i] = lut[distribution[i]];
            return dataOfDst;
        }

        public static void LsbWaterMark(GreyImage image, GreyImage waterMark)
        {
            var w = image.Width;
            var h = image.Height;
            var ww = waterMark.Width;
            var wh = waterMark.Height;
            if (w * h < 8 * ww * wh)
            {
                throw new Exception("watermark size should <= 1/8 source img's w * h");
            }
            var pos = 0;

            void WriteByte(byte b)
            {
                
                var bs = Convert.ToString(b, 2).PadLeft(8,'0' );
                for (var i = 0; i < 8; i++)
                {
                    var x = pos / w;
                    var y = pos % w ;
                    
                    if (bs[i] == 1)
                        image.PixelMatrix[x, y] = (byte)(int)image.PixelMatrix[x, y] | 1;
                    else
                        image.PixelMatrix[x, y] = (byte)(int)image.PixelMatrix[x, y] & (~0 << 1);
                    pos++;
                }
            }
            
            for (var i = 0; i < wh; i++)
            {
                for (var j = 0; j < ww; j++)
                {
                    var curPixel = waterMark[i, j];
                    WriteByte((byte)(int)curPixel.Color);
                }
            }
        }
        public static void LsbWaterMark(RgbImage image, RgbImage waterMark)
        {
            var w = image.Width;
            var h = image.Height;
            var ww = waterMark.Width;
            var wh = waterMark.Height;
            if (w * h < 8 * ww * wh)
            {
                throw new Exception("watermark size should <= 1/8 source img's w * h");
            }
            var pos = 0;

            void WriteByte(byte b)
            {
                
                var bs = Convert.ToString(b, 2).PadLeft(8,'0' );
                for (var i = 0; i < 8; i++)
                {
                    var x = pos / (w * 3);
                    var y = pos % (w * 3) / 3;
                    var d = pos % (w * 3) % 3;
                    if (bs[i] == 1)
                        image.Mat[d][x, y] = (byte)image.Mat[d][x, y] | 1;
                    else
                        image.Mat[d][x, y] = (byte)image.Mat[d][x, y] & (~0 << 1);
                    pos++;
                }
            }
            for (var i = 0; i < wh; i++)
            {
                for (var j = 0; j < ww; j++)
                {
                    var curPixel = waterMark[i, j];
                    WriteByte((byte)curPixel.Color.R);
                    WriteByte((byte)curPixel.Color.G);
                    WriteByte((byte)curPixel.Color.B);
                }
            }
        }

        public static void TestLabeling()
        {
            var labeledImg = Labeling(new double[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 1, 0, 0, 0, 0,
                0, 1, 1, 1, 1, 0, 1, 0,
                0, 1, 0, 1, 0, 1, 1, 0,
                0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 1, 0, 1, 0,
                0, 0, 0, 0, 1, 0, 1, 0,
                0, 0, 0, 0, 0, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0,
            }.ToMatrix(9, 8));
            labeledImg.PrintToConsole();
            labeledImg.mat.SelectMany(e => e.Select(e2 => e2 == 'A' ? 'A' : ' ')).ToMatrix(9 ,8).PrintToConsole();
            
            labeledImg.mat.MatrixFind(e => e == 'A').PrintCollectionToConsole();
            var t2 = labeledImg.mat[e => e == 'A']; //bool矩阵
            labeledImg.mat[t2] = 'A';
            labeledImg.PrintToConsole();
            var wtf = ImageProcessUtils
                .WalshTransform(new[] {1, 3, 3, 1, 1, 3, 3, 1, 1, 3, 3, 1, 1, 3, 3, 1}.ToMatrix(4, 4));
            wtf.PrintToConsole();
            var walshCode = Cdma.GenerateWalshCode(4).ToMatrix2D();
            //decode
            $"decode = \n {walshCode * wtf.CastToMatrix(e => (int)e) * walshCode}".PrintToConsole();
            "lzw 编码后:".PrintToConsole();
            var (code, dict) = LzwCoder.LzwEncode(wtf.SelectMany(e => e));
            $"lzw code = {code.ToEnumerationString()}".PrintToConsole();
            "lzss编码后".PrintToConsole();
            var res = LzssCoder.EnCode(wtf.SelectMany(e => e));
            res.PrintEnumerationToConsole();
            LzssCoder.DeCode(res).PrintEnumerationToConsole();
            InformaticThoery.RunLenCoder.CommonEnCode(wtf.SelectMany(e => e).ToArray()).PrintToConsole();
            
            wtf.PrintToConsole();
        }

        //双DP
        public static int[][] Labeling01MatrixDistanceDP(int[][] matrix)
        {
            int m = matrix.Length, n = matrix[0].Length;
            var dp = new int[m].Select(_ => new int[n]).ToArray();
            for (var i = 0; i < m; i++) {
                for (var j = 0; j < n; j++) {
                    dp[i][j] = matrix[i][j] == 0 ? 0 : 10000;
                }
            }

            // 从左上角开始
            for (var i = 0; i < m; i++) {
                for (var j = 0; j < n; j++) {
                    if (i - 1 >= 0) {
                        dp[i][j] = System.Math.Min(dp[i][j], dp[i - 1][j] + 1);
                    }
                    if (j - 1 >= 0) {
                        dp[i][j] = System.Math.Min(dp[i][j], dp[i][j - 1] + 1);
                    }
                }
            }
            // 从右下角开始
            for (var i = m - 1; i >= 0; i--) {
                for (var j = n - 1; j >= 0; j--) {
                    if (i + 1 < m) {
                        dp[i][j] = System.Math.Min(dp[i][j], dp[i + 1][j] + 1);
                    }
                    if (j + 1 < n) {
                        dp[i][j] = System.Math.Min(dp[i][j], dp[i][j + 1] + 1);
                    }
                }
            }
            return dp;
        

        }

        public static int[][] Labeling01MatrixDistance(int[][] matrix) {
            // 首先将 0 边上的 1 入队
            var dx = new[] {-1, 1, 0, 0};
            var dy = new[] {0, 0, -1, 1};
            var queue = new Queue<int[]>();
            int m = matrix.Length, n = matrix[0].Length;
            var res = new int[m].Select(_ => new int[n]).ToArray();
            //第一次BFS找到边缘的“1”点。入队
            for (var i = 0; i < m; i++) {
                for (var j = 0; j < n; j++)
                {
                    if (matrix[i][j] != 0) continue;
                    for (var k = 0; k < 4; k++) {
                        var x = i + dx[k];
                        var y = j + dy[k];
                        if (x < 0 || x >= m || y < 0 || y >= n || matrix[x][y] != 1 || res[x][y] != 0) continue;
                        // 这是在 0 边上的1。需要加上 res[x][y] == 0 的判断防止重复入队
                        res[x][y] = 1;
                        queue.Enqueue(new [] {x, y});
                    }
                }
            }

            //从边缘点BFS，更新距离
            while (queue.Any()) {
                var point = queue.Dequeue();
                int x = point[0], y = point[1];
                for (var i = 0; i < 4; i++) {
                    var newX = x + dx[i];
                    var newY = y + dy[i];
                    if (newX < 0 || newX >= m || newY < 0 || newY >= n || matrix[newX][newY] != 1 ||
                        res[newX][newY] != 0) continue;
                    res[newX][newY] = res[x][y] + 1;
                    queue.Enqueue(new[] {newX, newY});
                }
            }

            return res;
        }

        public static (Matrix<char> mat, Dictionary<char, List<(int x, int y)>> data) Labeling(GreyImage image)
        {
            //image.PixelMatrix.PrintToConsole();
            var mat = image.PixelMatrix;
            var visited = new Matrix<bool>(mat.RowsCount, mat.ColumnsCount);

            var dictionary = new Dictionary<char, List<(int x, int y)>>();
            var row = mat.RowsCount;
            var col = mat.ColumnsCount;
            var time = 0;
            var ans = new Matrix<char>(row, col);
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    if (!visited[i, j] && mat[i, j] != 0)
                    {
                        var set = new List<(int x, int y)>();
                        
                        //bfs一趟
                        FillBfs(mat, ans, (char)('A' + time), i, j, visited, set);
                        set = set.OrderBy(e => e.x).ThenBy(e => e.y).ToList();
                        dictionary[(char) ('A' + time++)] = set;
                    }
                }
            }

            return (ans, dictionary);
        }

        private static void FillBfs(Matrix<double> sourceMat, Matrix<char> outPutMatrix, char fill, int x, int y, Matrix<bool> vis
            , List<(int x, int y)> set)
        {
            var q = new Queue<(int px, int py)>();
            q.Enqueue((x, y));
            var direction = new (int ox, int oy)[4] {(0, -1), (0, 1), (1, 0), (-1, 0)};
            while (q.Any())
            {
                var (px, py) = q.Dequeue();
                if(vis[px, py])
                    continue;
                vis[px, py] = true;
                set.Add((px, py));
                outPutMatrix[px, py] = fill;
                foreach (var (ox, oy) in direction)
                {
                    if (px + ox >= 0 && px + ox < vis.RowsCount && py + oy >= 0 && py + oy < vis.ColumnsCount 
                        && !vis[px + ox, py + oy] && sourceMat[px + ox, py+oy] != 0)
                    {
                        q.Enqueue((px + ox, py + oy));
                    }
                }
            }
        }
        public static List<Matrix<int>> GetBitAspect(Matrix<int> matInput)
        {
            var r = matInput.RowsCount;
            var c = matInput.ColumnsCount;
            var mat = matInput.ElementEnumerator.Select(e => Utils.ToBinaryNumber(e, 8))
                .ToMatrix(r, c);
            var ans = new List<Matrix<int>>();
            for (var i = 0; i < 8; i++)
            {
                var i1 = i;
                var asp = mat.ElementEnumerator.Select(e => e[i1] - '0').ToMatrix(r, c);
                ans.Add(asp);
            }
            return ans;
        }
        private static void T1()
        {
            var mat = new Matrix<int>(5, 5, (i, i1, i2) => new Random().Next(0, 255));
            mat.PrintToConsole();

            var mat2 = mat.ElementEnumerator.Select(e => Utils.ToBinaryNumber(e, 8)).ToMatrix(5, 5);
            mat2.PrintToConsole();
            
            GetBitAspect(mat);
        }

        public static bool IsConnected(GreyImage image, double threshHold, int sx, int sy, int tx, int ty)
        {
            var mat = (Matrix<double>)image.PixelMatrix.Clone();
            var zeroOneImage =
                mat.ElementEnumerator.Select(e => e >= threshHold ? 1 : 0)
                    .ToArray().ToMatrix(mat.RowsCount, mat.ColumnsCount);
            zeroOneImage.PrintToConsole();
            var visit = new bool[mat.RowsCount, mat.ColumnsCount];
            //bfs
            var q = new Queue<(int x, int y)>();
            q.Enqueue((sx, sy));
            var offset = new (int offsetX, int offsetY)[] {(0, -1), (0, 1), (1, 0), (-1, 0)}; //4连通域
            while (q.Any())
            {
                var f = q.Dequeue();
                if (f.x == tx && f.y == ty)
                    return true;
                visit[f.x, f.y] = true;
                var ext = offset.Where(o => {
                    var x = f.x + o.offsetX;
                    var y = f.y + o.offsetY;
                    if (x < 0 || x >= mat.RowsCount || y < 0 || y > mat.ColumnsCount)
                        return false;
                    return System.Math.Abs(mat[x, y] - 1) < double.Epsilon && !visit[x, y];
                });
                foreach (var e in ext)
                {
                    q.Enqueue((f.x + e.offsetX, f.y + e.offsetY));
                }
            }
            return false;
        }

        public static UnionFindTree GetConnectedAreaByDisJointSet(GreyImage image, double threshHold)
        {
            var mat = (Matrix<double>)image.PixelMatrix.Clone();
            var zeroOneImage =
                mat.ElementEnumerator.Select(e => e >= threshHold ? 1 : 0)
                    .ToArray().ToMatrix(mat.RowsCount, mat.ColumnsCount);
            zeroOneImage.PrintToConsole();
            var ans = new List<List<(int x, int y)>>();
            var set = new UnionFindTree(mat.ColumnsCount * mat.RowsCount);
            
            var visit = new bool[mat.RowsCount, mat.ColumnsCount];
            //bfs
            var q = new Queue<(int x, int y)>();
           
            var offset = new (int offsetX, int offsetY)[] {(0, -1), (0, 1), (1, 0), (-1, 0)}; //4连通域

            int OneDimensionCoordination(int x, int y)
            {
                return x * mat.RowsCount + y;
            }
            for (var i = 0; i < image.Height; i++)
            {
                for (var j = 0; j < image.Width; j++)
                {
                   if(visit[i, j] || mat[i, j] == 0)
                       continue;
                   var (sx, sy) = (i, j);
                   q.Enqueue((sx, sy));
                   
                   //bfs
                   while (q.Any())
                   {
                       var f = q.Dequeue();
                       visit[f.x, f.y] = true;
                       set.Merge(OneDimensionCoordination(sx, sy), OneDimensionCoordination(f.x, f.y));
                       
                       var ext = offset.Where(o => {
                           var x = f.x + o.offsetX;
                           var y = f.y + o.offsetY;
                           if (x < 0 || x >= mat.RowsCount || y < 0 || y > mat.ColumnsCount)
                               return false;
                           return System.Math.Abs(mat[x, y] - 1) < double.Epsilon && !visit[x, y];
                       });
                       foreach (var e in ext)
                       {
                           q.Enqueue((f.x + e.offsetX, f.y + e.offsetY));
                       }
                   }
                }
            }
            return set;
        }
        public static List<List<(int x, int y)>> GetConnectedArea(GreyImage image, double threshHold)
        {
            var mat = (Matrix<double>)image.PixelMatrix.Clone();
            var zeroOneImage =
                mat.ElementEnumerator.Select(e => e >= threshHold ? 1 : 0)
                    .ToArray().ToMatrix(mat.RowsCount, mat.ColumnsCount);
            zeroOneImage.PrintToConsole();
            var ans = new List<List<(int x, int y)>>();
            for (var i = 0; i < image.Height; i++)
            {
                for (var j = 0; j < image.Width; j++)
                {
                    if (zeroOneImage[i, j] != 1)
                    {
                        var tmpArea = new HashSet<Vector<int>>();
                        GetConnectedAreaDfs(zeroOneImage, i, j, tmpArea);
                        ans.Add(tmpArea.Select(e => (e[1], e[2])).ToList());
                        foreach (var a in ans)
                        {
                            a.PrintEnumerationToConsole();
                        }
                    }
                }
            }
            return ans;
        }

        private static void GetConnectedAreaDfs(Matrix<int> matrix, int x, int y, ISet<Vector<int>> path)
        {
            //默认上下左右dfs
            var h = matrix.RowsCount;
            var w = matrix.ColumnsCount;
            
            //邻域之类的利用查找表遍历很方便
            var direction = new[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };
            
            if(y >= w || x >= h || x < 0 || y < 0)
                return;
            
            if (matrix[x, y] != 1)
            {
                matrix[x, y] = 1;
                path.Add(new Vector<int>(2, x, y));
                foreach (var (offsetX, offsetY) in direction)
                {
                    GetConnectedAreaDfs(matrix, x + offsetX, y + offsetY, path);
                }
            }
        }


        //这个式子的意思就是用x和x0，x1的距离作为一个权重，用于y0和y1的加权。双线性插值本质上就是在两个方向上做线性插值。
        // public static Matrix<int> InterpolationLinear(Matrix<int> img, int height, int width)
        // {
        //     var img2 = new Matrix<int>(height, width);
        //               
        //     
        //     var pSet = new HashSet<ValueTupleSlim>();
        //     for (var i = 0; i < img.RowsCount; i++)
        //     {
        //         for (var j = 0; j < img.ColumnsCount; j++)
        //         {
        //             var x = i * (height / img.RowsCount);
        //             var y = j * (width / img.ColumnsCount);
        //             img2[x, y] = img[i, j];
        //             pSet.Add(new ValueTupleSlim(x, y));
        //         }
        //     }
        //     
        //     //双线性插值
        //     for (var i = 0; i < height; i++)
        //     {
        //         for (var j = 0; j < width; j++)
        //         {
        //             if(pSet.Contains(new ValueTupleSlim(i, j)))
        //                 //(double)((i - e.Data[0]) * (i - e.Data[0])) + (double)((j - e.Data[1]) * (j - e.Data[1]))
        //                 continue;
        //             //$"interpolate {i},{j} => ".PrintToConsole();
        //
        //             var fourPoints = Utils.QuickSelection(pSet.ToArray(), 0, pSet.Count - 1, 4,
        //                 new CustomerComparer<ValueTupleSlim>(delegate(ValueTupleSlim t1, ValueTupleSlim t2)
        //                 {
        //                     double Dist(ValueTupleSlim t) =>
        //                         (double) ((i - t[0]) * (j - t[0])) + (double) ((i - t[1]) * (j - t[1]));
        //
        //                     return Dist(t1).CompareTo(Dist(t2));
        //                 })).Item2.Take(4).OrderBy(t => (int)t[0]).ThenBy(t => (int)t[1]).ToArray();
        //             
        //              var p1 = (img2[fourPoints[0][0], fourPoints[0][1]] + img2[fourPoints[2][0], fourPoints[2][1]] ) / 2.0;
        //              var p2 = (img2[fourPoints[1][0], fourPoints[1][1]] + img2[fourPoints[3][0], fourPoints[3][1]] ) / 2.0;
        //
        //
        //             img2[i, j] = (int)((p1 + p2) / 2.0);
        //         }
        //     }
        //     
        //     return img2;
        // }

        /*双三次
         int main(){
    Mat image = imread("lena.jpg");//源图像

    float Row_B = image.rows*2;
    float Col_B = image.cols*2;


    Mat biggerImage(Row_B, Col_B, CV_8UC3);

    for (int i = 2; i < Row_B-4; i++){
        for (int j = 2; j < Col_B-4; j++){
            float x = i*(image.rows / Row_B);//放大后的图像的像素位置相对于源图像的位置
            float y = j*(image.cols / Col_B);

          
                float w_x[4], w_y[4];//行列方向的加权系数
                getW_x(w_x, x);
                getW_y(w_y, y);

                Vec3f temp = { 0, 0, 0 };
                for (int s = 0; s <= 3; s++){
                    for (int t = 0; t <= 3; t++){
                        temp = temp + (Vec3f)(image.at<Vec3b>(int(x) + s - 1, int(y) + t - 1))*w_x[s] * w_y[t];
                    }
                }

                biggerImage.at<Vec3b>(i, j) = (Vec3b)temp;
            }
        }

    imshow("image", image);
    imshow("biggerImage", biggerImage);
    waitKey(0);

    return 0;
}

void getW_x(float w_x[4],float x){
    int X = (int)x;
    float stemp_x[4];
    stemp_x[0] = 1 + (x - X);
    stemp_x[1] = x - X;
    stemp_x[2] = 1 - (x - X);
    stemp_x[3] = 2 - (x - X);

    w_x[0] = a*abs(stemp_x[0] * stemp_x[0] * stemp_x[0]) - 5 * a*stemp_x[0] * stemp_x[0] + 8 * a*abs(stemp_x[0]) - 4 * a;
    w_x[1] = (a + 2)*abs(stemp_x[1] * stemp_x[1] * stemp_x[1]) - (a + 3)*stemp_x[1] * stemp_x[1] + 1;
    w_x[2] = (a + 2)*abs(stemp_x[2] * stemp_x[2] * stemp_x[2]) - (a + 3)*stemp_x[2] * stemp_x[2] + 1;
    w_x[3] = a*abs(stemp_x[3] * stemp_x[3] * stemp_x[3]) - 5 * a*stemp_x[3] * stemp_x[3] + 8 * a*abs(stemp_x[3]) - 4 * a;
}
void getW_y(float w_y[4], float y){
    int Y = (int)y;
    float stemp_y[4];
    stemp_y[0] = 1.0 + (y - Y);
    stemp_y[1] = y - Y;
    stemp_y[2] = 1 - (y - Y);
    stemp_y[3] = 2 - (y - Y);

    w_y[0] = a*abs(stemp_y[0] * stemp_y[0] * stemp_y[0]) - 5 * a*stemp_y[0] * stemp_y[0] + 8 * a*abs(stemp_y[0]) - 4 * a;
    w_y[1] = (a + 2)*abs(stemp_y[1] * stemp_y[1] * stemp_y[1]) - (a + 3)*stemp_y[1] * stemp_y[1] + 1;
    w_y[2] = (a + 2)*abs(stemp_y[2] * stemp_y[2] * stemp_y[2]) - (a + 3)*stemp_y[2] * stemp_y[2] + 1;
    w_y[3] = a*abs(stemp_y[3] * stemp_y[3] * stemp_y[3]) - 5 * a*stemp_y[3] * stemp_y[3] + 8 * a*abs(stemp_y[3]) - 4 * a;
}
*/
        // public static Matrix<int> InterpolationNearest(Matrix<int> img, int height, int width)
        // {
        //     var img2 = new Matrix<int>(height, width);
        //               
        //     
        //     var pSet = new HashSet<ValueTupleSlim>();
        //     for (var i = 0; i < img.RowsCount; i++)
        //     {
        //         for (var j = 0; j < img.ColumnsCount; j++)
        //         {
        //             var x = i * (height / img.RowsCount);
        //             var y = j * (width / img.ColumnsCount);
        //             img2[x, y] = img[i, j];
        //             pSet.Add(new ValueTupleSlim(x, y));
        //         }
        //     }
        //     
        //     //近邻插值
        //     for (var i = 0; i < height; i++)
        //     {
        //         for (var j = 0; j < width; j++)
        //         {
        //             if(pSet.Contains(new ValueTupleSlim(i, j)))
        //                 continue;
        //             //$"interpolate {i},{j} => ".PrintToConsole();
        //             var p = pSet.ArgMin(e => 
        //                 (double)((i - e.Data[0]) * (i - e.Data[0])) + (double)((j - e.Data[1]) * (j - e.Data[1])));
        //             img2[i, j] = img2[p.Item2[0], p.Item2[1]];
        //         }
        //     }
        //     
        //     return img2;
        // }


        // public static (List<(int x, int y)> edge, List<(int x, int y, int z)> extEdge) GetInnerEdge(GreyImage img, double threshHold)
        // {
        //     //获取内部像素
        //     var imgCopy = (Matrix<double>)img.PixelMatrix.Clone();
        //     //转为二值图像
        //     imgCopy[e => e < threshHold] = 0;
        //     imgCopy[e => e >= threshHold] = 1;
        //     for (var i = 0; i < imgCopy.RowsCount; i++)
        //     {
        //         var state = 0;
        //         var c = 0;
        //         for (var j = 0; j < imgCopy.ColumnsCount; j++)
        //         {
        //             if (System.Math.Abs(img[i, j].Color - 1) < double.Epsilon)
        //             {
        //                 imgCopy[i, j] = 0;
        //                 if (state == 0)
        //                 {
        //                     state = 1;
        //                     c++;
        //                 }
        //             }
        //             else
        //             {
        //                 c++;
        //                 state = 0;
        //                 if (c == 2)
        //                     imgCopy[i, j] = 1;
        //             }
        //         }
        //     }
        //
        //     return GetEdgePoints(imgCopy);
        // }
     
        //边界处理。如果存在多个边界的话需要处理多次，每次能够获得一个边界。
        public static (List<(int x, int y)> edge, List<(int x, int y, int z)> extEdge) GetEdgePoints(GreyImage image)
        {
            var matrix = image.PixelMatrix;
            var ans = new List<(int, int)> ();
            var ext = new List<(int, int, int)> ();
            //寻找第一个左上角的点
            var start = matrix.FindFirst((e, _) => e.Any(p => p != 0));
            //不存在点
            if (start.index == -1)
                return (ans, ext);
            
            var sx = start.index;
            var sy = start.data.FindFirst((e,_) => e != 0).index;
            sy.PrintToConsole();
            var b = (x: sx, y: sy);
            var b0 = b;
            //从西边的点开始寻找
            var c = (x: sx, y: sy - 1);
            ans.Add(b);
            var step = 0;
            ext.Add((c.x, c.y, ++step));
            
            //遍历查询表
            var offsets = new (int x, int y)[]
            {
                (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1)
            };

            do
            {
                //要从指定点开始遍历，这里是求出那个点的对于偏移，从那开始顺时针。
                //我们是希望从c开始顺时针。所以需要先指定c对应在偏移数组的哪
                var b1 = b;
                var c1 = c;
                var beginIndex = offsets.FindFirst((e, _) => b1.x + e.x == c1.x && b1.y + e.y == c1.y).index;
                $"{step}: c from {b1.x + offsets[beginIndex].x},{b1.y + offsets[beginIndex].y} with offset index {beginIndex}".PrintToConsole();
                //顺时针遍历
                for (var i = 1; i <= 8; i++)
                {
                    //遍历数组看作循环数组
                    var (offsetX, offsetY) = offsets[(beginIndex + i) % 8];
                    
                    if (matrix[b.x + offsetX, b.y + offsetY] == 0) continue;
                    //发现新边界点
                    var prevOffset = offsets[(beginIndex + i - 1) % 8];
                    c = (b.x + prevOffset.x, b.y + prevOffset.y);
                    b = (b.x + offsetX, b.y + offsetY);
                    $"new edge p = {b}".PrintToConsole();
                    
                    prevOffset.PrintToConsole();
                    
                    ans.Add(b);
                    ext.Add((c.x, c.y, ++step));
                    break;
                }
            } while (b0.x != b.x || b0.y != b.y); //直到找回原点
            
            return (ans, ext);
        }

        public static GreyImage Dither(GreyImage img, int sourceBit = 8, int targetBit = 3, int percent = 20)
        {
             //dither
             var range = Enumerable.Range(0, 1 << targetBit);
             var random = new Random();
             var target = (Matrix<double>) img.PixelMatrix.Clone();
             var c = (double) (1 << targetBit) / (double) (1 << sourceBit);
             for (var i = 0; i < img.PixelMatrix.RowsCount; i++)
             {
                 for (var j = 0; j < img.PixelMatrix.ColumnsCount; j++)
                 {
                     var point = target[i, j];
                                
                     var rand = random.Next(0, 2 * percent) - percent;
                     point *= (100 + rand) / 100.0;
                     
                     var d = point * c;
                                
                     var newValue = range.ArgMin(e => System.Math.Abs(e - d)).Item2;
                     target[i, j] = newValue;
                 }
             }

             return target;
        }
    }
}