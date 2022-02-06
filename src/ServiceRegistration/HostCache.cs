using Microsoft.Extensions.Caching.Memory;

public class HostCache
{
    private readonly IMemoryCache _cache;
    private readonly List<SiloHost> _hosts = new();
    private readonly object _lock = new();

    public HostCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IEnumerable<SiloHost> GetHosts()
    {
        return _hosts;
    }

    public SiloHost Register(string host, int port)
    {
        var key = GetKey(host, port);

        if (!_cache.TryGetValue(key, out SiloHost silo))
        {
            silo = new SiloHost(host, port);

            lock (_lock)
            {
                _hosts.Add(silo);
                _cache.Set(
                    key,
                    silo,
                    new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(5),
                        PostEvictionCallbacks =
                            { new PostEvictionCallbackRegistration { EvictionCallback = EvictionCallback } }
                    });
            }
        }

        return silo;
    }

    private void EvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        lock (_lock)
        {
            this._hosts.Remove((SiloHost)value);
        }
    }

    private string GetKey(string host, int port)
    {
        return $"{host}:{port}";
    }
}