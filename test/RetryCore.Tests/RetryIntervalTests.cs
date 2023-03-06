using Retry;
using Shouldly;

namespace RetryCore.Tests;

public class RetryIntervalTests
{
    [Fact]
    public void ConstantRetryInterval_Test()
    {
        var interval = new ConstantRetryInterval(TimeSpan.FromMilliseconds(100));

        //once
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
        //twice
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
        //three times
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void ExponentialRetryInterval_Max_Test()
    {
        var interval = new ExponentialRetryInterval(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

        //once
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
        //twice
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
        //three times
        interval.GetInterval().ShouldBe(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void ExponentialRetryInterval_Test()
    {
        var interval = new ExponentialRetryInterval(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(100));

        //once
        interval.GetInterval().Ticks.ShouldBeLessThan(TimeSpan.FromSeconds(1 * 2).Ticks);
        //twice
        interval.GetInterval().Ticks.ShouldBeLessThan(TimeSpan.FromSeconds(1 * 4).Ticks);
        //three times
        interval.GetInterval().Ticks.ShouldBeLessThan(TimeSpan.FromSeconds(1 * 8).Ticks);
    }
}
