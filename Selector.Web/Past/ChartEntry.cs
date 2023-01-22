using System;
using Selector.SignalR;

namespace Selector.Web;

public class ChartEntry : IChartEntry
{
    public string Name { get; set; }
    public int Value { get; set; }
}

