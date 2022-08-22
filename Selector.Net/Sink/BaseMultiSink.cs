using System;

namespace Selector.Net;

//public abstract class BaseMultiSink<TNodeId, TObj>: BaseNode<TNodeId>, ISink<TNodeId>
//{
//    public IEnumerable<string> Topics { get; }

//    public IDictionary<Type, Action<object>> Callbacks { get; set; }


//    public Task Consume(object obj)
//    {
//        var objType = obj.GetType();

//        if(Callbacks.ContainsKey(objType))
//        {
//            var callback = Callbacks[objType];

//            var objCast = (TObj)obj;

//            return ConsumeType(objCast);
//        }
//        else
//        {
//            throw new ArgumentException("Not of acceptable payload type");
//        }
//    }
//}

