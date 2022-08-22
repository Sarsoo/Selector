using System;

namespace Selector.Net;

public class BaseNode<T> : INode<T>
{
    public T Id { get; set; }
}

