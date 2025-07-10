using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Selector.Cache;

public class BaggageWrapper
{
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Message { get; set; }
}

public struct DeserializedActivity<T> : IDisposable
{
    public T Object { get; set; }
    public Activity Activity { get; set; }

    public void Dispose()
    {
        Activity?.Dispose();
    }
}