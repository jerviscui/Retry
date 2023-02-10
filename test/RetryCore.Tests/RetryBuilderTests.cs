using Retry;
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
    }
}