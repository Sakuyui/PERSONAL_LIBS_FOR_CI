using System.Collections.Generic;
using C5;

namespace CIExam.os.Cache
{
    
    public class LruList<T> : List<T>
    {
        public int Capacity => _capacity;
        private readonly int _capacity = int.MaxValue;
        public LruList(int capacity)
        {
            _capacity = capacity;
        }
        public LruList(){}

        public T Read(T element)
        {
            if (!Contains(element))
                return default;
            var i = IndexOf(element);
            return ReadByIndex(i);
        }
        
        public T ReadByIndex(int index)
        {
            var t = this[index];
            RemoveAt(index);
            Insert(0, t);
            return t;
        }

        public void Write(T element)
        {
            if (Contains(element))
            {
                Read(element);
                return;
            }
            //full
            if (Count == _capacity)
            {
                RemoveAt(Count - 1);
            }
            
            Insert(0, element);
            
        }
    }
}