using System;

namespace Selector
{
    public interface ITimelineItem<T>
    {
        public T Item { get; set; }
        public DateTime Time { get; set; }
    }
}
