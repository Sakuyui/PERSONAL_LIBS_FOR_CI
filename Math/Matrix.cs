using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using CIExam.FunctionExtension;
using CIExam.ImageProcess;

namespace CIExam.Math
{


    
    
    
    public class Matrix<T> : ICloneable, IEnumerable<List<T>>
    {
        public List<List<T>> Data;
        public int RowsCount => Data?.Count ?? 0;

        public int ColumnsSize; //列数
        public int RowSize; //行数


        public Matrix<T> RowConcat(Matrix<T> m2)
        {
            //行连接
            return this.Concat(m2).ToMatrix2D();
        }

        public Matrix<T> ColumnConcat(Matrix<T> m2)
        {
            
            //列连接
            return ColumnsEnumerator
                .Concat(m2.ColumnsEnumerator)
                .ToMatrix2D()._T();
        }

        public bool MatrixEquals(Matrix<T> mat)
        {
            return mat.Data.Select((e, i) => (e, i)).All(e => e.e.SequenceEqual(mat.Data[e.i]));
        }
        public IEnumerable<(int, int)> MatrixFind(Func<T, bool> condition)
        {
            var t = this.FindAll(
                (e,i) => e.Any(condition.Invoke), 
                (list, i) =>
                list.FindAndGetIndexes(condition.Invoke)
                    .Select(e => (i, e))
            ).SelectMany(e => e);
            return t;
        }
        private Tuple<int, int> Shape => new Tuple<int, int>(RowsCount,ColumnsCount);


        public IEnumerable<List<T>> ColumnsEnumerator
        {
            get
            {
                for(var i = 0; i <  ColumnsCount; i++)
                {
                    var col = Data.Select(e => e[i]).ToList();
                    yield return col;
                }
            }
        }
        
        
        public IEnumerable<List<T>> RowsEnumerator
        {
            get
            {
                for(var i = 0; i <  RowSize; i++)
                {
                    yield return Data[i];
                }
            }
        }

        public IEnumerable<T> ElementEnumerator
        {
            get { return Data.SelectMany(r => r); }
        }
        
        
        public int ColumnsCount
        {
            get
            {
                if (Data == null) return 0;
                return Data.Count != 0 ? Data[0].Count : ColumnsSize;
            }
            set => ColumnsSize = value;
        }

        public Matrix(IReadOnlyCollection<T[]> data)
        {
            if(data == null) throw new Exception();
            RowSize = data.Count;
            if (RowSize == 0) ColumnsSize = 0;
            Data = new List<List<T>>();
            foreach (var t in data)
            {
                ColumnsSize = t.Length;
                Data.Add(new List<T>(t));
            }
        }
        public Matrix(IReadOnlyList<List<T>> data)
        {
            if(data == null) throw new Exception();
            RowSize = data.Count;
            if (RowSize == 0) ColumnsSize = 0;
            Data = new List<List<T>>();
            foreach (var t in data)
            {
                ColumnsSize = t.Count;
                Data.Add(new List<T>(t.ToArray()));
            }
        }

        //从向量创建矩阵
        public Matrix(IReadOnlyList<Vector<T>> vectors)
        {
            //Check
            //If all the shape of the vectors are the same. And if all vector is row/column vector.
            if (vectors == null) throw new Exception();
            if (vectors.Count == 0)
            {
                RowSize = 0;
                ColumnsSize = 0;
                Data = new List<List<T>>();
                return;
            }

            if (vectors.Count == 1)
            {
                Matrix<T> m = (Matrix<T>) vectors[0];
                Data = m.Data;
                ColumnsSize = m.ColumnsSize;
                RowSize = m.RowSize;
                return;
            }

            var x = vectors[0].Count;
            var vectorType = vectors[0].IsColumnMatrix;
            for (var i = 1; i < vectors.Count; i++)
            {
                if(x != vectors[i].Count || vectorType!= vectors[i].IsColumnMatrix)  throw new Exception();;
               
            }
            //初始化
            this.ColumnsSize = vectors[0].Count;
            this.RowSize = vectors.Count;
            Data = new List<List<T>>();
          
            
            //如果是行矩阵，将所有行添加
            for (int i = 0; i < RowSize; i++)
            {
                Data.Add(new List<T>(vectors[i].Data.ToArray())); 
            }

            if (!vectors[0].IsColumnMatrix) return;
            var matrix = this._T();
            this.Data = matrix.Data;
            this.ColumnsSize = matrix.ColumnsSize;
            this.RowSize = matrix.RowSize;


        }
        public Matrix<T> _T()
        {
            var result = new Matrix<T>(ColumnsSize, RowSize);
            for (var i = 0; i < RowSize; i++)
            {
                for (var j = 0; j < ColumnsSize; j++)
                {
                    //应该按行读取，效率高,写入不走cache无所谓
                    result[j, i] = Data[i][j];
                }
            }

            return result;
        }

        public static Matrix<T> operator <<(Matrix<T> mat1, int k)
        {
            return mat1.ColumnsEnumerator.Skip(k).Union(new int[k > mat1.ColumnsCount ? mat1.ColumnsCount : k]
                    .Select(e => new T[mat1.ColumnsCount].ToList()))
                .ToMatrix2D()._T();
        }
        public static Matrix<T> operator >>(Matrix<T> mat1, int k)
        {
            return new int[k > mat1.ColumnsCount ? mat1.ColumnsCount : k].Select(e => new T[mat1.ColumnsCount].ToList()).
                Union(mat1.ColumnsEnumerator.Skip(k)).ToMatrix2D()._T();
        }

        //ab = this?
        public bool RandomEqualTest(Matrix<T> a, Matrix<T> b)
        {
            if (a.ColumnsCount != b.RowsCount || RowsCount != a.RowsCount || ColumnsCount != b.ColumnsCount)
                return false;
            var row = RowsCount;
            var col = ColumnsCount;
            var r = new Random();
            var n = a.ColumnsCount;
            for(var k = 1; k <= 30000; k++)
            {
                row = r.Next(0, a.RowsCount);
                col= r.Next(0, b.ColumnsCount);
                var temp=0; 
                for(var i = 1; i <= n; i++)
                    temp += (dynamic)a[row, i] * (dynamic)b[i, col];
                if(!temp.Equals(this[row, col]))
                    return false;
            }
            return true;
        }
        public Matrix<T> ReverseLr()
        {
            return ColumnsEnumerator.Reverse().SelectMany(e => e).ToMatrix(RowsCount, ColumnsCount)._T();
        }
       
        public Matrix<T> ReverseTopDown()
        {
            return RowsEnumerator.Reverse().ToMatrix2D();
        }
        public Matrix(int rows, int columns, T val = default)
        {
            RowSize = rows;
            ColumnsSize = columns;
            var list = new List<List<T>>();
            for (var i = 0; i < rows; i++)
            {
                var tmpList = new List<T>();
                for (var j = 0; j < columns; j++)
                {
                    tmpList.Add(val);
                }
                list.Add(tmpList);
            }

            Data = list;
        }




        public delegate bool ElementJudge(T e);

        public dynamic this[ElementJudge elementJudge]
        {
            get
            {
                var ans = new Matrix<bool>(RowsCount, ColumnsCount);
                for (var i = 0; i < RowsCount; i++)
                {
                    for (var j = 0; j < ColumnsCount; j++)
                    {
                        if (elementJudge.Invoke(this[i, j]))
                        {
                            ans[i, j] = true;
                        }
                    }
                }
                return ans;
            }
            set
            {
                for (var i = 0; i < RowsCount; i++)
                {
                    for (var j = 0; j < ColumnsCount; j++)
                    {
                        if (elementJudge.Invoke(this[i, j]))
                        {
                            this[i, j] = (T) value;
                        }
                    }
                }
            }
        }

        public delegate T ElementSetDelegate(T e, (int x, int y) index);

        public delegate bool SetJudge(T e, (int x, int y) index);
        public void Set(SetJudge setJudge, ElementSetDelegate elementSetDelegate)
        {
            //mat.Set((_,_)=> true, 
         //          (_, index) => index.x * mat.ColumnsCount + index.y);

            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    if (setJudge(this[i, j], (i, j)))
                    {
                        this[i, j] = elementSetDelegate(this[i, j], (i, j));
                    }
                }
            }
        }
        public object this[Matrix<bool> flags]
        {
            get
            {
                var ans = (Matrix<T>)Clone();
                for (var i = 0; i < RowsCount; i++)
                {
                    for (var j = 0; j < ColumnsCount; j++)
                    {
                        if (!flags[i, j])
                        {
                            ans[i, j] = default;
                        }
                    }
                }
                return ans;
            }
            set
            {
                for (var i = 0; i < RowsCount; i++)
                {
                    for (var j = 0; j < ColumnsCount; j++)
                    {
                        if (flags[i, j])
                        {
                            this[i, j] = (T) value;
                        }
                    }
                }
            }
        }
        public Matrix(IReadOnlyList<T> values, int rows, int columns)
        {
            if (values.Count != rows * columns)
                throw new ArithmeticException();
            RowSize = rows;
            ColumnsSize = columns;
            var list = new List<List<T>>();
            for (var i = 0; i < rows; i++)
            {
                var tmpList = new List<T>();
                for (var j = 0; j < columns; j++)
                {
                    tmpList.Add(values[i * rows + j]);
                }
                i.PrintToConsole();
                list.Add(tmpList);
            }
            Data = list;
        }

        public void Fill(int row, int column, int h, int w, int d)
        {
            
        }
        public delegate T ElementProcess(int i, int j, int e);
        public Matrix(int rows, int columns, ElementProcess initAction)
        {
            RowSize = rows;
            ColumnsSize = columns;
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < rows; i++)
            {
                List<T> clist = new List<T>();
                for (int j = 0; j < columns; j++)
                {
                    clist.Add(initAction(i, j, 0));
                }
                list.Add(clist);
            }

            Data = list;
        }


        
        
        public IEnumerator<List<T>> GetEnumerator()
        {
            return ((IEnumerable<List<T>>) Data).GetEnumerator();
        }

        public override string ToString()
        {
            var str = "|";
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    str += " " + Data[i][j] + "\t , ";
                }

                str = str.Substring(0, str.Length - 2) +"|\n|";
            }

            str = str.Substring(0, str.Length - 1) + Shape +'\n';
            return str;
        }
        
        
        //索引器
        public T this[int r, int c]
        {
            get => Data[r][c];
            set => Data[r][c] = value;
        }

        
        public Matrix<T> this[int rowFrom, int rowTo, int columnFrom, int columnTo]
        {
            get
            {
                var rf = rowFrom < 0 ? 0 : rowFrom;
                var rt = (rowTo < 0 || rowTo >= RowsCount) ? RowsCount - 1 : rowTo;
                var cf = columnFrom < 0 ? 0 : columnFrom;
                var ct = (columnTo < 0 || columnTo >= ColumnsCount) ? ColumnsCount - 1 : columnTo;
                return SubMatrix(rf, rt,cf,ct);
            }
            set
            {
                var rf = rowFrom < 0 ? 0 : rowFrom;
                var rt = rowTo < 0 || rowTo >= RowsCount ? RowsCount - 1 : rowTo;
                var cf = columnFrom < 0 ? 0 : columnFrom;
                var ct = columnTo < 0 || columnTo >= ColumnsCount ? ColumnsCount - 1 : columnTo;
                var mat = value;
                if (mat.RowsCount != rt - rf + 1 || mat.ColumnsCount != ct - cf + 1)
                {
                    throw new ArithmeticException(rf+","+rt+","+cf+","+ct+" not match "+mat.Shape);
                }

                for (var i = rf; i <= rt; i++)
                {
                    for (var j = cf; j <= ct; j++)
                    {
                        Data[i][j] = mat[i - rf, j - cf];
                        
                    }
                }
            }
        }
        
        //选择某些行以及某些列，索引可重复
        public Matrix<T> this[int[] rowsIndexes, int[] columnsIndexes]
        {
            get
            {
                
                List<Vector<T>> vectors = new List<Vector<T>>();
                for (var i = 0; i < rowsIndexes.Length; i++)
                {
                    Vector<T> vector = Iloc(rowsIndexes[i], rowsIndexes[i])[0];
                    vectors.Add(vector);
                }
                //选择需要的列
                for (var i = 0; i < vectors.Count; i++)
                {
                    vectors[i] = vectors[i][columnsIndexes];
                }

                return new Matrix<T>(vectors);
            }
            
        }


        public void AddARow(int index = -1)
        {
            var newRow = new List<T>();
            for (var i = 0; i < ColumnsCount; i++)
            {
                newRow.Add(default);
            }
            AddARow(newRow.ToArray(),index);
        }
        public void AddARow(T[] row, int index = -1)
        {
            if (row.Length != this.Shape.Val)
            {
                throw new ArithmeticException();
            }
            else
            {
                var newRow = new List<T>(row);
                if (index < 0)
                {
                    //默认插在最尾
                    this.Data.Add(newRow);
                }
                else
                {
                    this.Data.Insert(index,newRow);
                }
            }
        }

        public void AddColumn(int index = -1)
        {
            var newColumn = new List<T>();
            for (var i = 0; i < RowsCount; i++)
            {
                newColumn.Add(default);
            }
            AddColumn(newColumn.ToArray(),index);
        }
        public void AddColumn(T[] column, int index = -1)
        {
            if (column.Length != this.Shape.Key)
            {
                throw new ArithmeticException();
            }
            else
            {
                var newColumn = new List<T>(column);
                if (index < 0)
                {
                    //默认插在最尾
                    for (int i = 0; i < RowsCount; i++)
                    {
                        Data[i].Add(newColumn[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < RowsCount; i++)
                    {
                        Data[i].Insert(index,newColumn[i]);
                    }
                }
            }
        }


        public Matrix<T> SubMatrix(int rowTop, int rowBottom, int columnLeft, int columnRight)
        {
            var vectors = Iloc(rowTop, rowBottom);
            
            var indexes = new List<int>();
            for (var i = columnLeft; i <= columnRight; i++)
            {
                indexes.Add(i);
            }

            for (int i = 0; i < vectors.Count; i++)
            {
                vectors[i] = vectors[i][indexes.ToArray()];
                
            }

      
            return new Matrix<T>(vectors);
        }
        public Vector<T> Dense()
        {
            var vector = new Vector<T>(RowsCount*ColumnsCount);
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    vector[i * ColumnsSize + j] = this[i, j];
                }
            }

            
            vector.IsColumnMatrix = false;
            return vector;
        }


        public Matrix<T> Reverse180()
        {
            var mat = (Matrix<T>) this.Clone();
            var n = Shape.Key * Shape.Val;
            var mid = n >> 1;
            for (var p1 = 0; p1 < mid; p1++)
            {
                var p1X = p1 / Shape.Val;
                var p1Y = p1 % Shape.Val;
                var p2 = n - p1 - 1;
                var p2X = p2 / Shape.Val;
                var p2Y = p2 % Shape.Val;
                //swap
                var t = mat.Data[p1X][p1Y];
                mat.Data[p1X][p1Y] = mat.Data[p2X][p2Y];
                mat.Data[p2X][p2Y] = t;
            }

            return mat;
        }
        
        
        /*选择行/列*/
        //默认选择行
        public List<Vector<T>> Iloc(int from, int to, int axis = 0)
        {
            var vectors = new List<Vector<T>>();
            if(axis!=0 && axis!=1) 
                throw new ArithmeticException("Axis should be 1 or 0");
            //行选择
            if (axis == 0)
            {
                for (int i = from; i <= to; i++) {
                    //Extract a row
                    var vector = new Vector<T>(Data[i].ToArray());
                    vector.IsColumnMatrix = false;
                    vectors.Add(vector);
                   
                }
            }
            else
            {
                for (var i = from; i <= to; i++)
                {
                    var v = new Vector<T>(RowsCount) {IsColumnMatrix = true};
                    vectors.Add(v);
                }

               
                for (var i = 0; i < RowsCount; i++) {
                    for (var j = from; j <= to; j++)
                    {
                        vectors[j-from][i] = Data[i][j];
                    }
                }
            }

            return vectors;
        }
        /*类型转换*/
        public static  explicit operator T(Matrix<T> matrix)
        {
            if (matrix.RowsCount != 1 || matrix.ColumnsCount != 1 ) 
                throw new ArithmeticException("Matrix shape != (1,1) when trying to covert to a scalar");
            return matrix[0,0];
        }

        public static  explicit operator Matrix<T>(T[] arr)
        {
            return arr.ToMatrix(1, arr.Length);
        }

        public static explicit operator Matrix<T>(T e)
        {
            return new T[]{e}.ToMatrix(1, 1);
        }
        
        public static explicit operator Vector<T>(Matrix<T> matrix)
        {
            if (matrix.RowsCount != 1 && matrix.ColumnsCount != 1)
            {
                throw new ArithmeticException("Matrix shape is" + matrix.Shape +" which shold be (N,1) or (1,N)");
            }
            if(matrix.RowsCount == 1) return  matrix.Iloc(0, 0)[0];
            return matrix.Iloc(0, 0, 1)[0];
        }


        public object Clone()
        {
            var matrix = new Matrix<T>(Data);
            matrix.RowSize = RowSize;
            matrix.ColumnsSize = ColumnsSize;
            return matrix;
        }

        public delegate object MatrixMapFunction(int indexR,int indexC,object x);

        public Matrix<object> Map(MatrixMapFunction mapFunction)
        {
            Matrix<object> matrix = (Matrix<T>)Clone();
            for (var i = 0; i < matrix.RowsCount; i++)
            {
                for (var j = 0; j < matrix.ColumnsCount; j++)
                {
                    matrix[i, j] = mapFunction(i,j,matrix[i, j]);
                }
            }
            return matrix;
        }

        
        public static implicit operator Matrix<object>(Matrix<T> matrix)
        {
            var m = new Matrix<object>(matrix.RowsCount,matrix.ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = matrix[i, j];
                }
            }

            return m;
        }



        public Matrix<T2> CastToMatrix<T2>(Func<T, T2> castFunc)
        {
            var t = (Matrix<T>)Clone();
            return t.ElementEnumerator.Select(castFunc.Invoke).ToMatrix(this.RowsCount, this.ColumnsCount);
        }
        public Matrix<T2> CastToMatrix<T2>()
        {
            var t = (Matrix<T>)Clone();
            return t.ElementEnumerator.Select(e => (T2)(e as object)).ToMatrix(this.RowsCount, this.ColumnsCount);
        }
        public static explicit operator Matrix<T>(Matrix<object> matrix)
        {
            var m = new Matrix<T>(matrix.RowsCount,matrix.ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (T)matrix[i, j];
                }
            }

            return m;
        }


        public void Padding(int rL, int rR, int cT, int cB)
        {
            for(var i = 0; i < rL; i++)
                AddColumn(0);
            for(var i = 0; i < rR; i++)
                AddColumn(ColumnsCount);
            for(var i = 0; i < cT; i++)
                AddARow(0);
            for(var i = 0; i < cB; i++)
                AddARow(RowsCount);
        }

        public IEnumerable<Matrix<T>> WindowSelect2D( (int h, int w) winSize,
           (int r, int c) step = default, (int l, int r, int t, int b) padding = default, bool noPadding = false)
        {
            if (winSize.h > RowsCount || winSize.w > ColumnsCount)
                throw new ArithmeticException();
            if (padding == default && !noPadding)
            {
                padding = ((winSize.w - 1) / 2, (winSize.w - 1) / 2, (winSize.h - 1) / 2, (winSize.h - 1) / 2);
            }

            if (noPadding)
            {
                padding = (0, 0, 0, 0);
            }
            if (step == default)
            {
                step = (1, 1);
            }

            var mat = (Matrix<T>) Clone();
            for(var i = 0; i < padding.l; i++)
                mat.AddColumn(0);
            for(var i = 0; i < padding.r; i++)
                mat.AddColumn(mat.ColumnsCount );
            for(var i = 0; i < padding.t; i++)
                mat.AddARow(0);
            for(var i = 0; i < padding.b; i++)
                mat.AddARow(mat.RowsCount);
            
           
            for (var r = 0; r + winSize.h - 1 < mat.RowsCount; r += step.r)
            {
                for (var c = 0; c + winSize.w - 1< mat.ColumnsCount; c += step.c)
                {
                    
                    var subMat = mat[r..(r + winSize.h - 1), c..(c + winSize.w - 1)];
                    
                    yield return subMat;
                }
            }
        }

        public void DeleteRow(int i)
        {
            Data.RemoveAt(i);
        }
       
        public void DeleteCol(int i)
        {
            foreach (var l in Data)
            {
                l.RemoveAt(i);
            } 
        }
        
        public void ReverseSubMatrix(Range rowRange, Range colRange, bool inRow = true)
        {
            if (inRow)
                this[rowRange, colRange] = this[rowRange, colRange].Reverse().ToMatrix2D();
            else
                this[rowRange, colRange] = this[rowRange, colRange].ColumnsEnumerator.Reverse().ToMatrix2D(true);
        }
        public static Matrix<object> SpecifyReshape(int r, int c, Matrix<object> mat)
        {
            var cr = mat.RowsCount;
            var cc = mat.ColumnsCount;
            if (r == cr && c == cc)
            {
                return mat;
            }
            if (cr == 1 && cc == 1)
            {
                return Enumerable.Repeat(mat[0, 0], r * c).ToMatrix(r, c);
            }

            if (cr * cc == r * c)
            {
                return mat.SelectMany(e => e).ToMatrix(r, c);
            }

            if (cr * cc > r * c)
            {
                return mat.SelectMany(e => e).Take(r * c).ToMatrix(r, c);
            }

            var t = mat.SelectMany(e => e)
                .ToList();
            t.AddRange(Enumerable.Repeat<object>(default, r * c - cr * cc));
            return t.ToMatrix(r, c);
        }
        public static Matrix<object> operator + (Matrix<T> matrix1, Matrix<object> matrix2)
        {
            if (!matrix1.Shape.Equals(matrix2.Shape))
            {
                matrix2 = SpecifyReshape(matrix1.RowsCount, matrix1.ColumnsCount, matrix2);
            }
            var m = new Matrix<object>(matrix1.RowsCount,matrix1.ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (dynamic)matrix1[i, j] + (dynamic)matrix2[i,j];
                }
            }

            return m;
        }
        public static Matrix<object> operator + (Matrix<T> matrix1, object val)
        {
            Matrix<Object> mat = new Matrix<object>(matrix1.Shape.Key,matrix1.Shape.Val,val);
            
            return matrix1 + mat;
        }
        public static Matrix<object> operator - (Matrix<T> matrix1, object val)
        {
            var mat = new Matrix<object>(matrix1.Shape.Key,matrix1.Shape.Val,val);
            
            return matrix1 - mat;
        }
        public static Matrix<object> operator - (Matrix<T> matrix1, Matrix<Object> matrix2)
        {
            if (!matrix1.Shape.Equals(matrix2.Shape))
            {
                matrix2 = SpecifyReshape(matrix1.RowsCount, matrix1.ColumnsCount, matrix2);
            }
            var m = new Matrix<object>(matrix1.RowsCount,matrix1.ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (dynamic)matrix1[i, j] - (dynamic)matrix2[i,j];
                }
            }

            return m;
        }
        
        //
        public static Matrix<object> operator * (Matrix<T> matrix1, object val)
        {
            var mat = new Matrix<object>(matrix1.Shape.Key,matrix1.Shape.Val,val);
            
            return matrix1.DotMultiply(mat);
        }

        public static Matrix<object> operator /(Matrix<T> matrix1, Matrix<object> mat2)
        {
            var m = new Matrix<object>(matrix1.RowsCount, matrix1.ColumnsCount);
            return matrix1.MatrixSelect((e, x, y) => (dynamic)e / (dynamic)mat2[x, y]);
        }

       
        public static Matrix<object> operator / (Matrix<T> matrix1, object val)
        {
            var m = new Matrix<object>(matrix1.RowsCount, matrix1.ColumnsCount);
            if (val.GetType().HasImplementedRawGeneric(typeof(Matrix<>)))
            {
               
                return matrix1.MatrixSelect((e, x, y) => ((dynamic)e / (dynamic)((Matrix<object>)(Matrix<T>)val)[x, y]));
            }
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (dynamic)matrix1[i, j] / (dynamic)val;
                }
            }

            return m;
        }

        public Matrix<TResult> MatrixSelect<TResult>( Func<T,  int,  int, TResult> applyFunc)
        {

            
            var ans = new Matrix<TResult>(RowsCount, ColumnsCount);
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    
                    ans.Data[i][j] = applyFunc.Invoke(Data[i][j], i, j);
                }
            }

            return ans;
        }
        public void Apply(Func<T, T> applyFunc)
        {
            Apply(..^1, ..^1 ,applyFunc);
        }
        public void Apply(Range rowRange, Range colRange, Func<T, T> applyFunc)
        {
            
            var rs = rowRange.Start.IsFromEnd? RowsCount - rowRange.Start.Value : rowRange.Start.Value;
            var re = rowRange.End.IsFromEnd ? RowsCount -  rowRange.End.Value : rowRange.End.Value;
            var cs = colRange.Start.IsFromEnd ? ColumnsCount-  colRange.Start.Value : colRange.Start.Value;
            var ce = colRange.End.IsFromEnd ? ColumnsCount -  colRange.End.Value : colRange.End.Value;

            if (re < 0)
            {
                re = RowsCount + re;
            }

            if (ce < 0)
            {
                ce = ColumnsCount + ce;
            }

            for (var i = rs; i <= re; i++)
            {
                for (var j = cs; j <= ce; j++)
                {
                    Data[i][j] = applyFunc.Invoke(Data[i][j]);
                }
            }
        }
        public static Matrix<object> operator * (Matrix<T> matrix1, Matrix<object> matrix2)
        {
            
            if (!matrix1.Shape.Val.Equals(matrix2.Shape.Key))
            {
                if (!matrix1.Shape.Equals(matrix2.Shape))
                    throw new ArithmeticException("" + matrix1.Shape + "*" + matrix2.Shape);
                var m1 = matrix1.SelectMany(e => e);
                var m2 = matrix2.SelectMany(e => e);
                return 
                    m1.Zip(m2, (t, o) => (dynamic) t * (dynamic)o)
                        .ToMatrix((int)matrix1.Shape[0], (int)matrix1.Shape[1]);
            }
           
            var m = new Matrix<T>(matrix1.RowsCount,matrix2.ColumnsCount);
            for (var i = 0; i < (int)matrix1.Shape[0]; i++)
            {
                Vector<object> vec = null;
                
                for (var j = 0; j < (int) matrix1.Shape[1]; j++)
                {

                    if (vec == null)
                        vec = (Vector<T>) (matrix2.Iloc(j, j, 0)[0]) * matrix1[i, j];
                    else
                        vec += (Vector<T>) (matrix2.Iloc(j, j, 0)[0]) * matrix1[i, j];
                }
                
                m[i, i, -1, -1] = ((Matrix<T>)(Matrix<object>)vec)._T();
            }

            return m;
        }
        
        public override int GetHashCode()
        {
            var hash = 0;
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    hash += this[i, j].GetHashCode() + i + j;
                }
            }

            return hash;
        }

        public Matrix<T> this[Range rowRange, Range colRange, Func<T,T> elementProcessFunc = null]
        {
            get
            {
                var rs = rowRange.Start.IsFromEnd? RowsCount - rowRange.Start.Value : rowRange.Start.Value;
                var re = rowRange.End.IsFromEnd ? RowsCount -  rowRange.End.Value : rowRange.End.Value;
                var cs = colRange.Start.IsFromEnd ? ColumnsCount-  colRange.Start.Value : colRange.Start.Value;
                var ce = colRange.End.IsFromEnd ? ColumnsCount -  colRange.End.Value : colRange.End.Value;

                if (re < 0)
                {
                    re = RowsCount + re;
                }

                if (ce < 0)
                {
                    ce = ColumnsCount + ce;
                }

                if (ce >= ColumnsCount)
                {
                    ce = ColumnsCount - 1;
                }

                if (re >= RowsCount)
                {
                    re = RowsCount - 1;
                }
                if (elementProcessFunc != null)
                    return SubMatrix(rs, re, cs, ce).SelectMany(e => e)
                        .Select(elementProcessFunc.Invoke).ToMatrix(re - rs + 1, ce - cs + 1);
                
                return SubMatrix(rs, re, cs, ce);
            }

            set
            {
                var rs = rowRange.Start.IsFromEnd? RowsCount - rowRange.Start.Value : rowRange.Start.Value;
                var re = rowRange.End.IsFromEnd ? RowsCount -  rowRange.End.Value : rowRange.End.Value;
                var cs = colRange.Start.IsFromEnd ? ColumnsCount-  colRange.Start.Value : colRange.Start.Value;
                var ce = colRange.End.IsFromEnd ? ColumnsCount -  colRange.End.Value : colRange.End.Value;
                
                if (rs > re || cs > ce)
                    return;
                var r = value.RowsCount;
                var c = value.ColumnsCount;
                if (re < 0)
                {
                    re = RowsCount + re;
                }

                if (re > RowsCount - 1)
                    re = RowsCount - 1;
                if (ce > ColumnsCount - 1)
                    ce = ColumnsCount - 1;
                if (ce < 0)
                {
                    ce = ColumnsCount + ce;
                }
                //完全符合
                var rNeed = re - rs + 1;
                var cNeed = ce - cs + 1;

                var realValue = 
                    elementProcessFunc != null ? 
                        value.SelectMany(e => e).Select(elementProcessFunc).ToMatrix(r, c) : value;
                
                //严格相等
                if (rNeed == r && cNeed == c)
                {
                    this[rs, re, cs, ce] = realValue;
                   
                }
                //不太符合惹，但是元素个数相等
                else if (r * c == rNeed * cNeed)
                {
                    this[rs, re, cs, ce] = realValue.SelectMany(e => e).ToMatrix(rNeed, cNeed);
                    
                }else if (r == 1 && c == 1) //仅有一个元素。用这个元素填充所有
                {
                    this[rs, re, cs, ce] = Enumerable
                        .Repeat(realValue[0, 0], rNeed * cNeed).ToMatrix(rNeed, cNeed);
                }else if (r * c > rNeed * cNeed)
                {
                    //提供的要素过多
                    this[rs, re, cs, ce] = realValue.SelectMany(e => e).Take(rNeed * cNeed).ToMatrix(rNeed, cNeed);
                }
                else
                {
                    //提供的过少了。
                    var p = 0;
                    var t = realValue.SelectMany(e => e).ToArray();
                    for (var i = rs; i <= re; i++)
                    {
                        for (var j = cs; j <= ce; j++)
                        {
                            if(p >= t.Length)
                                return;
                            Data[i][j] = t[p++];
                        }
                    }
                }

            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(Matrix<T>)) return false;
            var matrix = (Matrix<T>) obj;
            if (matrix.ColumnsCount != ColumnsCount || matrix.RowsCount != RowsCount) return false;
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    if (!this[i, j].Equals(matrix[i, j])) return false;
                }
            }
            return true;

        }

       

        public Matrix<object> BlockMultiPly(Matrix<T> mat, int blockSize = 16)
        {
            var r = RowsCount;
            var c = ColumnsCount;
            var r2 = mat.RowsCount;
            var c2 = mat.ColumnsCount;
            
            if (!Shape[1].Equals(mat.Shape[0]) || blockSize <= 0)
            {
                throw new ArithmeticException();
            }
            var m = new Matrix<object>(r,  c2, default(T));

            for (var i = 0; i < r; i += blockSize)
            {
                for (var j = 0; j < c2; j += blockSize)
                {
                    for (var k = 0; k < c; k += blockSize)
                    {
                        for (var i1 = i; i1 < i + blockSize && i1 < r; i1++)
                        {
                            for (var j1 = j; j1 < j + blockSize && j1 < c2; j1++)
                            {
                                for (var k1 = k; k1 < k + blockSize && k1 < c; k1++)
                                {
                                    var foo = (dynamic)this[i1, k1] * (dynamic)mat[k1, j1];
                                    m[i1, j1] = (dynamic)m[i1, j1] + foo;
                                }
                            }
                        }
                    }
                }
            }

            return m.CastToMatrix(e => (object)e);
        }

        
        //局所性の十分利用のため、上のブロック乗算を利用した方がいい、
        public Matrix<object> DotMultiply(Matrix<object> matrix)
        {
            if (!matrix.Shape.Equals(Shape))
            {
                throw new ArithmeticException();
            }
            var m = new Matrix<Object>(RowsCount,ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (dynamic)this[i, j] * (dynamic)matrix[i,j];
                }
            }

            return m;
        }
      
        public Matrix<object> CorrespondMultiply(Matrix<object> matrix)
        {
            if (!matrix.Shape.Equals(Shape))
            {
                throw new ArithmeticException();
            }
            var m = new Matrix<Object>(RowsCount,ColumnsCount);
            for (var i = 0; i < (int)m.Shape[0]; i++)
            {
                for (var j = 0; j < (int) m.Shape[1]; j++)
                {
                    m[i, j] = (dynamic)this[i, j] * (dynamic)matrix[i,j];
                }
            }

            return m;
        }

        
        public Matrix<T> Reshape(Tuple<int, int> shape)
        {
            //为了效率更高可以用内存操作
            if (!Equals(shape, Shape)) throw new ArithmeticException();
            var rs = (int) shape[0];
            var cs = (int) shape[1];
            var matrix = new Matrix<T>(rs,cs);
            for (var i = 0; i < rs; i++)
            {
                for (var j = 0; j < cs; j++)
                {
                    var pos = i * rs + j;
                    matrix[i, j] = this[pos / ColumnsCount, pos % ColumnsCount];
                }
            }

            return matrix;
        }


        public Vector<T> this[int row]
        {
            get => new( Data[row].ToArray());
            set => Data[row] = value.ToList();
        }


        public IEnumerable<Matrix<T>> Split(int height, int width)
        {
            var ans = new List<Matrix<T>>();
            for (var i = 0; i < RowsCount; i+= height)
            {
                for (var j = 0; j < ColumnsCount; j += width)
                {
                    ans.Add(this[i..(i + height - 1), j..(j + width - 1)]);
                }
            }

            return ans;
        }
        public IEnumerable<T> this[IEnumerable<(int row, int col)> indexes]
        {
            get
            {
                return indexes.Select(e => this[e.row, e.col]).ToList();
            }
            set
            {
                var v = value.ToList();
                var valueTuples = indexes as (int row, int col)[] ?? indexes.ToArray();
                for (var i = 0; i < valueTuples.Length; i++)
                {
                    var (row, col) = valueTuples[i];
                    this[row, col] = v[i];
                }
            }
        }
        
        public IEnumerable<T> this[IEnumerable<IEnumerable<int>> index]
        {
            get
            {
                return this[index.Select(i => {
                    var t = i.ToArray();
                    return (t[0], t[1]);
                })];
            }
            set
            {
                this[index.Select(i =>
                {
                    var t = i.ToArray();
                    return (t[0], t[1]);
                })] = value;
            }
        }
    }
}