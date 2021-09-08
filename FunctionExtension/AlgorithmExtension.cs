using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.FunctionExtension
{
    public static class AlgorithmExtension
    {
        public static (int l, int r) BinarySearchLeftRightEdge<T>(this IEnumerable<T> enumerable, T target) where T:IComparable
        {
            var arr = enumerable.ToArray();
            var l = 0;
            var r = arr.Length - 1;
            var leftBound = -1;
            var rightBound = -1;
            //find left boundary
            while (l <= r)
            {
                var m = (l + r) / 2;
                if (arr[m].CompareTo(target) >= 0)
                {
                    r = m - 1;
                }
                else
                {
                    l = m + 1;
                }
            }
            leftBound = l;
            if (l < 0)
                return (-1, -1);

            l = 0;
            r = arr.Length - 1;
            while (l <= r)
            {
                var m = ((r - l) >> 1) + l;
                if (arr[m].CompareTo(target) <= 0)
                {
                    l = m + 1;
                }
                else
                {
                    r = m - 1;
                }
            }

            rightBound = r;
            return (leftBound, rightBound);
        }
        
        public static (int l, int r) BinarySearchLeftRightEdge<T>(this IEnumerable<T> enumerable, T target, Comparer<T> comparer) 
        {
            var arr = enumerable.ToArray();
            var l = 0;
            var r = arr.Length - 1;
            var leftBound = -1;
            var rightBound = -1;
            //find left boundary
            while (l <= r)
            {
                var m = (l + r) / 2;
                if (comparer.Compare(arr[m],target) >= 0)
                {
                    r = m - 1;
                }
                else
                {
                    l = m + 1;
                }
            }
            leftBound = l;
            if (l < 0)
                return (-1, -1);

            l = 0;
            r = arr.Length - 1;
            while (l <= r)
            {
                var m = ((r - l) >> 1) + l;
                if (comparer.Compare(arr[m],target) <= 0)
                {
                    l = m + 1;
                }
                else
                {
                    r = m - 1;
                }
            }

            rightBound = r;
            return (leftBound, rightBound);
        }
    }
}