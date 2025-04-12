using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.AppleMusic.Exceptions;
using Selector.AppleMusic.Model;

namespace Selector.AppleMusic.Watcher;

public class AppleMusicPlayerWatcher : BaseWatcher, IAppleMusicPlayerWatcher
{
    private new readonly ILogger<AppleMusicPlayerWatcher> Logger;
    private readonly AppleMusicApi _appleMusicApi;

    public event EventHandler<AppleListeningChangeEventArgs>? NetworkPoll;
    public event EventHandler<AppleListeningChangeEventArgs>? ItemChange;
    public event EventHandler<AppleListeningChangeEventArgs>? AlbumChange;
    public event EventHandler<AppleListeningChangeEventArgs>? ArtistChange;

    public AppleMusicCurrentlyPlayingContext? Live { get; private set; }
    private AppleMusicCurrentlyPlayingContext? Previous { get; set; }
    public AppleTimeline Past { get; private set; } = new();

    public AppleMusicPlayerWatcher(AppleMusicApi appleMusicClient,
        ILogger<AppleMusicPlayerWatcher>? logger = null,
        int pollPeriod = 3000
    ) : base(logger)
    {
        _appleMusicApi = appleMusicClient;
        Logger = logger ?? NullLogger<AppleMusicPlayerWatcher>.Instance;
        PollPeriod = pollPeriod;
    }

    public override async Task WatchOne(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            using var polledLogScope = Logger.BeginScope(new Dictionary<string, object>() { { "user_id", Id } });

            Logger.LogTrace("Making Apple Music call");
            var polledCurrent = await _appleMusicApi.GetRecentlyPlayedTracks();

            if (polledCurrent is null)
            {
                Logger.LogInformation("Null response when calling Apple Music API");
                return;
            }

            if (polledCurrent.Data is null)
            {
                Logger.LogInformation("Null track list when calling Apple Music API");
                return;
            }

            Logger.LogTrace("Received Apple Music call");

            var currentPrevious = Previous;
            var reversedItems = polledCurrent.Data.ToList();
            reversedItems.Reverse();
            var addedItems = Past.Add(reversedItems);

            // swap new item into live and bump existing down to previous
            Previous = Live;
            SetLive(polledCurrent);

            OnNetworkPoll(GetEvent());

            if (currentPrevious != null && addedItems.Any())
            {
                addedItems.Insert(0, currentPrevious);
                foreach (var (first, second) in addedItems.Zip(addedItems.Skip(1)))
                {
                    Logger.LogInformation("Track changed: {prevTrack} -> {currentTrack}", first.Track, second.Track);
                    OnItemChange(AppleListeningChangeEventArgs.From(first, second, Past, id: Id));
                }
            }
        }
        catch (RateLimitException e)
        {
            Logger.LogError(e, "Rate Limit exception");
            // throw;
        }
        catch (ForbiddenException e)
        {
            Logger.LogError(e, "Forbidden exception");
            // throw;
        }
        catch (ServiceException)
        {
            Logger.LogInformation("Apple Music internal error");
            // throw;
        }
        catch (UnauthorisedException e)
        {
            Logger.LogError(e, "Unauthorised exception");
            // throw;
        }
        catch (AppleMusicException e)
        {
            Logger.LogInformation("Apple Music exception ({})", e.StatusCode);
            // throw;
        }
    }

    private void SetLive(RecentlyPlayedTracksResponse recentlyPlayedTracks)
    {
        var lastTrack = recentlyPlayedTracks.Data?.FirstOrDefault();

        if (Live is { Track: not null } && Live.Track.Id == lastTrack?.Id)
        {
            Live = new()
            {
                Track = Live.Track,
                FirstSeen = Live.FirstSeen,
                Scrobbled = Live.Scrobbled,
                ScrobbleIgnored = Live.ScrobbleIgnored,
            };
        }
        else
        {
            if (recentlyPlayedTracks.Data is not null && recentlyPlayedTracks.Data.Any())
            {
                Live = new()
                {
                    Track = recentlyPlayedTracks.Data.First(),
                    FirstSeen = DateTime.UtcNow,
                    Scrobbled = false,
                    ScrobbleIgnored = false,
                };
            }
        }
    }

    public override Task Reset()
    {
        Previous = null;
        Live = null;
        Past = new();

        return Task.CompletedTask;
    }

    private AppleListeningChangeEventArgs GetEvent() =>
        AppleListeningChangeEventArgs.From(Previous, Live, Past, id: Id);

    #region Event Firers

    private void OnNetworkPoll(AppleListeningChangeEventArgs args)
    {
        NetworkPoll?.Invoke(this, args);
    }

    private void OnItemChange(AppleListeningChangeEventArgs args)
    {
        ItemChange?.Invoke(this, args);
    }

    protected void OnAlbumChange(AppleListeningChangeEventArgs args)
    {
        AlbumChange?.Invoke(this, args);
    }

    protected void OnArtistChange(AppleListeningChangeEventArgs args)
    {
        ArtistChange?.Invoke(this, args);
    }

    #endregion
}