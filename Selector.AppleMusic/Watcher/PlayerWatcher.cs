using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.AppleMusic.Exceptions;
using Selector.AppleMusic.Model;

namespace Selector.AppleMusic.Watcher;

public class AppleMusicPlayerWatcher : BaseWatcher, IAppleMusicPlayerWatcher
{
    new protected readonly ILogger<AppleMusicPlayerWatcher> Logger;
    private readonly AppleMusicApi _appleMusicApi;

    public event EventHandler<AppleListeningChangeEventArgs> NetworkPoll;
    public event EventHandler<AppleListeningChangeEventArgs> ItemChange;
    public event EventHandler<AppleListeningChangeEventArgs> AlbumChange;
    public event EventHandler<AppleListeningChangeEventArgs> ArtistChange;

    public AppleMusicCurrentlyPlayingContext Live { get; protected set; }
    protected AppleMusicCurrentlyPlayingContext Previous { get; set; }
    public AppleTimeline Past { get; set; } = new();

    public AppleMusicPlayerWatcher(AppleMusicApi appleMusicClient,
        ILogger<AppleMusicPlayerWatcher> logger = null,
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
            Logger.LogTrace("Making Apple Music call");
            var polledCurrent = await _appleMusicApi.GetRecentlyPlayedTracks();

            // using var polledLogScope = Logger.BeginScope(new Dictionary<string, object>() { { "context", polledCurrent?.DisplayString() } });

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
                    Logger.LogDebug("Track changed: {prevTrack} -> {currentTrack}", first.Track, second.Track);
                    OnItemChange(AppleListeningChangeEventArgs.From(first, second, Past, id: Id));
                }
            }
        }
        catch (RateLimitException e)
        {
            Logger.LogDebug("Rate Limit exception: [{message}]", e.Message);
            // throw e;
        }
        catch (ForbiddenException e)
        {
            Logger.LogDebug("Forbidden exception: [{message}]", e.Message);
            throw;
        }
        catch (ServiceException e)
        {
            Logger.LogDebug("Apple Music internal error: [{message}]", e.Message);
            // throw e;
        }
        catch (UnauthorisedException e)
        {
            Logger.LogDebug("Unauthorised exception: [{message}]", e.Message);
            // throw e;
        }
    }

    private void SetLive(RecentlyPlayedTracksResponse recentlyPlayedTracks)
    {
        var lastTrack = recentlyPlayedTracks.Data?.FirstOrDefault();

        if (Live != null && Live.Track != null && Live.Track.Id == lastTrack?.Id)
        {
            Live = new()
            {
                Track = Live.Track,
                FirstSeen = Live.FirstSeen,
            };
        }
        else
        {
            Live = new()
            {
                Track = recentlyPlayedTracks.Data?.FirstOrDefault(),
                FirstSeen = DateTime.UtcNow,
            };
        }
    }

    public override Task Reset()
    {
        Previous = null;
        Live = null;
        Past = new();

        return Task.CompletedTask;
    }

    protected AppleListeningChangeEventArgs GetEvent() =>
        AppleListeningChangeEventArgs.From(Previous, Live, Past, id: Id);

    #region Event Firers

    protected virtual void OnNetworkPoll(AppleListeningChangeEventArgs args)
    {
        NetworkPoll?.Invoke(this, args);
    }

    protected virtual void OnItemChange(AppleListeningChangeEventArgs args)
    {
        ItemChange?.Invoke(this, args);
    }

    protected virtual void OnAlbumChange(AppleListeningChangeEventArgs args)
    {
        AlbumChange?.Invoke(this, args);
    }

    protected virtual void OnArtistChange(AppleListeningChangeEventArgs args)
    {
        ArtistChange?.Invoke(this, args);
    }

    #endregion
}