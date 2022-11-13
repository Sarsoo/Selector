using System;
using System.Collections.Generic;

namespace Selector.Web;

public class RankResult
{
    public IEnumerable<ChartEntry> TrackEntries { get; set; }
    public IEnumerable<ChartEntry> AlbumEntries { get; set; }
    public IEnumerable<ChartEntry> ArtistEntries { get; set; }

    public IEnumerable<CountSample> ResampledSeries { get; set; }

    public int TotalCount { get; set; }
}

