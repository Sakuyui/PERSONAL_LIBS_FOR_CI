using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using CIExam.Math;

namespace CIExam.ImageProcess
{
    public abstract class Image<TColor> : IEnumerable<Pixel<TColor>>
    {
        public int Height;
        public int Width;
        
        public Image(int height, int width)
        {
            if(height < 0 || width < 0)
                throw new ArithmeticException();
            Height = height;
            Width = width;
        }

        public abstract IEnumerator<Pixel<TColor>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RgbImage : Image<(double R, double G, double B, double T)>
    {
        private Matrix<double> _r;
        private Matrix<double> _g;
        private Matrix<double> _b;
        private Matrix<double> _t;
        public List<Matrix<double>> Mat => new[] {_r, _g, _r}.ToList();
        private bool _withTransplant;
        public RgbImage(int height, int width, bool withTransplant) : base(height, width)
        {
            _r = new Matrix<double>(height, width);
            _g = new Matrix<double>(height, width);
            _b = new Matrix<double>(height, width);
            
            this._withTransplant = withTransplant;
            if (withTransplant)
            {
                _t = new Matrix<double>(height, width);
            }
        }

        public Pixel<(double R, double G, double B, double T)> this[int px, int py]
        {
            get => new Pixel<(double R, double G, double B, double T)>(px, py
                , (_r[px,py], _g[px,py], _b[px,py], _withTransplant ? _t[px,py] : 255));
            set
            {
                _r[px, py] = value.Color.R;
                _g[px, py] = value.Color.G;
                _b[px, py] = value.Color.B;
                if(_withTransplant)
                    _t[px, py] = value.Color.T;
            }
        }
        public Matrix<double> this[int axis]
        {
            get
            {
                switch(axis)
                {
                  case  0:
                      return _r;
                  case 1:
                      return _g;
                  case 2:
                      return _b;
                  case 3:
                      return _t;
                }

                return null;
            }
        }

        public override IEnumerator<Pixel<(double R, double G, double B, double T)>> GetEnumerator()
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

    public class Pixel<TColor>
    {
        public int Px;
        public int Py;
        public TColor Color;
        
        public Pixel(int px, int py, TColor color)
        {
            Px = px;
            Py = py;
            Color = color;
        }
    }
}