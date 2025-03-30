using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BaseSpotifyWatcher(ILogger<BaseWatcher> logger = null) : BaseWatcher(logger)
{
    public string SpotifyUsername { get; set; }

    protected override Dictionary<string, object> LogScopeContext =>
        new[]
            {
                base.LogScopeContext,
                new Dictionary<string, object>() { { "spotify_username", SpotifyUsername } }
            }
            .SelectMany(x => x)
            .ToDictionary();
}