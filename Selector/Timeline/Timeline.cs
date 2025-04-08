using System.Collections;

namespace Selector
{
    public class Timeline<T> : ITimeline<T>, IEnumerable<TimelineItem<T>> where T : class
    {
        protected List<TimelineItem<T>> Recent = new();

        public int Count
        {
            get => Recent.Count;
        }

        public void Clear() => Recent.Clear();
        public bool SortOnBackDate { get; set; } = true;

        private int? max = 1000;

        public int? MaxSize
        {
            get => max;
            set => max = value is null ? value : Math.Max(1, (int)value);
        }

        public virtual void Add(T item) => Add(item, DateTime.UtcNow);

        public virtual void Add(T item, DateTime timestamp)
        {
            Recent.Add(TimelineItem<T>.From(item, timestamp));

            if (timestamp < Recent.Last().Time && SortOnBackDate)
            {
                Sort();
            }

            CheckSize();
        }

        public void Sort()
        {
            Recent = Recent.OrderBy(i => i.Time).ToList();
        }

        protected void CheckSize()
        {
            if (MaxSize is int maxSize && Count > maxSize)
            {
                Recent.RemoveRange(0, Count - maxSize);
            }
        }

        public T Get()
            => Recent.Last().Item;

        public T? Get(DateTime at)
            => GetTimelineItem(at)?.Item;

        public TimelineItem<T>? GetTimelineItem(DateTime at)
            => Recent.LastOrDefault(i => i.Time <= at);

        public IEnumerator<TimelineItem<T>> GetEnumerator() => Recent.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}