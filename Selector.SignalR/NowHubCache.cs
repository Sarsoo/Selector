using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.SignalR;

public class NowHubCache
{
    private readonly NowHubClient _connection;
    private readonly ILogger<NowHubCache> _logger;

    public TrackAudioFeatures? LastFeature { get; private set; }
    public List<Card> LastCards { get; private set; } = new();
    private readonly object _updateLock = new();

    private readonly object _bindingLock = new();
    private bool _isBound = false;

    public PlayCount? LastPlayCount { get; private set; }
    public SpotifyCurrentlyPlayingDTO? LastPlayingSpotify { get; private set; }
    public AppleCurrentlyPlayingDTO? LastPlayingApple { get; private set; }

    public event EventHandler? NewAudioFeature;
    public event EventHandler? NewCard;
    public event EventHandler? NewPlayCount;
    public event EventHandler? NewNowPlayingSpotify;
    public event EventHandler? NewNowPlayingApple;

    public NowHubCache(NowHubClient connection, ILogger<NowHubCache> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void BindClient()
    {
        lock (_bindingLock)
        {
            if (!_isBound)
            {
                _connection.OnNewAudioFeature(af =>
                {
                    lock (_updateLock)
                    {
                        _logger.LogInformation("New audio features received: {0}", af);
                        LastFeature = af;
                        NewAudioFeature?.Invoke(this, EventArgs.Empty);
                    }
                });

                _connection.OnNewCard(c =>
                {
                    lock (_updateLock)
                    {
                        _logger.LogInformation("New card received: {0}", c);
                        LastCards.Add(c);
                        NewCard?.Invoke(this, EventArgs.Empty);
                    }
                });

                _connection.OnNewPlayCount(pc =>
                {
                    lock (_updateLock)
                    {
                        _logger.LogInformation("New play count received: {0}", pc);
                        LastPlayCount = pc;
                        NewPlayCount?.Invoke(this, EventArgs.Empty);
                    }
                });

                _connection.OnNewPlayingSpotify(async np =>
                {
                    try
                    {
                        lock (_updateLock)
                        {
                            _logger.LogInformation("New Spotify now playing recieved: {0}", np);
                            LastPlayingSpotify = np;
                            LastCards.Clear();
                            NewNowPlayingSpotify?.Invoke(this, EventArgs.Empty);
                        }

                        if (LastPlayingSpotify?.Track is not null)
                        {
                            // if (!string.IsNullOrWhiteSpace(LastPlayingSpotify.Track.Id))
                            // {
                            //     await _connection.SendAudioFeatures(LastPlayingSpotify.Track.Id);
                            // }

                            await _connection.SendPlayCount(
                                LastPlayingSpotify.Track.Name,
                                LastPlayingSpotify.Track.Artists.FirstOrDefault()?.Name,
                                LastPlayingSpotify.Track.Album?.Name,
                                LastPlayingSpotify.Track.Album?.Artists.FirstOrDefault()?.Name
                            );

                            await _connection.SendFacts(
                                LastPlayingSpotify.Track.Name,
                                LastPlayingSpotify.Track.Artists.FirstOrDefault()?.Name,
                                LastPlayingSpotify.Track.Album?.Name,
                                LastPlayingSpotify.Track.Album?.Artists.FirstOrDefault()?.Name
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while handling new Spotify now playing");
                    }
                });

                _connection.OnNewPlayingApple(np =>
                {
                    try
                    {
                        lock (_updateLock)
                        {
                            _logger.LogInformation("New Apple now playing recieved: {0}", np);
                            LastPlayingApple = np;
                            LastCards.Clear();
                            NewNowPlayingApple?.Invoke(this, EventArgs.Empty);
                        }

                        if (LastPlayingApple?.Track is not null)
                        {
                            // if (!string.IsNullOrWhiteSpace(LastPlayingApple.Track.Id))
                            // {
                            //     await _connection.SendAudioFeatures(LastPlayingApple.Track.Id);
                            // }

                            // await _connection.SendPlayCount(
                            //     LastPlayingSpotify.Track.Name,
                            //     LastPlayingSpotify.Track.Artists.FirstOrDefault()?.Name,
                            //     LastPlayingSpotify.Track.Album?.Name,
                            //     LastPlayingSpotify.Track.Album?.Artists.FirstOrDefault()?.Name
                            // );
                            //
                            // await _connection.SendFacts(
                            //     LastPlayingSpotify.Track.Name,
                            //     LastPlayingSpotify.Track.Artists.FirstOrDefault()?.Name,
                            //     LastPlayingSpotify.Track.Album?.Name,
                            //     LastPlayingSpotify.Track.Album?.Artists.FirstOrDefault()?.Name
                            // );
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while handling new Apple now playing");
                    }

                    return Task.CompletedTask;
                });

                _isBound = true;
            }
        }
    }
}