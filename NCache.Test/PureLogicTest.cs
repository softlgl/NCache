using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NCache.Json;
using NCache.Test.Model;
using NCache.Utils;
using Xunit;

namespace NCache.Test
{
    /// <summary>
    /// 纯逻辑单元测试：不依赖任何外部服务（Redis 等），覆盖 key 生成、返回类型包装、Json 转换与序列化等核心逻辑。
    /// </summary>
    public class PureLogicTest
    {
        #region CacheAspectUtils.GenerateKey

        [Theory]
        [InlineData(null, "")]
        [InlineData("a", "a")]
        public void GenerateKey_Basic(object value, string expected)
        {
            Assert.Equal(expected, CacheAspectUtils.GenerateKey(value));
        }

        [Fact]
        public void GenerateKey_MultipleParams_JoinWithSeparator()
        {
            Assert.Equal("a|b|c", CacheAspectUtils.GenerateKey("a", "b", "c"));
        }

        [Fact]
        public void GenerateKey_NumberParams_ToString()
        {
            Assert.Equal("1|2", CacheAspectUtils.GenerateKey(1, 2));
        }

        [Fact]
        public void GenerateKey_NullItem_IsFiltered()
        {
            Assert.Equal("a|b", CacheAspectUtils.GenerateKey("a", null, "b"));
        }

        [Fact]
        public void GenerateKey_Array_IsFlattened()
        {
            Assert.Equal("x|y", CacheAspectUtils.GenerateKey(new[] { "x", "y" }));
        }

        [Fact]
        public void GenerateKey_List_IsFlattened()
        {
            Assert.Equal("1|2", CacheAspectUtils.GenerateKey(new List<int> { 1, 2 }));
        }

        [Fact]
        public void GenerateKey_String_IsNotTreatedAsEnumerable()
        {
            Assert.Equal("abc", CacheAspectUtils.GenerateKey("abc"));
        }

        #endregion

        #region CacheAspectUtils 返回类型包装

        [Fact]
        public void TaskResultFunc_WrapsValueIntoTaskOfT()
        {
            Func<object, object> func = CacheAspectUtils.TaskResultFunc(typeof(int));
            object wrapped = func(42);
            Assert.IsAssignableFrom<Task<int>>(wrapped);
            Assert.Equal(42, ((Task<int>)wrapped).Result);
        }

        [Fact]
        public void TaskResultFunc_WorksForString()
        {
            Func<object, object> func = CacheAspectUtils.TaskResultFunc(typeof(string));
            object wrapped = func("hello");
            Assert.Equal("hello", ((Task<string>)wrapped).Result);
        }

        [Fact]
        public void ValueTaskResultFunc_WrapsValueIntoValueTaskOfT()
        {
            Func<object, object> func = CacheAspectUtils.ValueTaskResultFunc(typeof(int));
            object wrapped = func(7);
            Assert.IsAssignableFrom<ValueTask<int>>(wrapped);
            Assert.Equal(7, ((ValueTask<int>)wrapped).AsTask().Result);
        }

        #endregion

        #region CacheAspectUtils.GetKeyLock

        [Fact]
        public void GetKeyLock_SameKey_ReturnsSameLock()
        {
            SemaphoreSlim lock1 = CacheAspectUtils.GetKeyLock("key-x");
            SemaphoreSlim lock2 = CacheAspectUtils.GetKeyLock("key-x");
            Assert.Same(lock1, lock2);
        }

        #endregion

        #region KeyHelper

        [Fact]
        public void KeyBuilder_ContainsPrefixAnd32HexMd5()
        {
            string key = KeyHelper.KeyBuilder("Person", 1);
            Assert.StartsWith("Person:", key);
            // 前缀 + ':' + 32位MD5
            Assert.Equal("Person:".Length + 32, key.Length);
            Assert.True(key.Skip("Person:".Length).All(c => Uri.IsHexDigit(c)));
        }

        [Fact]
        public void KeyBuilder_SameParams_ProducesSameKey()
        {
            string key1 = KeyHelper.KeyBuilder("P", "a", 1);
            string key2 = KeyHelper.KeyBuilder("P", "a", 1);
            Assert.Equal(key1, key2);
        }

        [Fact]
        public void KeyBuilder_DifferentParams_ProducesDifferentKey()
        {
            string key1 = KeyHelper.KeyBuilder("P", 1);
            string key2 = KeyHelper.KeyBuilder("P", 2);
            Assert.NotEqual(key1, key2);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(60)]
        public void KeyExpirationRandom_ShortExpiration_NoJitter(int expiration)
        {
            Assert.Equal(expiration, KeyHelper.KeyExpirationRandom(expiration));
        }

        [Theory]
        [InlineData(61, 59)]
        [InlineData(600, 59)]
        public void KeyExpirationRandom_MediumExpiration_JitterWithin60(int expiration, int maxJitter)
        {
            int result = KeyHelper.KeyExpirationRandom(expiration);
            Assert.InRange(result, expiration, expiration + maxJitter);
        }

        [Fact]
        public void KeyExpirationRandom_HourExpiration_JitterWithin300()
        {
            int result = KeyHelper.KeyExpirationRandom(601);
            Assert.InRange(result, 601, 601 + 299);
        }

        [Fact]
        public void KeyExpirationRandom_LongExpiration_JitterWithin600()
        {
            int result = KeyHelper.KeyExpirationRandom(3601);
            Assert.InRange(result, 3601, 3601 + 599);
        }

        [Fact]
        public void IntRandom_ReturnsValueWithinRange()
        {
            int result = KeyHelper.IntRandom(5, 8);
            Assert.InRange(result, 5, 7);
        }

        #endregion

        #region MD5Helper

        [Theory]
        [InlineData("abc", "900150983CD24FB0D6963F7D28E17F72")]
        [InlineData("", "D41D8CD98F00B204E9800998ECF8427E")]
        public void Get32MD5_ProducesUppercase32Hex(string source, string expected)
        {
            Assert.Equal(expected, MD5Helper.Get32MD5(source));
        }

        #endregion

        #region TypeHelper.IsEnumerable

        [Theory]
        [InlineData(typeof(int[]), true)]
        [InlineData(typeof(List<int>), true)]
        [InlineData(typeof(IEnumerable<int>), true)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        public void IsEnumerable_Classification(Type type, bool expected)
        {
            Assert.Equal(expected, type.IsEnumerable());
        }

        #endregion

        #region JsonSerializerExtensions

        [Fact]
        public void SerializeObject_DeserializeObject_RoundTrip()
        {
            Person person = new Person { Id = 5, Name = "liguoliang", Sex = 1, Birthday = new DateTime(1992, 12, 11) };
            string json = person.SerializeObject();
            Person actual = json.DeserializeObject<Person>();
            Assert.Equal(person.Id, actual.Id);
            Assert.Equal(person.Name, actual.Name);
            Assert.Equal(person.Birthday, actual.Birthday);
        }

        [Fact]
        public void SerializeBytes_DeserializeBytes_RoundTrip()
        {
            Person person = new Person { Id = 9, Name = "x" };
            byte[] bytes = person.SerializeBytes();
            Person actual = bytes.DeserializeBytes<Person>();
            Assert.Equal(person.Id, actual.Id);
        }

        #endregion

        #region DateTimeJsonConverter

        [Fact]
        public void DateTimeJsonConverter_FormatsToConfiguredString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeJsonConverter());

            DateTime value = new DateTime(2023, 5, 6, 13, 14, 15);
            string json = JsonSerializer.Serialize(value, options);

            Assert.Equal("\"2023-05-06 13:14:15\"", json);
        }

        [Fact]
        public void DateTimeJsonConverter_ParsesFormattedString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeJsonConverter());

            DateTime value = JsonSerializer.Deserialize<DateTime>("\"2023-05-06 13:14:15\"", options);

            Assert.Equal(new DateTime(2023, 5, 6, 13, 14, 15), value);
        }

        #endregion

        #region LongJsonConverter

        [Fact]
        public void LongJsonConverter_WritesAsString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new LongJsonConverter());

            string json = JsonSerializer.Serialize(123456789L, options);

            Assert.Equal("\"123456789\"", json);
        }

        [Fact]
        public void LongJsonConverter_ReadsNumericToken()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new LongJsonConverter());

            long value = JsonSerializer.Deserialize<long>("123", options);

            Assert.Equal(123L, value);
        }

        [Fact]
        public void LongJsonConverter_ReadsStringNumber()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new LongJsonConverter());

            long value = JsonSerializer.Deserialize<long>("\"456\"", options);

            Assert.Equal(456L, value);
        }

        [Fact]
        public void LongJsonConverter_InvalidString_Throws()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new LongJsonConverter());

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<long>("\"abc\"", options));
        }

        #endregion
    }
}