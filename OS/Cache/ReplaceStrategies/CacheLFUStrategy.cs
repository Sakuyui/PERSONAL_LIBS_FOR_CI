using System.Collections.Generic;
using System.Collections.Specialized;

namespace CIExam.os.Cache.ReplaceStrategies
{
    public class CacheLfuStrategy<TK,TV> : BaseCacheStrategy<TK,TV>
    {
        private readonly SortedList<int, List<TK>> _frequentSetMap= new();
        private readonly Dictionary<TK, int> _keyFrequentMap = new();
        public override object DoAccess(TK key, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines, object param,AbstractCache<TK, TV>.DataReadCallBack callBack = null)
        {
            if (!cacheLines.Contains(key))
            {
                LineMiss?.Invoke(this, key, null);
                return default;
            }
            else
            {

                if (!_keyFrequentMap.ContainsKey(key))
                {
                    if (callBack == null)
                    {
                        var b = callBack != null ? callBack(key, null) : default;
                        return b;
                    }
                    var v = callBack.Invoke(key);
                    DoWrite(key, v, cache, cacheLines);
                    return v;
                }
                var c = cacheLines[key]; //根据key直接获取内容
                var freq = _keyFrequentMap[key];  //根据k获取频率
                _frequentSetMap[freq]?.Remove(key); //从该频率集合中删除key
                //key转移到下一频率
                if (_frequentSetMap.ContainsKey(freq + 1))
                {
                    _frequentSetMap[freq + 1]?.Add(key);
                }
                else
                {
                    _frequentSetMap.Add(freq + 1, new List<TK>());
                }
                //修改频率映射
                _keyFrequentMap[key] = freq + 1;
                return (TV) c;
            }
        }

        public override object DoWrite(TK key, TV val, AbstractCache<TK, TV> cache, OrderedDictionary cacheLines)
        {
            if (!cacheLines.Contains(key))
            {
                if (cacheLines.Count < cache.Capacity)
                {
                    _keyFrequentMap[key] = 1;
                    cacheLines.Add(key, val);
                    //key加入频率列表
                    if (_frequentSetMap.ContainsKey(1))
                    {
                        _frequentSetMap[1].Add(key);
                    }
                    else
                    {
                        _frequentSetMap.Add(1, new List<TK>());
                    }
                }
                else
                {
                    //需要替换惹。
                    foreach (var f in _frequentSetMap.Keys)
                    {
                        var list = _frequentSetMap[f];
                        if (list.Count <= 0) continue;
                        //替换出一个元素
                        _keyFrequentMap.Remove(list[0]);
                        cacheLines.Remove(list[0]);
                        list.RemoveAt(0);
                        //插入新元素
                        _keyFrequentMap[key] = 1;
                        cacheLines.Add(key, val);
                        //key加入频率列表
                        if (_frequentSetMap.ContainsKey(1))
                        {
                            _frequentSetMap[1].Add(key);
                        }
                        else
                        {
                            _frequentSetMap.Add(1, new List<TK>());
                        }
                        break;
                    }
                }
            }
            else
            {
                //存在key的情况
                DoAccess(key, cache, cacheLines,null);
                cacheLines[key] = val;
            }

            return cache;
        }
    }
}