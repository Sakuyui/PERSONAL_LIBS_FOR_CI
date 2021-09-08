using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Math
{
    public class ValueMatrix<T> : Matrix<T>, IEquatable<Matrix<T>>
    {
        //private BitAlgorithms.BitTreeArray _bitTreeArray;


        public static ValueMatrix<T> FromMatrix(Matrix<T> mat)
        {
            var vMat = new ValueMatrix<T>(mat.RowsCount, mat.ColumnsCount) {[..^1, ..^1] = mat[..^1, ..^1]};
            return vMat;
        }
        private void Init()
        {
            // for (var r = 0; r < RowsCount; r ++)
            // {
            //     for (var c = 0; c < ColumnsCount; c ++)
            //     {
            //         var ID = r * ColumnsCount + c;
            //         _bitTreeArray.Update(ID, this[r, c]);
            //     }
            // }

        }
        // void update(int row, int col, int val) 
        // {
        //     int ID = row * Col + col;
        //     int diff = val - matrix[row][col];
        //     matrix[row][col] = val;
        //     BT.update(ID, diff);
        // }
        //
        // int sumRegion(int row1, int col1, int row2, int col2) 
        // {
        //     int res = 0;
        //     for(int r = row1; r <= row2; r++)
        //     {
        //         int ID1 = r * Col + col1;
        //         int ID2 = r * Col + col2;
        //         res += (BT.query(ID2) - BT.query(ID1 - 1));
        //     }
        //     return res;
        // }

      
        public ValueMatrix(IReadOnlyCollection<T[]> data) : base(data)
        {
            
        }

        public ValueMatrix(IReadOnlyList<List<T>> data) : base(data)
        {
            
        }

        public ValueMatrix(IReadOnlyList<Vector<T>> vectors) : base(vectors)
        {
        }

        public ValueMatrix(int rows, int columns, T val = default(T)) : base(rows, columns, val)
        {
           
        }

        public ValueMatrix(IReadOnlyList<T> values, int rows, int columns) : base(values, rows, columns)
        {
            
        }

        public ValueMatrix(int rows, int columns, ElementProcess initAction) : base(rows, columns, initAction)
        {
          
        }

        private bool IsEquals(Matrix<T> other)
        {
            return this.RowsCount == other.RowsCount &&
                   this.ColumnsCount == other.ColumnsCount &&
                   ElementEnumerator.SequenceEqual(other.ElementEnumerator);
        }
        public bool Equals(Matrix<T>? other)
        {
            return other != null && IsEquals(other);
        }

        public override bool Equals(object obj)
        {
            return obj switch {
                null => false,
                Matrix<T> mat => IsEquals(mat),
                _ => false
            };
        }

        public override int GetHashCode()
        {
            return ElementEnumerator.Aggregate(0, (a, b) => (a + b.GetHashCode()) % int.MaxValue);
        }
    }
}