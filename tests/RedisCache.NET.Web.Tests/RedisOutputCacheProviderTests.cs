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
        }
        
        // Use TestCleanup to run code after each test has run
        // [TestCleanup]
        // public void TestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Set_Simple_Success()
        {
            string key = "TestKey";
            int entry = 32;
            DateTime expiry = DateTime.UtcNow.AddDays(1);

            _provider.Set(key, entry, expiry);
        }

        [TestMethod]
        public void Set_Complex_Success()
        {
            string key = "TestComplex";
            SampleComplexType entry = new SampleComplexType
            {
                IntValue = 23,
                StringValue = "TestString",
                Nested = new NestedType
                {
                    NestedIntValue = 11
                }
            };
            DateTime expiry = DateTime.UtcNow.AddHours(1);

            _provider.Set(key, entry, expiry);
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
                    NestedIntValue = 11
                }
            };

            SampleComplexType cachedVal = _provider.Get(key) as SampleComplexType;

            Assert.IsNotNull(cachedVal);
            Assert.AreEqual(23, cachedVal.IntValue);
            Assert.AreEqual("TestString", cachedVal.StringValue);
            Assert.AreEqual(11, cachedVal.Nested.NestedIntValue);
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
    }
}
