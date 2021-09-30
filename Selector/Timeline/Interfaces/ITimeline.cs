﻿using System;

namespace Selector
{
    public interface ITimeline<T>
    {
        public int Count { get; set; }
        public T Get(DateTime at);
        public T Get();
        public void Add(T item, DateTime timestamp);
        public void Clear();
    }
}
