using System;
using System.Collections.Generic;
using Selector.SignalR;

namespace Selector.Web;

public class RankResult : IRankResult
{
    public IEnumerable<IChartEntry> TrackEntries { get; set; }
    public IEnumerable<IChartEntry> AlbumEntries { get; set; }
    public IEnumerable<IChartEntry> ArtistEntries { get; set; }

    public IEnumerable<CountSample> ResampledSeries { get; set; }

    public int TotalCount { get; set; }
}

