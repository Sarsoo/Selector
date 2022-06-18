using System;
using System.Collections.Generic;
using System.Linq;

namespace Selector
{
	public record struct CountSample {
		public DateTime TimeStamp { get; set; }
		public int Value { get; set; }
	}

	public static class Resampler
	{
		public static IEnumerable<CountSample> Resample(this IEnumerable<Scrobble> scrobbles, TimeSpan window)
		{
			var sortedScrobbles = scrobbles.OrderBy(s => s.Timestamp).ToList();

			if (!sortedScrobbles.Any())
			{
				yield break;
			}

			var sortedScrobblesIter = sortedScrobbles.GetEnumerator();
			sortedScrobblesIter.MoveNext();

			var earliest = sortedScrobbles.First().Timestamp;
			var latest = sortedScrobbles.Last().Timestamp;

			for (var counter = earliest; counter <= latest; counter += window)
			{
				var windowEnd = counter + window;

				var count = 0;

				if (sortedScrobblesIter.Current is not null)
                {
					count++;
				}

				while (sortedScrobblesIter.MoveNext() && counter <= sortedScrobblesIter.Current.Timestamp && sortedScrobblesIter.Current.Timestamp < windowEnd)
				{
					count++;
				}

				yield return new CountSample()
				{
					TimeStamp = counter + (window / 2),
					Value = count
				};
			}
		}

		public static IEnumerable<CountSample> ResampleByMonth(this IEnumerable<Scrobble> scrobbles)
		{
			var sortedScrobbles = scrobbles.OrderBy(s => s.Timestamp).ToList();

			if (!sortedScrobbles.Any())
			{
				yield break;
			}

			var sortedScrobblesIter = sortedScrobbles.GetEnumerator();
			sortedScrobblesIter.MoveNext();

			var earliest = sortedScrobbles.First().Timestamp;
			var latest = sortedScrobbles.Last().Timestamp;
			var latestPlusMonth = latest.AddMonths(1);

			var periodStart = new DateTime(earliest.Year, earliest.Month, 1);
			var periodEnd = new DateTime(latestPlusMonth.Year, latestPlusMonth.Month, 1);

			for (var counter = periodStart; counter <= periodEnd; counter = counter.AddMonths(1))
			{
				var count = 0;

				if (sortedScrobblesIter.Current is not null)
				{
					count++;
				}

				while (sortedScrobblesIter.MoveNext()
					&& sortedScrobblesIter.Current.Timestamp.Year == counter.Year
					&& sortedScrobblesIter.Current.Timestamp.Month == counter.Month)
				{
					count++;
				}

				yield return new CountSample()
				{
					TimeStamp = counter,
					Value = count
				};
			}
		}
	}
}

