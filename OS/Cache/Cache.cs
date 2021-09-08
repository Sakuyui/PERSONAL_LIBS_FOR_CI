using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CIExam.os.Cache.ReplaceStrategies;
using CIExam.FunctionExtension;

/*LFU存在Bug*/
namespace CIExam.os.Cache
{
    public class Page
    {
        
    }
    
    public delegate object CacheEvent(object source, object key, object content);

    public abstract class AbstractCache<TK,TV> : IEnumerable<DictionaryEntry>
    {
        //<key,val> val可以是page

        protected int Size => CacheLines.Count;
        public readonly int Capacity;
       
        protected readonly BaseCacheStrategy<TK,TV> CacheStrategy;
        protected delegate bool ContentJudgeStrategy(object cacheContent, object writeContent);
        
        //用于判断取出的内容是否是想要的。因为可能key相同。
        protected ContentJudgeStrategy JudgeContent = (c,w) => true;
        
        //OrderedDictionary
        protected readonly OrderedDictionary CacheLines = new OrderedDictionary();


        public bool ContainsKey(TK key)
        {
            return CacheLines.Contains(key);
        }
        
        protected AbstractCache(int capacity, BaseCacheStrategy<TK,TV> strategy, DataReadCallBack readCallBack = null)
        {
            Capacity = capacity;
            CacheStrategy = strategy;
            ReadCallBack = readCallBack;
        }
        public delegate TV DataReadCallBack(TK key, params object[] param);

        public DataReadCallBack ReadCallBack;
        public abstract TV Access(TK key, object param, DataReadCallBack callBack = null);   //访问某个页
        public abstract void Write(TK key, TV val);  //加入一个新的页面 相当于提供 （页号，对应页内容）


        public IEnumerator<DictionaryEntry> GetEnumerator()
        {
            return CacheLines.Cast<DictionaryEntry>().GetEnumerator();
        }

        public override string ToString()
        {
            return this.Select(e => "[key: " + e.Key + ", val: " + e.Value + "]").ToEnumerationString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    
   
    
}