﻿using System;
using System.Collections.Generic;
using System.Text;
using SpotifyAPI.Web;

namespace Selector
{
    public class Equal : IEqual
    {
        protected Dictionary<Type, object> comps;

        public bool IsEqual<T>(T item, T other)
        {
            if (comps.ContainsKey(typeof(T)))
            {
                var comp = (IEqualityComparer<T>) comps[typeof(T)];
                return comp.Equals(item, other);
            }
            else
            {
                throw new ArgumentException($"{typeof(T)} had no corresponding equality checker");
            }
        }
    }
}
