using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.Math;
using CIExam.FunctionExtension;

namespace CIExam.ImageProcess
{
    public class GreyImage : Image<double>
    {
    
        private Matrix<double> _data;
        public Matrix<double> PixelMatrix => _data;
        public GreyImage(int height, int width) : base(height, width)
        {
            _data = new Matrix<double>(height, width);
        }

        private GreyImage(Matrix<object> mat) : base(mat.RowsCount, mat.ColumnsCount)
        {
            _data = mat.ElementEnumerator.Cast<double>().ToMatrix(Height, Width);
        }


        public static implicit operator GreyImage(Matrix<int> mat)
        {
            return new GreyImage(mat.CastToMatrix(e => (double)e));
        }
        public static implicit operator GreyImage(Matrix<double> mat)
        {
            return new GreyImage(mat);
        }
        public Pixel<double> this[int x, int y]
        {
            get => new Pixel<double>(x, y, _data[x, y]);
            set => _data[x, y] = value.Color;
        }

        public override string ToString()
        {
            return PixelMatrix.ToString();
        }

        public override IEnumerator<Pixel<double>> GetEnumerator()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    yield return this[i, j];
                }
            }
        }
    }
}