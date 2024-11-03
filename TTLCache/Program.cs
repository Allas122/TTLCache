using TTLCache.TTL;

var TTLCacheLeyValue = new TtlCacheKeyValue<string,string>(new TimeSpan(20));
TTLCacheLeyValue.Set("12","12",TimeSpan.FromHours(100));
TTLCacheLeyValue.Set("11","12",TimeSpan.FromTicks(1));
Thread.Sleep(1000);
Console.WriteLine("complete");


