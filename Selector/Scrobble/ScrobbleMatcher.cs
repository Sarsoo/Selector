using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Selector
{
    public static class ScrobbleMatcher
    {
        public static bool MatchTime(Scrobble nativeScrobble, LastTrack serviceScrobble) 
            => serviceScrobble.TimePlayed.Equals(nativeScrobble);

        public static bool MatchTime(Scrobble nativeScrobble, Scrobble serviceScrobble)
            => serviceScrobble.Timestamp.Equals(nativeScrobble.Timestamp);

        public static (IEnumerable<Scrobble>, IEnumerable<Scrobble>) IdentifyDiffs(IEnumerable<Scrobble> existing, IEnumerable<Scrobble> toApply, bool matchContents = true)
        {
            existing = existing.OrderBy(s => s.Timestamp);
            toApply = toApply.OrderBy(s => s.Timestamp);
            var toApplyIter = toApply.GetEnumerator();

            var toAdd = new List<Scrobble>();
            var toRemove = new List<Scrobble>();

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

        public static void MatchData(Scrobble currentExisting, Scrobble toApply)
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

        public static (IEnumerable<Scrobble>, IEnumerable<Scrobble>) IdentifyDiffsContains(IEnumerable<Scrobble> existing, IEnumerable<Scrobble> toApply)
        {
            var toAdd = toApply.Where(s => !existing.Contains(s, new ScrobbleComp()));
            var toRemove = existing.Where(s => !toApply.Contains(s, new ScrobbleComp()));

            return (toAdd, toRemove);
        }

        public class ScrobbleComp : IEqualityComparer<Scrobble>
        {
            public bool Equals(Scrobble x, Scrobble y) => x.Timestamp == y.Timestamp;

            public int GetHashCode([DisallowNull] Scrobble obj) => obj.Timestamp.GetHashCode();
        }
    }
}
