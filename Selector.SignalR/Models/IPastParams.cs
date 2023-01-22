namespace Selector.SignalR;

public interface IPastParams
{
    string Track { get; set; }
    string Album { get; set; }
    string Artist { get; set; }
    string From { get; set; }
    string To { get; set; }
}