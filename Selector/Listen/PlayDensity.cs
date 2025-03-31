using System;
using System.Collections.Generic;
using System.Linq;

namespace Selector
{
    public static class PlayDensity
    {
        public static decimal Density(this IEnumerable<IListen> scrobbles, TimeSpan window) =>
            scrobbles.Density(DateTime.UtcNow - window, DateTime.UtcNow);

        public static decimal Density(this IEnumerable<IListen> scrobbles, DateTime from, DateTime to)
        {
            var filteredScrobbles = scrobbles.Where(s => s.Timestamp > from && s.Timestamp < to);

            var dayDelta = (decimal)(to - from).Days;

            return filteredScrobbles.Count() / dayDelta;
        }

        public static decimal Density(this IEnumerable<IListen> scrobbles)
        {
            var minDate = scrobbles.Select(s => s.Timestamp).Min();
            var maxDate = scrobbles.Select(s => s.Timestamp).Max();

            var dayDelta = (decimal)(maxDate - minDate).Days;

            return scrobbles.Count() / dayDelta;
        }
    }
}