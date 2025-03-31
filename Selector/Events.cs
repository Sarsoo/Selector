using System;

namespace Selector;

public class ListeningChangeEventArgs : EventArgs
{
    /// <summary>
    /// String Id for watcher, used to hold user Db Id
    /// </summary>
    /// <value></value>
    public string Id { get; set; }
}