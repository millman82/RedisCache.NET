using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using ServiceStack.Redis;
using System.Configuration;

namespace RedisCacheNET.Web
{
    public class RedisOutputCacheProvider : OutputCacheProvider
    {
        static IRedisClientsManager _redisClientsManager;
        private object _lockObj = new object();

        public RedisOutputCacheProvider()
        {
            lock (_lockObj)
            {
                if (_redisClientsManager == null)
                {
                    ConnectionStringsSection connectionStringsSection = new ConnectionStringsSection();
                    string connString = connectionStringsSection.ConnectionStrings["RedisCacheConnection"].ConnectionString;

                    _redisClientsManager = new PooledRedisClientManager(new string[] { connString });
                }
            }
        }

        public RedisOutputCacheProvider(IRedisClientsManager redisClientsManager)
        {
            lock (_lockObj)
            {
                if (_redisClientsManager == null)
                {
                    _redisClientsManager = redisClientsManager;
                }
            }
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            var cacheClient = _redisClientsManager.GetCacheClient();
            cacheClient.Set(key, entry, utcExpiry);

            cacheClient.Dispose();
        }

        public override object Get(string key)
        {
            var cacheClient = _redisClientsManager.GetCacheClient();
            object item = cacheClient.Get<object>(key);

            cacheClient.Dispose();
            
            return item;
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            var cacheClient = _redisClientsManager.GetCacheClient();

            var item = cacheClient.Get<object>(key);
            if (item != null)
            {
                return item;
            }
            else
            {
                cacheClient.Add(key, entry, utcExpiry);
            }

            cacheClient.Dispose();

            return entry;
        }

        public override void Remove(string key)
        {
            var cacheClient = _redisClientsManager.GetCacheClient();
            cacheClient.Remove(key);
            cacheClient.Dispose();
        }
    }
}
