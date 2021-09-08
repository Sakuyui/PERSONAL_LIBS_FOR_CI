using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using CIExam.os.Cache.ReplaceStrategies;

namespace CIExam.os.Cache
{
    public class CacheBuilder
    {
        private CacheBuilder()
        {
            
        }
        private static CacheBuilder _instance;
        private static readonly object InstanceLock = new object();
        public static CacheBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (InstanceLock)
                {
                    if(_instance == null) _instance = new CacheBuilder();
                }
            }

            return _instance;
        }
        public static CommonCache<TK,TV> BuildFifoCommonCache<TK, TV>(int capacity, AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            CommonCache<TK, TV> cache = new CommonCache<TK, TV>(capacity, new CacheFifoStrategy<TK, TV>(), callBack);
            return cache;
        }
        public static CommonCache<TK,TV> BuildLruCommonCache<TK, TV>(int capacity, AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            var cache = new CommonCache<TK, TV>(capacity, new CacheLruStrategy<TK, TV>(), callBack);
            return cache;
        }
        public static CommonCache<TK,TV> BuildLfuCommonCache<TK, TV>(int capacity, AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            CommonCache<TK, TV> cache = new CommonCache<TK, TV>(capacity, new CacheLfuStrategy<TK, TV>(), callBack);
            return cache;
        }

        public static CommonCache<TK,TV> BuildCustomeCommonCache<TK, TV>(int capacity, BaseCacheStrategy<TK,TV> strategy, AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            CommonCache<TK,TV> cache = new CommonCache<TK, TV>(capacity, strategy, callBack);
            return cache;
        }

       
        public static CommonCache<TK,TV> BuildCustomeCommonCache<TK, TV>(int capacity, 
            CacheCustomStrategy<TK,TV>.DoAccessDelegate<TK,TV> doAccessStrategy, CacheCustomStrategy<TK,TV>.DoWriteDelegate<TK,TV> doWriteDelegate,AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            BaseCacheStrategy<TK,TV> strategy =  new CacheCustomStrategy<TK, TV>(doAccessStrategy, doWriteDelegate);
            CommonCache<TK,TV> cache = new CommonCache<TK, TV>(capacity, strategy,callBack);
            return cache;
        }
    }
}