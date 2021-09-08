using System;
using System.Collections;
using System.Collections.Specialized;

namespace CIExam.os.Cache.ReplaceStrategies
{
    public abstract class BaseCacheStrategy<TK,TV>
    {
        protected delegate bool ContentJudgeStrategy(object cacheContent, object writeContent);
           //用于判断取出的内容是否是想要的。因为可能key相同。
        protected ContentJudgeStrategy JudgeContent = (c,w) => true;
        
        protected Hashtable ParamsMap = new Hashtable();
     
        protected readonly CacheEvent LineReplaced = null;
        protected readonly CacheEvent LineMiss = null;
        protected readonly CacheEvent LineAccess = null;

       
        public abstract Object DoAccess(TK key, AbstractCache<TK,TV> cache, OrderedDictionary cacheLines,
            object param, AbstractCache<TK, TV>.DataReadCallBack callBack = null);
        public abstract Object DoWrite(TK key, TV val, AbstractCache<TK,TV> cache, OrderedDictionary cacheLines);
     
    }
}