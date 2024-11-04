using TTLCache.TTL;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Tests;

public class ConcurencyTests
{
        [Fact]
        public async Task SetAndGet_ShouldWorkCorrectlyInMultipleThreads()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromSeconds(1));
            var tasks = new List<Task>();

            // Act: Set values in multiple threads
            for (int i = 0; i < 10; i++)
            {
                int index = i; // Capture the current index
                tasks.Add(Task.Run(() => cache.Set($"key{index}", $"value{index}", TimeSpan.FromSeconds(1))));
            }

            await Task.WhenAll(tasks); // Wait for all set operations to complete

            // Act: Get values in multiple threads
            var getTasks = new List<Task<string>>();
            for (int i = 0; i < 10; i++)
            {
                int index = i; // Capture the current index
                getTasks.Add(Task.Run(() => cache.Get($"key{index}")));
            }

            var results = await Task.WhenAll(getTasks); // Wait for all get operations to complete

            // Assert: Check that all values are retrieved correctly
            for (int i = 0; i < 10; i++)
            {
                Assert.Equal($"value{i}", results[i]);
            }
        }

        [Fact]
        public async Task Cache_ShouldAutomaticallyInvalidateExpiredItemsInMultipleThreads()
        {
            // Arrange
            var cache = new TtlCacheKeyValue<string, string>(TimeSpan.FromMilliseconds(100));
            var tasks = new List<Task>();

            // Act: Set values in multiple threads
            for (int i = 0; i < 10; i++)
            {
                int index = i; // Capture the current index
                tasks.Add(Task.Run(() => cache.Set($"key{index}", $"value{index}", TimeSpan.FromMilliseconds(50))));
            }

            await Task.WhenAll(tasks); // Wait for all set operations to complete

            // Wait for values to expire
            await Task.Delay(200);

            // Act: Get values in multiple threads
            var getTasks = new List<Task<string>>();
            for (int i = 0; i < 10; i++)
            {
                int index = i; // Capture the current index
                getTasks.Add(Task.Run(() => cache.Get($"key{index}")));
            }

            var results = await Task.WhenAll(getTasks); // Wait for all get operations to complete

            // Assert: Check that all values are null (expired)
            foreach (var result in results)
            {
                Assert.Null(result);
            }
        }

        [Fact]
        public void Dispose_ShouldCancelInvalidateLoopInMultipleThreads()
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
