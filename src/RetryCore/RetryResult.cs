using System;

namespace Retry;

public class RetryResult
{
    public Exception? Exception { get; set; }

    public bool IsSuccess => Exception is null;
}

public class RetryResult<TData> : RetryResult
{
    public TData? Result { get; set; }
}
