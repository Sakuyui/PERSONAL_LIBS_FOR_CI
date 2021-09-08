using System.Linq;
using C5;
using CIExam.Math;
using CIExam.Structure;
using IntervalTree;
using CIExam.FunctionExtension;

namespace CIExam.Geometry
{
    public static class RectangleTest
    {
        public static void Test()
        {
            //找出所有相交矩形
            var rect1 = new Rectangle(new Point2D(0, 10), 10, 8);
            var rect2 = new Rectangle(new Point2D(2, 12), 5, 5);
            var rect3 = new Rectangle(new Point2D(2, 20), 5, 2);
            var rects = new[] {rect1, rect2, rect3}.ExtendIndex().Select(e => new ValueTupleSlim(e.element, e.index));
            
            //区间树
            var it1X = new IntervalTree<double, ValueTupleSlim>();
            var it1Y = new IntervalTree<double, ValueTupleSlim>();
            //x,y分别投影
            var enumerable = rects as ValueTupleSlim[] ?? rects.ToArray();
            
            foreach (var rec in enumerable)
            {
                it1X.Add(((Rectangle)rec[1]).LeftTop.X, ((Rectangle)rec[1]).RightTop.X, rec);
                it1Y.Add(((Rectangle)rec[1]).LeftBottom.Y, ((Rectangle)rec[1]).LeftTop.Y, rec);
            }
            
            foreach (var rec in enumerable)
            {
                var recSet = it1X.Query(((Rectangle)rec[1]).LeftTop.X,((Rectangle)rec[1]).RightTop.X).ToHashSet();
                recSet.IntersectWith(it1Y.Query(((Rectangle)rec[1]).LeftBottom.Y, ((Rectangle)rec[1]).LeftTop.Y).ToHashSet());
                recSet.Remove(rec);
                //相交对
                var t = recSet.Select(e => ((int)rec[0], (int)e[0]));
                recSet.PrintEnumerationToConsole();
            }
            

            rect1.MarginMatrix.PrintToConsole();
            
            it1X.Query(8,20).PrintCollectionToConsole();
            //给定x, y范围，找出范围内的所有矩形
        }
    }
    public class Rectangle
    {
        public Point2D LeftTop { get; }
        public Point2D RightTop { get; }
        public Point2D LeftBottom { get; }
        public Point2D RightBottom { get; }
        private double Width { get; }
        private double Height { get; }

        public (Point2D ltop, Point2D rtop, Point2D lbottom, Point2D rbottom) Margin =>
            (LeftTop, RightTop, LeftBottom, RightBottom);

        public Matrix<double> MarginMatrix =>
            new[] {LeftTop.ToList(), RightTop.ToList(), LeftBottom.ToList(), RightBottom.ToList()}
                .SelectMany(e => e).ToMatrix(4, 2);

        public double Area => Height * Width;
        
        public Rectangle(Point2D leftTop, int h, int w)
        {
            LeftTop =  (Vector<object>)leftTop.ToVector();
            Height = h;
            Width = w;
            RightTop = new Point2D(leftTop.X + w, leftTop.Y);
            LeftBottom = new Point2D(leftTop.X, leftTop.Y - h);
            RightBottom = new Point2D(leftTop.X + w, leftTop.Y - h);
        }
        
        public Rectangle(int h, int w)
        {
            Height = h;
            Width = w;
        }

        public override string ToString()
        {
            return new ValueTupleSlim(LeftTop.ToList().ToEnumerationString(), Width, Height).ToString();
        }
    }
}