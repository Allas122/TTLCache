﻿using System.Collections.Concurrent;
using System.Collections.Specialized;
using Microsoft.VisualBasic;

namespace TTLCache.TTL;

public class ValueWrapper<TValue>
{
    public TValue value { get; set; }
    public DateTime ExpireDate { get; set; }
}


public class TtlCacheKeyValue<TKey, TValue> : IDisposable
{
    private ConcurrentDictionary<TKey, ValueWrapper<TValue>?> _dictionary = new();
    private TimeSpan _timeSpanBetweenInvalidate;
    private CancellationTokenSource _cts = new();
    
    public TtlCacheKeyValue(TimeSpan timeSpanBetweenInvalidate)
    {
        var ct = _cts.Token;
        _timeSpanBetweenInvalidate = timeSpanBetweenInvalidate; 
        Task.Run(()=>StartInvalidateCacheLoop(ct),ct);
    }
    
    public void Set(TKey key,TValue value,TimeSpan span)
    {
        var now = DateTime.Now;
        _dictionary[key] = new ValueWrapper<TValue>{value = value, ExpireDate = now + span};
    }
    
    public TValue Get(TKey key)
    {
        if (_dictionary.TryGetValue(key,out var value))
        {
            return value.value;
        }
        return default;
    }

    private async Task StartInvalidateCacheLoop(CancellationToken ct)
    {
        while (ct.IsCancellationRequested==false)
        {
            await Task.Delay(_timeSpanBetweenInvalidate);
            DateTime now = DateTime.Now;
            var ExpiredPair =_dictionary
                .Where(pair => pair.Value.ExpireDate < now)
                .Select(x => x.Key)
                .ToList();
            ExpiredPair.ForEach(k=>_dictionary.Remove(k,out _));
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}