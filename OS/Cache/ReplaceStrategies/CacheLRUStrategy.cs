using System;
using System.Collections.Specialized;
using CIExam.FunctionExtension;
using CIExam.OS;

namespace CIExam.os.Cache.ReplaceStrategies
{
    public class CacheLruStrategy<TK, TV> : BaseCacheStrategy<TK,TV>
    {
   

        public override object DoAccess(TK key, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines, object param,AbstractCache<TK ,TV>.DataReadCallBack callBack)
        {
            
            callBack ??= cache.ReadCallBack;
            
            if (cacheLines.Contains(key))
            {
                //处理
                var e = cacheLines[key];
                cacheLines.Remove(key);
                cacheLines.Insert(0,key,e);
                return (TV)e;
            }

            $"cache miss with key : {key}".PrintToConsole();
            if(callBack == null)
                return default;
            var v = callBack.Invoke(key);
            DoWrite(key, v, cache, cacheLines);
            return v;
        }

        public override object DoWrite(TK key, TV val, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines)
        {
            //缓存满
            if (cacheLines.Count == cache.Capacity)
            {
                if (cacheLines.Contains(key))
                {
                    cacheLines.Remove(key);
                    cacheLines.Insert(0,key,val);
                    return true;
                }
                
                //替换策略
                LineReplaced?.Invoke(this, key, val);
                cacheLines.RemoveAt(cacheLines.Count - 1);
                cacheLines.Insert(0, key, val);
            }
            else
            {
                if (cacheLines.Contains(key))
                {
                    cacheLines.Remove(key);
                    cacheLines.Insert(0, key,val);
                    return cacheLines.Count;
                }

                LineMiss?.Invoke(this, key, val);
                cacheLines.Insert(0, key,val);
            }

            return cacheLines.Count;
        }

        
    }
}