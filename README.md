# Retry

A library for retrying operations

## Samples

```csharp
var result = RetryBuilder.Default.Build(() => { i = value; }).Run();

result.IsSuccess
result.Result
```

```csharp
var result = await RetryBuilder.Default.Build(() => Task.Run(() => { i = value; })).RunAsync();

result.IsSuccess
result.Result
```

### IRetryIntervalStrategy

```csharp
var builder = RetryBuilder.Default.ConfigureOptions(options =>
{
    options.RetryInterval = new ConstantRetryInterval(TimeSpan.FromMilliseconds(100));
});

var builder = RetryBuilder.Default.ConfigureOptions(options =>
{
    options.RetryInterval =
        new ExponentialRetryInterval(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(60));
}); 
```

