using System;
using System.Collections.Specialized;

namespace CIExam.os.Cache.ReplaceStrategies
{
    public class CacheCustomStrategy<TK,TV> : BaseCacheStrategy<TK,TV>
    {
        public delegate object DoAccessDelegate<TK,TV>(TK key, AbstractCache<TK,TV> cache, OrderedDictionary cacheLines, object param,
            AbstractCache<TK, TV>.DataReadCallBack callBack = null);
        public delegate object DoWriteDelegate<TK,TV>(TK key, TV val, AbstractCache<TK,TV> cache, OrderedDictionary cacheLines);

        private DoAccessDelegate<TK, TV> _doAccessDelegate;
        private DoWriteDelegate<TK, TV> _doWriteDelegate;

        public CacheCustomStrategy(DoAccessDelegate<TK, TV> doAccessDelegate, DoWriteDelegate<TK, TV> doWriteDelegate)
        {
            _doAccessDelegate = doAccessDelegate;
            _doWriteDelegate = doWriteDelegate;
        }

        public override object DoAccess(TK key, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines,object param, AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            return _doAccessDelegate(key, cache, cacheLines,param, callBack);
        }

        public override object DoWrite(TK key, TV val, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines)
        {
            return _doWriteDelegate(key, val, cache, cacheLines);
        }
    }
}