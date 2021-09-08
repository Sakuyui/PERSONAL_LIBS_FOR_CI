using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;
using JetBrains.Annotations;

namespace CIExam.Structure
{
    
    public class DuplicateKeyComparer<TKey> : IComparer<TKey> 
        //where TKey : IComparable
    {

        private ComparerStrategy _comparerStrategy;
        public DuplicateKeyComparer(ComparerStrategy c = null)
        {
            _comparerStrategy = c;
        }

        public delegate int ComparerStrategy(TKey x, TKey y);
        public int Compare(TKey x, TKey y)
        {
            
            if (x == null) return 0;
            
            var cmp = _comparerStrategy?.Invoke(x, y) ?? ((IComparable)x).CompareTo(y);
            return cmp == 0 ? 1 : cmp;
        }
    }
    
        public class ComparableStrut<T> : IComparable
    {
        public readonly T Data;
        public static Comparer<T> Comparer;
        public ComparableStrut(T data, Comparer<T> comparer = null)
        {
            if (comparer != null)
            {
                Comparer = comparer;
            }
            Data = data;
        }

        public int CompareTo(object? obj)
        {
            if (obj is T t)
            {
                return Comparer?.Compare(this.Data, t) ?? ((IComparable) Data).CompareTo(t);
            }

            return -1;
        }

        public override bool Equals(object? obj)
        {
            return Data.Equals(obj);
        }

        public override string ToString()
        {
            return Data.ToString();
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
    public class CustomerComparer<T> : Comparer<T>
    {
        public delegate int CompareStrategy(T t1, T t2);

        private CompareStrategy _compareStrategy = null;
     
        public CustomerComparer(CompareStrategy compareStrategy)
        {
            _compareStrategy = compareStrategy;
        }
        public override int Compare(T x, T y)
        {
            return _compareStrategy.Invoke(x, y); ;
        }

        
        //后缀数组
        public static void Test()
        {
            const string tmp = "abracadabra";
            var s = tmp.Select((t, i) 
                => tmp.Substring(i, tmp.Length - i)).ToList();
            s.Add("");
            s.PrintCollectionToConsole();
            var orderedEnumerable = 
                s.OrderBy(e => e, new CustomerComparer<string>((t1, t2) =>
                    string.Compare(t1, t2, StringComparison.Ordinal)
                ));
            orderedEnumerable.PrintCollectionToConsole();
        }
    }
 
    public class CustomerEqualityComparer<T> : IEqualityComparer<T>
    {
        public delegate bool EqualityCompareStrategy([CanBeNull]T t1,[CanBeNull]T t2);

        private readonly EqualityCompareStrategy _compareStrategy = null;
        private readonly Func<T, int> _hashCode;
        
        public CustomerEqualityComparer(EqualityCompareStrategy compareStrategy, Func<T, int> hashCode)
        {
            _compareStrategy = compareStrategy;
            _hashCode = hashCode;
        }


        public bool Equals(T? x, T? y)
        {
            return _compareStrategy.Invoke(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hashCode.Invoke(obj);
        }
    }
}