using Selector.AppleMusic.Model;
using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic;

public class AppleTimeline : Timeline<AppleMusicCurrentlyPlayingContext>
{
    public List<AppleMusicCurrentlyPlayingContext> Add(IEnumerable<Track> tracks)
        => Add(tracks
            .Select(x => new AppleMusicCurrentlyPlayingContext()
            {
                Track = x,
                FirstSeen = DateTime.UtcNow,
            }).ToList());

    public List<AppleMusicCurrentlyPlayingContext> Add(List<AppleMusicCurrentlyPlayingContext> items)
    {
        var newItems = new List<AppleMusicCurrentlyPlayingContext>();

        if (items == null || !items.Any())
        {
            return newItems;
        }

        if (!Recent.Any())
        {
            Recent.AddRange(items.Select(x =>
                TimelineItem<AppleMusicCurrentlyPlayingContext>.From(x, DateTime.UtcNow)));
            return newItems;
        }

        if (Recent
            .TakeLast(items.Count)
            .Select(x => x.Item)
            .SequenceEqual(items, new AppleMusicCurrentlyPlayingContextComparer()))
        {
            return newItems;
        }

        var (found, startIdx) = Loop(items, 0);

        TimelineItem<AppleMusicCurrentlyPlayingContext>? popped = null;
        if (found == 0)
        {
            var (foundOffseted, startIdxOffseted) = Loop(items, 1);

            if (foundOffseted > found)
            {
                popped = Recent[^1];
                Recent.RemoveAt(Recent.Count - 1);

                startIdx = startIdxOffseted;
            }
        }

        foreach (var item in items.TakeLast(startIdx))
        {
            newItems.Add(item);
            Recent.Add(TimelineItem<AppleMusicCurrentlyPlayingContext>.From(item, DateTime.UtcNow));
        }

        if (popped is not null)
        {
            var idx = newItems.FindIndex(x => x.Track.Id == popped.Item.Track.Id);
            if (idx >= 0)
            {
                newItems.RemoveAt(idx);
            }
        }

        CheckSize();

        return newItems;
    }

    private (int, int) Loop(List<AppleMusicCurrentlyPlayingContext> items, int storedOffset)
    {
        var stop = false;
        var found = 0;
        var startIdx = 0;
        while (!stop)
        {
            found = Loop(items, storedOffset, ref startIdx, ref stop);

            if (!stop) startIdx += 1;
        }

        return (found, startIdx);
    }

    private int Loop(List<AppleMusicCurrentlyPlayingContext> items, int storedOffset, ref int startIdx, ref bool stop)
    {
        var found = 0;

        for (var i = 0; i < items.Count; i++)
        {
            var storedIdx = (Recent.Count - 1) - i - storedOffset;
            // start from the end, minus this loops index, minus the offset
            var pulledIdx = (items.Count - 1) - i - startIdx;

            if (pulledIdx < 0)
            {
                // ran to the end of new items and none matched the end, add all the new ones
                stop = true;
                break;
            }

            if (storedIdx < 0)
            {
                // all the new stuff matches, we're done and there's nothing new to add
                stop = true;
                break;
            }

            if (Recent[storedIdx].Item.Track.Id == items[pulledIdx].Track.Id)
            {
                // good, keep going
                found++;
                if (found >= 3)
                {
                    stop = true;
                    break;
                }
            }
            else
            {
                // bad, doesn't match, break and bump stored
                found = 0;
                break;
            }
        }

        return found;
    }
}