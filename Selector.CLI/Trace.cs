using System.Diagnostics;

namespace Selector;

public static class Trace
{
    public static ActivitySource Tracer { get; } = new(typeof(Trace).Assembly.GetName().Name ?? "Unknown",
        typeof(Trace).Assembly.GetName().Version?.ToString() ?? "Unknown");
}