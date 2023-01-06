using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Selector
{
    public static class ListenMatcher
    {
        // public static bool MatchTime(IListen nativeScrobble, LastTrack serviceScrobble) 
        //     => serviceScrobble.TimePlayed.Equals(nativeScrobble);

        public static bool MatchTime(IListen nativeScrobble, IListen serviceScrobble)
            => serviceScrobble.Timestamp.Equals(nativeScrobble.Timestamp);

        public static (IEnumerable<IListen>, IEnumerable<IListen>) IdentifyDiffs(IEnumerable<IListen> existing, IEnumerable<IListen> toApply, bool matchContents = true)
        {
            existing = existing.OrderBy(s => s.Timestamp);
            toApply = toApply.OrderBy(s => s.Timestamp);
            var toApplyIter = toApply.GetEnumerator();

            var toAdd = new List<IListen>();
            var toRemove = new List<IListen>();

            var toApplyOverrun = false;

            if (toApplyIter.MoveNext())
            {
                if (existing.Any())
                {
                    foreach (var currentExisting in existing)
                    {
                        if (!toApplyOverrun)
                        {
                            while (toApplyIter.Current.Timestamp < currentExisting.Timestamp)
                            {
                                toAdd.Add(toApplyIter.Current);

                                toApplyIter.MoveNext();
                            }

                            if (MatchTime(currentExisting, toApplyIter.Current))
                            {
                                if (matchContents)
                                {
                                    MatchData(currentExisting, toApplyIter.Current);
                                }

                                toApplyOverrun = !toApplyIter.MoveNext();
                            }
                            else
                            {
                                toRemove.Add(currentExisting);
                            }
                        }
                        else
                        {
                            toRemove.Add(currentExisting);
                        }
                    }

                    if (toApplyIter.Current is not null && !toApplyOverrun)
                    {
                        toAdd.Add(toApplyIter.Current);

                        while (toApplyIter.MoveNext())
                        {
                            toAdd.Add(toApplyIter.Current);
                        }
                    }
                }
                else
                {
                    toAdd.AddRange(toApply);
                }
            }

            return (toAdd, toRemove);
        }

        public static void MatchData(IListen currentExisting, IListen toApply)
        {
            if (!currentExisting.TrackName.Equals(toApply.TrackName, StringComparison.InvariantCultureIgnoreCase))
            {
                currentExisting.TrackName = toApply.TrackName;
            }

            if (!currentExisting.AlbumName.Equals(toApply.AlbumName, StringComparison.InvariantCultureIgnoreCase))
            {
                currentExisting.AlbumName = toApply.AlbumName;
            }

            if (!currentExisting.ArtistName.Equals(toApply.ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                currentExisting.ArtistName = toApply.ArtistName;
            }
        }

        public static (IEnumerable<IListen>, IEnumerable<IListen>) IdentifyDiffsContains(IEnumerable<IListen> existing, IEnumerable<IListen> toApply)
        {
            var toAdd = toApply.Where(s => !existing.Contains(s, new ListenComp()));
            var toRemove = existing.Where(s => !toApply.Contains(s, new ListenComp()));

            return (toAdd, toRemove);
        }

        public class ListenComp : IEqualityComparer<IListen>
        {
            public bool Equals(IListen x, IListen y) => x.Timestamp == y.Timestamp;

            public int GetHashCode([DisallowNull] IListen obj) => obj.Timestamp.GetHashCode();
        }
    }
}
