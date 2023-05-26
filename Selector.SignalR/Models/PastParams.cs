using System;
using Selector.SignalR;

namespace Selector.SignalR;

public class PastParams
{
    public string Track { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }

    public string From { get; set; }
    public string To { get; set; }
}

