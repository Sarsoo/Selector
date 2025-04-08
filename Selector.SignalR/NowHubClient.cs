using Microsoft.AspNetCore.SignalR.Client;
using Selector.AppleMusic;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.SignalR;

public class NowHubClient : BaseSignalRClient, INowPlayingHub, IDisposable
{
    private readonly List<IDisposable> _newPlayingSpotifyCallbacks = new();
    private readonly List<IDisposable> _newPlayingAppleCallbacks = new();
    private readonly List<IDisposable> _newAudioFeatureCallbacks = new();
    private readonly List<IDisposable> _newPlayCountCallbacks = new();
    private readonly List<IDisposable> _newCardCallbacks = new();
    private bool _disposedValue;

    public NowHubClient(string? token = null) : base("nowhub", token)
    {
    }

    public void OnNewPlayingSpotify(Action<SpotifyCurrentlyPlayingDTO> action)
    {
        _newPlayingSpotifyCallbacks.Add(hubConnection.On(nameof(OnNewPlayingSpotify), action));
    }

    public void OnNewPlayingApple(Action<AppleCurrentlyPlayingDTO> action)
    {
        _newPlayingAppleCallbacks.Add(hubConnection.On(nameof(OnNewPlayingApple), action));
    }

    public void OnNewAudioFeature(Action<TrackAudioFeatures> action)
    {
        _newAudioFeatureCallbacks.Add(hubConnection.On(nameof(OnNewAudioFeature), action));
    }

    public void OnNewPlayCount(Action<PlayCount> action)
    {
        _newPlayCountCallbacks.Add(hubConnection.On(nameof(OnNewPlayCount), action));
    }

    public void OnNewCard(Action<Card> action)
    {
        _newCardCallbacks.Add(hubConnection.On(nameof(OnNewCard), action));
    }

    public void OnNewPlayingSpotify(Func<SpotifyCurrentlyPlayingDTO, Task> action)
    {
        _newPlayingSpotifyCallbacks.Add(hubConnection.On(nameof(OnNewPlayingSpotify), action));
    }

    public void OnNewPlayingApple(Func<AppleCurrentlyPlayingDTO, Task> action)
    {
        _newPlayingAppleCallbacks.Add(hubConnection.On(nameof(OnNewPlayingApple), action));
    }

    public void OnNewAudioFeature(Func<TrackAudioFeatures, Task> action)
    {
        _newAudioFeatureCallbacks.Add(hubConnection.On(nameof(OnNewAudioFeature), action));
    }

    public void OnNewPlayCount(Func<PlayCount, Task> action)
    {
        _newPlayCountCallbacks.Add(hubConnection.On(nameof(OnNewPlayCount), action));
    }

    public void OnNewCard(Func<Card, Task> action)
    {
        _newCardCallbacks.Add(hubConnection.On(nameof(OnNewCard), action));
    }

    public Task OnConnected()
    {
        return hubConnection.InvokeAsync(nameof(OnConnected));
    }

    public Task PlayDensityFacts(string track, string artist, string album, string albumArtist)
    {
        return hubConnection.InvokeAsync(nameof(PlayDensityFacts), track, artist, album, albumArtist);
    }

    [Obsolete]
    public Task SendAudioFeatures(string trackId)
    {
        return hubConnection.InvokeAsync(nameof(SendAudioFeatures), trackId);
    }

    public Task SendFacts(string track, string artist, string album, string albumArtist)
    {
        return hubConnection.InvokeAsync(nameof(SendFacts), track, artist, album, albumArtist);
    }

    public Task SendNewPlaying()
    {
        return hubConnection.InvokeAsync(nameof(SendNewPlaying));
    }

    public Task SendPlayCount(string track, string artist, string album, string albumArtist)
    {
        return hubConnection.InvokeAsync(nameof(SendPlayCount), track, artist, album, albumArtist);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var callback in _newPlayingSpotifyCallbacks
                             .Concat(_newAudioFeatureCallbacks)
                             .Concat(_newPlayCountCallbacks)
                             .Concat(_newCardCallbacks))
                {
                    callback.Dispose();
                }

                base.DisposeAsync();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}