using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;
using CIExam.os.Cache;

namespace CIExam.Math
{
    public class BoolMatrix : ValueMatrix<bool>, ICloneable
    {
        public BoolMatrix(IReadOnlyCollection<bool[]> data) : base(data)
        {
        }

        public BoolMatrix(int rowsCount, int columnsCount) : base(rowsCount, columnsCount)
        {
        }

        public new object Clone()
        {
            var c = (Matrix<bool>)base.Clone();
            return FromMatrix(c);
        } 

        public BoolMatrix ExtendWithParityCheck()
        {
            var clone = (BoolMatrix)Clone();
            clone.Padding(0,1,0,1);
            var r = Enumerable.Range(0, RowsCount).Select(index =>
                clone[index].Aggregate(0, (a, b) => a ^ (b ? 1 : 0)) == 1);
            var cols = clone.ColumnsEnumerator.ToArray();
            clone[..(RowsCount - 1), ^1..^1] = r.ToMatrix(RowsCount, 1);
            var c = Enumerable.Range(0, ColumnsCount).Select(index =>
                cols[index].Aggregate(0, (a, b) => a ^ (b ? 1 : 0)) == 1);
            clone[^1..^1, ..(ColumnsCount - 1)] = c.ToMatrix(1, ColumnsCount);
            
            clone[RowsCount, ColumnsCount] = clone.Last().Take(ColumnsCount)
                .Concat(clone.ColumnsEnumerator.Last().Take(RowsCount))
                .Aggregate(0, (a, b) => a ^ (b ? 1 : 0)) == 1;
            return clone;
        }
        
        public BoolMatrix CorrespondenceMultiply(BoolMatrix mat2)
        {
            if (mat2 == null)
                return null;
            if (mat2.RowsCount != RowsCount || mat2.ColumnsCount != ColumnsCount)
                return null;
            var res = new BoolMatrix(RowsCount, ColumnsCount);
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    res[i, j] = this[i, j] && mat2[i, j];
                }
            }
            return res;
        }
        const int _block = 255;

        public static BoolMatrix FromMatrix(Matrix<bool> mat)
        {
            var bm = new BoolMatrix(mat.Select(e => e.ToArray()).ToArray());
            return bm;
        }
        public static BoolMatrix operator *(BoolMatrix matrix1, BoolMatrix matrix2)
        {
            var r = matrix1.RowsCount;
            var c = matrix1.ColumnsCount;
            var r2 = matrix2.RowsCount;
            var c2 = matrix2.ColumnsCount;
            if (c != r2)
                throw new Exception();
            var res = new BoolMatrix(r, c2);
            for (var i = 0; i < r; i += _block)
            {
                for (var j = 0; j < c2; j += _block)
                {
                    for (var k = 0; k < c; k += _block)
                    {
                        // var ulong1 = matrix1[i].Skip(k).Take(_block);
                        // var ulong2 = matrix2.ColumnsEnumerator.Skip(j).First().Take(_block);
                        // var tij = res[i, j] || 
                        //           ulong1.Zip(ulong2, (a, b) => a && b).Aggregate((a, b) => a || b);

                        
                        for (var i1 = i; i1 < i + _block && i1 < r; i1++)
                        {
                            for (var j1 = j; j1 < j + _block && j1 < c2; j1++)
                            {
                                for (var k1 = k; k1 < k + _block && k1 < c; k1++)
                                {
                                    res[i1, j1] |= matrix1[i1, k1] & matrix2[k1, j1];
                                }
                            }
                        }
                       
                    }
                }
            }

            return res;
        }

        public override string ToString()
        {
            var s = "";
            for (var i = 0; i < RowsCount; i++)
            {
                for (var j = 0; j < ColumnsCount; j++)
                {
                    s += this[i, j] ? 1 : 0;
                    s += " ";
                }

                s += '\n';
            }

            return s;
        }

        public static BoolMatrix operator |(BoolMatrix matrix1, BoolMatrix matrix2)
        {
            var r = matrix1.RowsCount;
            var c = matrix1.ColumnsCount;
            var r2 = matrix2.RowsCount;
            var c2 = matrix2.ColumnsCount;
            if (r != r2 || c != c2)
                throw new Exception();
            var res = new BoolMatrix(r, c);
            for (var i = 0; i < r; i ++)
            {
                for (var j = 0; j < c; j ++)
                {
                    res[i, j] = matrix1[i, j] || matrix2[i, j];
                }
            }

            return res;
        }
        public static BoolMatrix operator &(BoolMatrix matrix1, BoolMatrix matrix2)
        {
            return matrix1.CorrespondenceMultiply(matrix2);
        }
    }

    public static class MatrixExt
    {

        public static Matrix<double> GaussianElimination(Matrix<double> mat)
        {
            var matrix = mat.Clone() as Matrix<double>;
            var r = matrix.RowsCount;
            var c = matrix.ColumnsCount;

            void RowSwap(int i, int j)
            {
                var t = matrix[i];
                matrix[i] = matrix[j];
                matrix[j] = t;
            }
            
            void ERow(int rowIndex)
            {
                
                var b = matrix[rowIndex][rowIndex];
                if (b == 0)
                {
                    var j = rowIndex + 1;
                    while (j < r)
                    {
                        RowSwap(rowIndex, j);
                        if (matrix[rowIndex][rowIndex] != 0)
                        {
                            b = matrix[rowIndex][rowIndex];
                            break;
                        }
                        j++;
                    }
                }

                if (b == 0)
                    return;
                $"Process {rowIndex} row, sel {b}".PrintToConsole();

                for (var j = rowIndex + 1; j < r; j++)
                {
                    var curRow = matrix[j];
                    var ratio = matrix[j][rowIndex] / matrix[rowIndex][rowIndex];
                    var vec = (Vector<double>)(curRow - matrix[rowIndex] * ratio);
                    matrix[j] = vec;
                    //vec.PrintEnumerationToConsole();
                }
                //matrix.PrintToConsole();
            }
            

            for (var i = 0; i < r && i < c; i++)
            {
                ERow(i);
                if(matrix[i][i] == 0) //当前行消元后主对角线为0，后面全是0惹，直接退出，没必要继续消元
                    break;
            }
            return matrix;
        }

        public static Matrix<double> GaussianEliminationSaveDiagOnly(Matrix<double> mat)
        {
            var m = GaussianElimination(mat);
            var rows = mat.FindAll(e => e.Any(row => row != 0.0));
            if (!rows.Any())
                return m;
            var r = rows.Last().Item1; //最后一个不全为0的行索引

            void ERow(int i)
            {
                var last = m[i];
                for (var j = i - 1; j >= 0; j--)
                {
                    var cur = m[j];
                    var ratio = cur[i] / last[i];
                    var vec = (Vector<double>) (cur - last * ratio);
                    //last.PrintToConsole();
                    //(last * ratio).PrintToConsole();
                    m[j] = vec;
                    //m.PrintToConsole();
                }
                
            }
            for (var i = r; i >= 0; i--)
            {
                ERow(i);
            }

            return m;
        }
        //平移变换。其中offset负数为左/上。 x控制左右
        public static Matrix<T> Shift<T>(this Matrix<T> matrix, int rowStart, int rowEnd, int colStart, int colEnd ,int offsetX, int offsetY
            , T fill = default, bool noRefill = false)
        {
            var mat = (Matrix<T>) matrix.Clone();
            //先处理平移
            var area = mat[rowStart..rowEnd, colStart..colEnd];
            var cLen = colEnd - colStart + 1;
            var rLen = rowEnd - rowStart + 1;
            var newRs = rowStart + offsetY;
            var newCs = colStart + offsetX;

            if (noRefill)
            {
                mat[newRs..(newRs + rLen - 1), newCs..(newCs + cLen - 1)] = area;
                return mat;
            }
            area.PrintToConsole();
            if (offsetX >= 0 && offsetY >= 0)
            {
                mat[newRs..(newRs + rLen - 1), newCs..(newCs + cLen - 1)] = area;
                var re = newRs >= rowEnd ? rowEnd : newRs - 1;
                var ce = newCs >= colEnd ? colEnd : newCs - 1;
                //填充消失的部分
                mat[rowStart..rowEnd, colStart..ce] = fill.CastToMatrix();
                mat[rowStart..re, newCs..colEnd] = fill.CastToMatrix();
            }
            if (offsetX >= 0 && offsetY <= 0)
            {
                mat[newRs..(newRs + rLen - 1), newCs..(newCs + cLen - 1)] = area;
                var newRe = (newRs + rLen - 1);
                var newCe = (newCs + cLen - 1);
                var re = newRs >= rowEnd ? rowEnd : newRs - 1;
                var ce = newCs >= colEnd ? colEnd : newCs - 1;
                var rs = newRe < rowStart ? rowStart : newRe;
                //填充消失的部分
                mat[rowStart..rowEnd, colStart..ce] = fill.CastToMatrix();
                mat[newRe..rowEnd, newCs..colEnd] = fill.CastToMatrix();
            }
            if (offsetX <= 0 && offsetY >= 0)
            {
                mat[newRs..(newRs + rLen - 1), newCs..(newCs + cLen - 1)] = area;
                var newRe = (newRs + rLen - 1);
                var newCe = (newCs + cLen - 1);
                var re = newRs >= rowEnd ? rowEnd : newRs - 1;
                var ce = newCs >= colEnd ? colEnd : newCs - 1;
                var rs = newRe > rowEnd ? rowEnd : newRe;
                var cs = newCe < colStart ? colStart : newCe;
                //填充消失的部分
                mat[rowStart..rowEnd, cs..colEnd] = fill.CastToMatrix();
                mat[rowStart..re, colStart..newCe] = fill.CastToMatrix();
            }
            if (offsetX <= 0 && offsetY <= 0)
            {
                mat[newRs..(newRs + rLen - 1), newCs..(newCs + cLen - 1)] = area;
                var newRe = (newRs + rLen - 1);
                var newCe = (newCs + cLen - 1);
                var re = newRs >= rowEnd ? rowEnd : newRs - 1;
                var ce = newCs >= colEnd ? colEnd : newCs - 1;
                var rs = newRe < rowStart ? rowStart : newRe;
                var cs = newCe < colStart ? colStart : newCe;
                //填充消失的部分
                mat[rowStart..rowEnd, cs..colEnd] = fill.CastToMatrix();
                mat[rs..re, colStart..newCe] = fill.CastToMatrix();
            }
            //mat[rowStart..rowEnd, newCs..(newCs + cLen - 1)].PrintToConsole();
            //mat[rowStart..rowEnd, newCs..(newCs + cLen - 1)] = area;
            return mat;
        }
        public static BoolMatrix AsBoolMatrix(this Matrix<bool> matrix)
        {
            var bMat = new BoolMatrix(matrix.RowsCount, matrix.ColumnsCount);
            for (var i = 0; i < matrix.RowsCount; i++)
            {
                for (var j = 0; j < matrix.ColumnsCount; j++)
                {
                    bMat[i, j] = matrix[i, j];
                }
            }
            return bMat;
        }

        public static void SinglePositionFiltering<T>(this Matrix<T> mat, int x, int y, Matrix<T> filter)
        {
            var fW = filter.ColumnsCount;
            var fH = filter.RowsCount;
            var rs = x - fH / 2;
            rs = rs < 0 ? 0 : rs;
            var re = x + fH / 2;
            var cs = y - fW / 2;
            cs = cs < 0 ? 0 : cs;
            var ce = y + fW / 2;
            mat[rs..re, cs..ce] = mat[rs..re, cs..ce].CorrespondMultiply(filter).CastToMatrix(e => (T)e);
        }
    }
}