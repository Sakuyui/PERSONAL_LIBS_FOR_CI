using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;

//DataFrame
//输入输出API
//二维图形相关
//编码相关
//灰度图/矩阵处理

namespace CIExam.Math
{
    public delegate  (Hashtable,List<Tuple<E,int>>) CountStrategy<S,E>(S dataSource);

    
    public class Counter<S,E>
    {
        private CountStrategy<S,E> DefaultStrategy = delegate(S source)
        {

            if (typeof(S).GetInterface("ICollection")!=null){

                ICollection<E> collection = (dynamic) source;
                
                var query = (from num in
                            //这段是分组统计代码。从source的每个元素中，对元素根据元素进行分组.之后对组进行操作，返回key以及个数。
                            (from e in collection
                                group e by e
                                into g
                                select new
                                {
                                    element = g.Key,
                                    cnt = g.Count()
                                }
                                
                            )
                        // 最先的那个from num,num是统计结果。再次select返回最终结果
                        select new {num.element, num.cnt}
                        
                    ).ToList();
                Hashtable hashtable = new Hashtable();
                foreach(var e in query)
                {
                    hashtable.Add(e.element,e.cnt);
                }

                List<Tuple<E,int>> counts = new List<Tuple<E, int>>();
                foreach (var q in query)
                {
                    counts.Add(new Tuple<E, int>(q.element,q.cnt));
                }
                return (hashtable,counts);
            }
            

            return (null,null);


        };

        public Hashtable CountTable
        {
            get;
            private set;
        }
        public List<Tuple<E,int>> DataList
        {
            get;
            private set;
        }
        private S _source;
        private CountStrategy<S,E> _strategy;
        public Counter(S source, CountStrategy<S,E> strategy)
        {
            if (strategy == null)
            {
               
                _strategy = DefaultStrategy;
                
            }
            else
            {
                _strategy = strategy;
            }
           
            _source = source;
            (var countTable, var datalist) = _strategy(source);
            CountTable = countTable;
            DataList = datalist;
        }

        public void BindData(S source)
        {
            this._source = source;
        }
        
        public Hashtable ReCount()
        {
            if (_strategy == null || _source == null) return null;
            var (countTable, datalist) = _strategy(_source);
            CountTable = countTable;
            DataList = datalist;
            return CountTable;
        }


    }
}