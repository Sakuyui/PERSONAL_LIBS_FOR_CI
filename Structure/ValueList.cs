using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{
    /*
     * 该结构是一个集合，重写了比较以及哈希函数。能够直接对集合进行比较
     */
    public class ValueList<T> : List<T>
    {
        public ValueList()
        {
            
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, e) => (current + e.GetHashCode()) % int.MaxValue);
        }

        public override bool Equals(object? obj)
        {
            switch (obj)
            {
                case null:
                case List<T> list when list.Count != Count:
                    return false;
                case List<T> list:
                {
                    for (var i = 0; i < this.Count; i++)
                    {
                        if (!list[i].Equals(this[i]))
                            return false;
                    }

                    return true;
                }
                default:
                    return false;
            }
        }
    }

    public class ValueHashSet<T> : HashSet<T>
    {
        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, e) => (current + e.GetHashCode()) % int.MaxValue);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is List<T> list)
            {
                return SetEquals(list);
            }

            return false;
        }
    }
}