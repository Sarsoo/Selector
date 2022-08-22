using System;

namespace Selector.Net;

public abstract class BaseSingleSink<TNodeId, TObj>: BaseNode<TNodeId>, ISink<TNodeId, TObj>
{
    public IEnumerable<string> Topics { get; set; }

    public Task Consume(object obj)
    {
        var type = GetType();
        var genericArgs = type.GetGenericArguments();
        var objType = obj.GetType();

        if (objType.IsAssignableTo(genericArgs[1]))
        {
            var objCast = (TObj)obj;

            return ConsumeType(objCast);
        }

        return Task.CompletedTask;
    }

    public abstract Task ConsumeType(TObj obj);
}

