using System.Collections.Generic;

namespace Selector.SignalR;

public interface IRankResult
{
    IEnumerable<IChartEntry> TrackEntries { get; set; }
    IEnumerable<IChartEntry> AlbumEntries { get; set; }
    IEnumerable<IChartEntry> ArtistEntries { get; set; }
    IEnumerable<CountSample> ResampledSeries { get; set; }
    int TotalCount { get; set; }
}