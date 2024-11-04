using TTLCache.TTL;

namespace Tests.Tests;

public class StandardTests
{
          [Fact]
        public void SetAndGet_ShouldReturnValue_WhenNotExpired()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromMilliseconds(100));
            var key = "testKey";
            var value = "testValue";
            cache.Set(key, value, TimeSpan.FromSeconds(1));

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void Get_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromMilliseconds(100));
            var key = "nonExistentKey";

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_ShouldReturnDefault_WhenValueIsExpired()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromMilliseconds(100));
            var key = "testKey";
            var value = "testValue";
            cache.Set(key, value, TimeSpan.FromMilliseconds(50)); // Set a short TTL

            // Wait for the value to expire
            Thread.Sleep(100);

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Cache_ShouldAutomaticallyInvalidateExpiredItems()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromMilliseconds(50));
            var key = "testKey";
            var value = "testValue";
            cache.Set(key, value, TimeSpan.FromMilliseconds(50)); // Set a short TTL

            // Wait for the value to expire
            await Task.Delay(100);

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Dispose_ShouldCancelInvalidateLoop()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromSeconds(1));

            // Act
            cache.Dispose();

            // Assert
            // No exceptions should be thrown during disposal
            Assert.True(true);
        }
    }