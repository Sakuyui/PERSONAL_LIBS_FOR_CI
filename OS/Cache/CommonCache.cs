using System.Collections.Specialized;
using CIExam.os.Cache.ReplaceStrategies;
using CIExam.FunctionExtension;

namespace CIExam.os.Cache
{
    public class CommonCache<TK,TV> : AbstractCache<TK,TV>
    {
        public CommonCache(int capacity, BaseCacheStrategy<TK, TV> strategy, DataReadCallBack callBack) : base(capacity, strategy, callBack)
        {
            
        }

       

        public override TV Access(TK key, object param, DataReadCallBack callBack = null)
        {
         
            return (TV) CacheStrategy.DoAccess(key, this, CacheLines, callBack ?? ReadCallBack);
        }

        public override void Write(TK key, TV val)
        {
            CacheStrategy.DoWrite(key, val, this, CacheLines);
        }
    }
}