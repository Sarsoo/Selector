using IF.Lastfm.Core.Objects;
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

        public static (IEnumerable<Scrobble>, IEnumerable<Scrobble>) IdentifyDiffs(IEnumerable<Scrobble> existing, IEnumerable<Scrobble> toApply)
        {
            existing = existing.OrderBy(s => s.Timestamp);
            toApply = toApply.OrderBy(s => s.Timestamp);
            var toApplyIter = toApply.GetEnumerator();

            var toAdd = new List<Scrobble>();
            var toRemove = new List<Scrobble>();

            if (toApplyIter.MoveNext())
            {
                if (existing.Any())
                {
                    foreach (var currentExisting in existing)
                    {
                        while (toApplyIter.Current.Timestamp < currentExisting.Timestamp)
                        {
                            toAdd.Add(toApplyIter.Current);

                            toApplyIter.MoveNext();
                        }

                        if (MatchTime(currentExisting, toApplyIter.Current))
                        {
                            toApplyIter.MoveNext();
                        }
                        else
                        {
                            toRemove.Add(currentExisting);
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
