using System;

namespace Selector
{
    public class TimelineItem<T>: ITimelineItem<T>
    {
        public T Item { get; set; }
        public DateTime Time { get; set; }

        public static TimelineItem<TT> From<TT>(TT item, DateTime time)
        {
            return new TimelineItem<TT>()
            {
                Item = item,
                Time = time
            };
        }
    }
}
