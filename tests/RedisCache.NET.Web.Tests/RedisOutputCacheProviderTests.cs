using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Redis;

namespace RedisCacheNET.Web.Tests
{
    /// <summary>
    /// Summary description for RedisOutputCacheProviderTests
    /// </summary>
    [TestClass]
    public class RedisOutputCacheProviderTests
    {
        public RedisOutputCacheProviderTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private IRedisClientsManager _redisClientsManager;
        private RedisOutputCacheProvider _provider;
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize]
        // public static void ClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup]
        // public static void ClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void TestInitialize()
        {
            _redisClientsManager = new PooledRedisClientManager(new string[] { "127.0.0.1" });
            _provider = new RedisOutputCacheProvider(_redisClientsManager);

            // Add test objects to cache
            SetupTestCache();
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void TestCleanup() 
        {
            _provider.Dispose();
        }
        
        #endregion

        private void SetupTestCache()
        {
            string key = "TestKey";
            int entry = 32;
            DateTime expiry = DateTime.UtcNow.AddDays(1);

            _provider.Add(key, entry, expiry);

            key = "TestComplex";
            var complexEntry = new SampleComplexType
            {
                IntValue = 23,
                StringValue = "TestString",
                Nested = new NestedType
                {
                    NestedIntValue = 11,
                    NestedStringValue = "NestedTestString"
                }
            };
            expiry = DateTime.UtcNow.AddHours(1);

            _provider.Add(key, complexEntry, expiry);
        }

        [TestMethod]
        public void Get_Simple_Success()
        {
            string key = "TestKey";

            object cachedVal = _provider.Get(key);

            Assert.AreEqual(32, cachedVal);
        }

        [TestMethod]
        public void Get_Complex_Success()
        {
            string key = "TestComplex";
            SampleComplexType entry = new SampleComplexType
            {
                IntValue = 23,
                StringValue = "TestString",
                Nested = new NestedType
                {
                    NestedIntValue = 11,
                    NestedStringValue = "NestedTestString"
                }
            };

            SampleComplexType cachedVal = _provider.Get(key) as SampleComplexType;

            Assert.IsNotNull(cachedVal);
            Assert.AreEqual(23, cachedVal.IntValue);
            Assert.AreEqual("TestString", cachedVal.StringValue);
            Assert.AreEqual(11, cachedVal.Nested.NestedIntValue);
            Assert.AreEqual("NestedTestString", cachedVal.Nested.NestedStringValue);
        }
        
        [TestMethod]
        public void Get_Key_Not_Found()
        {
            object cachedVal = _provider.Get("InvalidKey");

            Assert.IsNull(cachedVal);
        }

        [TestMethod]
        public void Remove_Cached_Value()
        {
            string key = "ToRemove";
            int entry = 7;

            _provider.Set(key, entry, DateTime.UtcNow.AddHours(1));

            // Ensure object was cached
            object cachedValue = _provider.Get(key);

            Assert.IsNotNull(cachedValue);

            _provider.Remove(key);

            cachedValue = _provider.Get(key);

            Assert.IsNull(cachedValue);
        }

        [TestMethod]
        public void Remove_Nonexistent_Key()
        {
            var shouldBeNull = _provider.Get("DoesNotExist");

            Assert.IsNull(shouldBeNull);
        }

        [TestMethod]
        public void Add_Key_Already_Exists()
        {
            var result = _provider.Add("TestKey", "This won't get stored.", DateTime.UtcNow.AddHours(1));

            Assert.AreEqual(32, result);
        }

        public void Set_New_Key()
        {
            var key = "NewKey";
            string newVal = "New Value";

            _provider.Set(key, newVal, DateTime.UtcNow.AddHours(1));

            var storedVal = _provider.Get(key);

            Assert.AreEqual(newVal, storedVal);
        }

        [TestMethod]
        public void Set_Update_Existing()
        {
            var key = "ToUpdate";

            _provider.Add(key, "Original", DateTime.UtcNow.AddHours(1));

            string updatedVal = "Updated";

            _provider.Set(key, updatedVal, DateTime.UtcNow.AddHours(1));

            var storedVal = _provider.Get(key);

            Assert.AreEqual(updatedVal, storedVal);
        }
    }

    [Serializable]
    public class SampleComplexType
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public NestedType Nested { get; set; }
    }

    [Serializable]
    public class NestedType
    {
        public int NestedIntValue { get; set; }
        public string NestedStringValue { get; set; }
    }
}
