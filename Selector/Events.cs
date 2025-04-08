namespace Selector;

public class ListeningChangeEventArgs : EventArgs
{
    /// <summary>
    /// String Id for watcher, used to hold user Db Id
    /// </summary>
    /// <value></value>
    public required string Id { get; set; }
}