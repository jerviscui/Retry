using Retry;
using Shouldly;

namespace RetryCore.Tests
{
    public class AsyncRetryTaskTests
    {
        [Fact]
        public async Task Retrying_Cancelled_Test()
        {
            int count = 0;
            int retryTimes = 0;

            var task = new AsyncRetryTask<int>(() =>
            {
                throw new Exception();
                return Task.FromResult(1);
            }, new RetryOptions(), Array.Empty<Type>());
            task.OnRetry((_, _) => { retryTimes++; });
            task.OnFailure((_, i) => { count = i.TriedCount; });

            var retry = await task.RunAsync(new CancellationTokenSource(0).Token);

            retry.IsSuccess.ShouldBeFalse();
            retry.Exception.ShouldBeOfType<TaskCanceledException>();
            count.ShouldBe(1);
            retryTimes.ShouldBe(0);
        }

        [Fact]
        public async Task Retried_Cancelled_Test()
        {
            int count = 0;
            int retryTimes = 0;

            var cts = new CancellationTokenSource();
            var task = new AsyncRetryTask<int>(() =>
            {
                throw new Exception();
                return Task.FromResult(1);
            }, new RetryOptions(), Array.Empty<Type>());
            task.OnRetry((_, _) =>
            {
                retryTimes++;
                cts.Cancel();
            });
            task.OnFailure((_, i) => { count = i.TriedCount; });

            var retry = await task.RunAsync(cts.Token);

            retry.IsSuccess.ShouldBeFalse();
            retry.Exception.ShouldBeOfType<TaskCanceledException>();
            count.ShouldBe(2);
            retryTimes.ShouldBe(1);
        }
    }
}
