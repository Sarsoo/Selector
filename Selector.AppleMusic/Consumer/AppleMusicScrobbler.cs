using System.Diagnostics;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic.Consumer;

public class AppleMusicScrobbler :
    BaseSequentialPlayerConsumer<IAppleMusicPlayerWatcher, AppleListeningChangeEventArgs>, IApplePlayerConsumer
{
    public CancellationToken CancelToken { get; set; }
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly IScrobbler _scrobbler;
    private readonly LastfmClient _lastClient;

    public AppleMusicScrobbler(IAppleMusicPlayerWatcher? watcher,
        LastfmClient LastClient,
        ILogger<AppleMusicScrobbler> logger,
        CancellationToken token = default) : base(watcher, logger)
    {
        CancelToken = token;

        _lastClient = LastClient;
        _scrobbler = _lastClient.Scrobbler;
    }

    public async Task Auth(string lastfmUsername,
        string lastfmPassword)
    {
        using var span = Trace.Tracer.StartActivity();
        span?.AddBaggage(TraceConst.LastFmUsername, lastfmUsername);

        var response = await _lastClient.Auth.GetSessionTokenAsync(lastfmUsername, lastfmPassword);

        if (response.Success)
        {
            span?.SetStatus(ActivityStatusCode.Ok);
            Logger.LogInformation("[{username}] Successfully authenticated to Last.fm for Apple Music scrobbling",
                lastfmUsername);
        }
        else
        {
            span?.SetStatus(ActivityStatusCode.Error, "Failed to authenticate to Last.fm for Apple Music scrobbling");
            Logger.LogError("[{username}] Failed to authenticate to Last.fm for Apple Music scrobbling ({})",
                lastfmUsername, response.Status);
        }
    }

    private async Task SendCached()
    {
        using var span = Trace.Tracer.StartActivity();
        Logger.LogInformation("Sending any cached Apple Music scrobbles");
        var response = await _scrobbler.SendCachedScrobblesAsync();
        if (response.Success)
        {
            Logger.LogInformation("Sent [{}] cached Apple Music scrobbles", response.AcceptedCount);
        }
        else
        {
            Logger.LogError(response.Exception, "Failed to send cached Apple Music scrobbles");
        }
    }

    protected override async Task ProcessEvent(AppleListeningChangeEventArgs e)
    {
        using var span = Trace.Tracer.StartActivity();
        await _lock.WaitAsync(CancelToken);

        try
        {
            await SendCached();

            var lastScrobbled = e.Timeline.LastOrDefault(t => !t.Item.ToScrobble);
            var hourAgo = DateTimeOffset.UtcNow - TimeSpan.FromHours(1);
            var unScrobbled = e.Timeline
                .Where(i =>
                    i.Item.ToScrobble
                    && i.Item.FirstSeen < hourAgo)
                .ToList();

            var lastThreeHours = DateTimeOffset.UtcNow - TimeSpan.FromHours(4);
            var alreadyProcessed = e.Timeline
                .Where(x =>
                    !x.Item.ToScrobble
                    && x.Item.FirstSeen > lastThreeHours
                    && x.Item.FirstSeen < hourAgo)
                .Select(x => x.Item.Track.Id)
                .ToArray();
            var toScrobble = new List<TimelineItem<AppleMusicCurrentlyPlayingContext>>();
            foreach (var u in unScrobbled)
            {
                if (!alreadyProcessed.Contains(u.Item.Track.Id))
                {
                    toScrobble.Add(u);
                }
                else
                {
                    u.Item.ScrobbleIgnored = true;
                    Logger.LogInformation("Ignored [{}], been scrobbled in the last three hours", u.Item.Track);
                }
            }

            if (!toScrobble.Any()) return;

            var scrobbleTimes = new List<DateTimeOffset>();
            if (lastScrobbled != null)
            {
                var times = (toScrobble.Last().Time - lastScrobbled.Time).Seconds;
                var intervals = times / (toScrobble.Count + 1);

                foreach (var interval in Enumerable.Range(0, toScrobble.Count))
                {
                    scrobbleTimes.Add(toScrobble.Last().Time - TimeSpan.FromSeconds(interval * int.Max(31, intervals)));
                }
            }
            else
            {
                foreach (var interval in Enumerable.Range(0, toScrobble.Count))
                {
                    scrobbleTimes.Add(toScrobble.Last().Time - TimeSpan.FromSeconds(interval * 31));
                }
            }

            scrobbleTimes.Reverse();

            Logger.LogInformation("Sending scrobbles for [{}]",
                string.Join(", ", toScrobble.Select(x => x.Item.Track)));

            var scrobbleResponse = await _scrobbler.ScrobbleAsync(
                toScrobble.Zip(scrobbleTimes)
                    .Select((s) =>
                        new Scrobble(
                            s.First.Item.Track.Attributes.ArtistName,
                            s.First.Item.Track.Attributes.AlbumName,
                            s.First.Item.Track.Attributes.Name,
                            s.Second)));

            foreach (var track in toScrobble)
            {
                track.Item.Scrobbled = true;
            }

            if (scrobbleResponse.Success)
            {
                Logger.LogInformation("Sent [{}] Apple Music scrobbles", scrobbleResponse.AcceptedCount);
            }
            else
            {
                Logger.LogError(scrobbleResponse.Exception, "Failed to send Apple Music scrobbles, ignored [{}]",
                    string.Join(", ", scrobbleResponse.Ignored));
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}