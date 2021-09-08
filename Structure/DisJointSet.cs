using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.FunctionExtension;

namespace CIExam.Structure
{
    public class DisJointSetTest
    {
        public static void Test()
        {
            DisJointSet<int> jointSet = new DisJointSet<int>();
            jointSet.Union(1 ,2);
            jointSet.PrintToConsole();
            jointSet.Union(2,3);
            jointSet.PrintToConsole();
            jointSet.Union(4,5);
            jointSet.PrintToConsole();
            jointSet.Union(2,5);
            jointSet.PrintToConsole();
            jointSet.DeleteElement(4);
            jointSet.PrintToConsole();
            jointSet.DeleteElement(1);
            jointSet.PrintToConsole();
            jointSet.Union(6, 7);
            jointSet.DeleteSet(2);
            jointSet.PrintToConsole();
        }
    }
    public class DisJointSet <T>
    {
        private readonly Dictionary<T, System.Collections.Generic.HashSet<T>> _dictionary =
            new Dictionary<T, System.Collections.Generic.HashSet<T>>(); //代表点到集合的映射
        private Dictionary<T, T> _representDictionary = new Dictionary<T, T>(); //元素到代表点的映射
        
        public void MakeSet(T e)
        {
            if (_representDictionary.ContainsKey(e) || _dictionary.ContainsKey(e)) return; //已经存在了
            var s = new System.Collections.Generic.HashSet<T>();
            _representDictionary[e] = e; //新的元素自己指向自己
            _dictionary.Add(e, s); //代表点->集合
            s.Add(e);
        }
        

        public bool IsSame(T a, T b)
        {
            return _representDictionary[a].Equals(_representDictionary[b]);
        }


        //从一个元素去找集合
        public (T, System.Collections.Generic.HashSet<T>) FindSet(T e)
        {
            if (_representDictionary.ContainsKey(e))
            {
                var p = _representDictionary[e];
                return (p, _dictionary[p]);
            }
            return (default, null);
        }


        public bool DeleteSet(T representElement)
        {
            if (_dictionary[representElement] == null) return false;
            var s = _dictionary[representElement];
            _dictionary.Remove(representElement);
            foreach (var e in s)
            {
                _representDictionary.Remove(e);
            }
            s.Clear();
            return true;
        }
        public bool DeleteElement(T e)
        {
            if (!_representDictionary.ContainsKey(e))
                return false;
            var p = _representDictionary[e];
            if (!p.Equals(e))
            {
                _representDictionary.Remove(e);
                //_dictionary[e].Remove(e);
                _dictionary[p].Remove(e);
            }
            else
            {
                //要删除的是根。
                var s = _dictionary[e];
                if (s.Count <= 1)
                {
                    s.Clear();
                    _dictionary.Remove(p);
                    _representDictionary.Remove(e);
                }
                else
                {
                    s.Remove(e);
                    var newP = s.Take(1).First();

                    _dictionary[newP] = s;
                    _dictionary.Remove(e);
                    _representDictionary.Remove(e);
                    foreach (var setElement in s){
                        _representDictionary[setElement] = newP;
                    }
                }
            }
            return true;
        }
        
        
        
        //将两个元素合并。
        public void Union(T a, T b)
        {
            MakeSet(a);
            MakeSet(b);
            var (p1, s1) = FindSet(a);
            var (p2, s2) = FindSet(b);

            if (p1.Equals(p2)) return;
            //合并两个集合
            var (newP,sp) = s1.Count >= s2.Count ? (p1,p2) : (p2,p1);
            s1.UnionWith(s2);
            _representDictionary[newP] = newP;
            foreach (var e in _dictionary[sp])
            {
                _representDictionary[e] = newP;
            }
            _dictionary.Remove(p1);
            _dictionary.Remove(p2);
            _dictionary[newP] = s1;
           
            s2.Clear();
            
        }

        public override string ToString()
        {
            var s = "";
            foreach (var key in _dictionary.Keys)
            {
                s += "(r: " + key + " => [";
                var set = _dictionary[key];
                foreach (var e in set)
                {
                    s += e + ", ";
                }

                if (set.Count != 0)
                    s = s.Substring(0, s.Length - 2);
                s += "])\n";
            }

            return s;
        }
    }
}