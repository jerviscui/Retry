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

