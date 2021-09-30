using System;

namespace Selector
{
    public class Timeline<T> : ITimeline<T>
    {
        public int Count { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Add(T item, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public T Get(DateTime at)
        {
            throw new NotImplementedException();
        }

        public T Get()
        {
            throw new NotImplementedException();
        }
    }
}
