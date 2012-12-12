using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Caching;
using ServiceStack.Redis;

namespace RedisCacheNET.Web
{
    public class RedisOutputCacheProvider : OutputCacheProvider, IDisposable
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
            using (var cacheClient = _redisClientsManager.GetCacheClient())
            {
                cacheClient.Set(key, Serialize(entry), utcExpiry);
            }
        }

        public override object Get(string key)
        {
            byte[] cachedValBytes = null;
            using (var cacheClient = _redisClientsManager.GetCacheClient())
            {
                cachedValBytes = cacheClient.Get<byte[]>(key);
            }
            
            object cachedVal = null;
            if (cachedValBytes != null)
                cachedVal = Deserialize(cachedValBytes);
            
            return cachedVal;
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            using (var cacheClient = _redisClientsManager.GetCacheClient())
            {
                var cachedValBytes = cacheClient.Get<byte[]>(key);
                
                if (cachedValBytes != null)
                {
                    return Deserialize(cachedValBytes);
                }
                else
                {
                    cacheClient.Add(key, Serialize(entry), utcExpiry);
                }
            }

            return entry;
        }

        public override void Remove(string key)
        {
            using (var cacheClient = _redisClientsManager.GetCacheClient())
            {
                cacheClient.Remove(key);
            }
        }

        private static byte[] Serialize(object entry)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, entry);

            return stream.ToArray();
        }

        private static object Deserialize(byte[] serializedEntry)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(serializedEntry);

            return formatter.Deserialize(stream);
        }

        public void Dispose()
        {
            _redisClientsManager.Dispose();
        }
    }
}
