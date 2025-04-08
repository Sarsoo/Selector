namespace Selector
{
    public interface ITimeline<T>
    {
        public int Count { get; }
        public T? Get(DateTime at);
        public T Get();
        public void Add(T item, DateTime timestamp);
        public void Clear();
    }
}