using System.Collections.Specialized;
using Microsoft.VisualBasic;

namespace TTLCache.TTL;

class Key<TKey> : IComparable<Key<TKey>>
{
    public TKey Value;
    public DateTime ExpireDate;
    public Key(TKey value, DateTime Expire)
    {
        
        Value = value;
        ExpireDate = Expire;
    }

    public int CompareTo(Key<TKey> other)
    {
        if (Value.Equals(other.Value)) return 0;
        if (other == null || ExpireDate >= other.ExpireDate) return 1;
        if (ExpireDate < other.ExpireDate) return -1;
        return 1;
    }
}


public class TTLCacheKeyValue<TKey,TValue> where TValue : class
{
    private Dictionary<TKey, DateTime> _keyExpiers = new();
    private SortedDictionary<Key<TKey>, TValue> _dictionary = new();
    private Mutex _mutex = new Mutex();

    public void Set(TKey key, TValue value, DateTime timeDelta)
    {
        _mutex.WaitOne();
        var Now = DateTime.Now;
        var Expire = new DateTime(Now.Ticks + timeDelta.Ticks);
        var NewKey = new Key<TKey>(key, Expire);
        if (_dictionary.Count() != 0)
        {
            while (_dictionary.First().Key.ExpireDate < Now)
            {
                var First = _dictionary.First();
                _keyExpiers.Remove(First.Key.Value);
                _dictionary.Remove(First.Key);
            }
        }

        _keyExpiers[key] = Expire;
        _dictionary[NewKey] = value;
        _mutex.ReleaseMutex();
    }

    public TValue? Get(TKey key)
    {
        if (_keyExpiers.ContainsKey(key))
        {
            var Now = DateTime.Now;
            var Expire = _keyExpiers[key];
            var SearchedKey = new Key<TKey>(key, Expire);
            if (Expire < Now)
            {
                return null;
            }
            return _dictionary[SearchedKey];
        }

        throw new Exception($"Key '{key}' is not exist");
    }
}