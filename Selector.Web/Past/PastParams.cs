using System;
using Selector.SignalR;

namespace Selector.Web;

public class PastParams : IPastParams
{
    public string Track { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }

    public string From { get; set; }
    public string To { get; set; }
}

