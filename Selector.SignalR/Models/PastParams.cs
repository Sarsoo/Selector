namespace Selector.SignalR;

public class PastParams
{
    public required string Track { get; set; }
    public required string Album { get; set; }
    public required string Artist { get; set; }

    public required string From { get; set; }
    public required string To { get; set; }
}