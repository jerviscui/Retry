using Retry;
using Retry.Exceptions;
using Shouldly;

namespace RetryCore.Tests
{
    public class RetryBuilderTests
    {
        [Fact]
        public void TryAction_Test()
        {
            int i = 0;
            int value = 10;

            var r = RetryBuilder.Default.Build(() => { i = value; }).Run();

            r.IsSuccess.ShouldBeTrue();
            i.ShouldBe(value);
        }

        [Fact]
        public void TryFunction_Test()
        {
            int value = 10;

            var r = RetryBuilder.Default.Build(() => value).Run();

            r.IsSuccess.ShouldBeTrue();
            r.Result.ShouldBe(value);
        }

        [Fact]
        public async Task TryAsync_Test()
        {
            int i = 0;
            int value = 10;

            var r = await RetryBuilder.Default.Build(() => Task.Run(() => { i = value; })).RunAsync();

            r.IsSuccess.ShouldBeTrue();
            i.ShouldBe(value);
        }

        [Fact]
        public async Task TryAsyncFunction_Test()
        {
            int value = 10;

            var r = await RetryBuilder.Default.Build(() => Task.Run(() => value)).RunAsync();

            r.IsSuccess.ShouldBeTrue();
            r.Result.ShouldBe(value);
        }

        [Fact]
        public async Task TryAction_AsAsync_Test()
        {
            int i = 0;
            int value = 10;

            var r = await RetryBuilder.Default.Build(() => { i = value; }).AsAsync().RunAsync();

            r.IsSuccess.ShouldBeTrue();
            i.ShouldBe(value);
        }

        [Fact]
        public async Task TryFunction_AsAsync_Test()
        {
            int value = 10;

            var r = await RetryBuilder.Default.Build(() => value).AsAsync().RunAsync();

            r.IsSuccess.ShouldBeTrue();
            r.Result.ShouldBe(value);
        }

        [Fact]
        public void TryFunction_OverMaxTryTimeException_Test()
        {
            var r = RetryBuilder.Default.ConfigureOptions(options =>
            {
                options.MaxTryCount = int.MaxValue;
                options.MaxTryTime = TimeSpan.FromSeconds(1);
            }).Build(() =>
            {
                throw new Exception();
                return 1;
            }).Run();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<OverMaxTryTimeException>();
        }

        [Fact]
        public async Task TryAsyncFunction_OverMaxTryTimeException_Test()
        {
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
            {
                options.MaxTryCount = int.MaxValue;
                options.MaxTryTime = TimeSpan.FromSeconds(1);
            }).Build(() =>
            {
                throw new Exception();
                return Task.Run(() => 1);
            }).RunAsync();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<OverMaxTryTimeException>();
        }

        [Fact]
        public void TryFunction_OverMaxTryCountException_Test()
        {
            int count = 0;
            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() =>
                {
                    throw new Exception();
                    return 1;
                })
                .OnFailure((result, i) => count = i)
                .Run();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<OverMaxTryCountException>();
            ((OverMaxTryCountException)r.Exception).TriedCount.ShouldBe(count);
        }

        [Fact]
        public async Task TryAsyncFunction_OverMaxTryCountException_Test()
        {
            int count = 0;
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                }).Build(() =>
                {
                    throw new Exception();
                    return Task.Run(() => 1);
                })
                .OnFailure((result, i) => count = i)
                .RunAsync();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<OverMaxTryCountException>();
            ((OverMaxTryCountException)r.Exception).TriedCount.ShouldBe(count);
        }

        [Fact]
        public void TryFunction_RetryCallbackException_Test()
        {
            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .RetryOnException<TimeoutException>()
                .Build(() =>
                {
                    throw new TimeoutException();
                    return 1;
                })
                .OnRetry(result => throw new Exception())
                .Run();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<RetryCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public async Task TryAsyncFunction_RetryCallbackException_Test()
        {
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .RetryOnException<TimeoutException>()
                .Build(() =>
                {
                    throw new TimeoutException();
                    return Task.Run(() => 1);
                })
                .OnRetryAsync(result => throw new Exception())
                .RunAsync();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<RetryCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public void TryFunction_FailureCallbackException_Test()
        {
            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .RetryOnException<TimeoutException>()
                .Build(() =>
                {
                    throw new TimeoutException();
                    return 1;
                })
                .OnFailure(result => throw new Exception())
                .Run();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<FailureCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public async Task TryAsyncFunction_FailureCallbackException_Test()
        {
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .RetryOnException<TimeoutException>()
                .Build(() =>
                {
                    throw new TimeoutException();
                    return Task.Run(() => 1);
                })
                .OnFailureAsync(result => throw new Exception())
                .RunAsync();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<FailureCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public void TryFunction_SuccessCallbackException_Test()
        {
            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() => 1)
                .OnSuccess(result => throw new Exception())
                .Run();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<SuccessCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public async Task TryAsyncFunction_SuccessCallbackException_Test()
        {
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() =>
                {
                    return Task.Run(() => 1);
                })
                .OnSuccessAsync(result => throw new Exception())
                .RunAsync();

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<SuccessCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public void TryFunction_Assert_Test()
        {
            int? value = null;
            int count = 0;

            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() => value)
                .OnRetry((result, i) => count = i)
                .Run(result =>
                {
                    var stop = result.Result != null;
                    value = 1;

                    return stop;
                });

            count.ShouldBe(1);
            r.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TryAsyncFunction_Assert_Sync_Test()
        {
            int? value = null;
            int count = 0;

            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() => Task.FromResult(value))
                .OnRetry((result, i) => count = i)
                .RunAsync(result =>
                {
                    var stop = result.Result != null;
                    value = 1;

                    return stop;
                });

            count.ShouldBe(1);
            r.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void TryFunction_AssertCallbackException_Test()
        {
            var r = RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() => 1)
                .Run(result => throw new Exception());

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<AssertCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }

        [Fact]
        public async Task TryAsyncFunction_AssertCallbackException_Async_Test()
        {
            var r = await RetryBuilder.Default.ConfigureOptions(options =>
                {
                    options.MaxTryCount = 2;
                })
                .Build(() =>
                {
                    return Task.Run(() => 1);
                })
                .RunAsync(result =>
                {
                    throw new Exception();
                    return Task.FromResult(true);
                });

            r.IsSuccess.ShouldBeFalse();
            r.Exception.ShouldBeOfType<AssertCallbackException>();
            r.Exception.InnerException.ShouldBeOfType<Exception>();
        }
    }
}
