using Microsoft.AspNetCore.SignalR.Client;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.SignalR;

public class NowHubClient : BaseSignalRClient, INowPlayingHub, IDisposable
{
    private List<IDisposable> NewPlayingCallbacks = new();
    private List<IDisposable> NewAudioFeatureCallbacks = new();
    private List<IDisposable> NewPlayCountCallbacks = new();
    private List<IDisposable> NewCardCallbacks = new();
    private bool disposedValue;

    public NowHubClient(string token = null) : base("nowhub", token)
    {
    }

    public void OnNewPlaying(Action<SpotifyCurrentlyPlayingDTO> action)
    {
        NewPlayingCallbacks.Add(hubConnection.On(nameof(OnNewPlaying), action));
    }

    public void OnNewAudioFeature(Action<TrackAudioFeatures> action)
    {
        NewAudioFeatureCallbacks.Add(hubConnection.On(nameof(OnNewAudioFeature), action));
    }

    public void OnNewPlayCount(Action<PlayCount> action)
    {
        NewPlayCountCallbacks.Add(hubConnection.On(nameof(OnNewPlayCount), action));
    }

    public void OnNewCard(Action<Card> action)
    {
        NewCardCallbacks.Add(hubConnection.On(nameof(OnNewCard), action));
    }

    public void OnNewPlaying(Func<SpotifyCurrentlyPlayingDTO, Task> action)
    {
        NewPlayingCallbacks.Add(hubConnection.On(nameof(OnNewPlaying), action));
    }

    public void OnNewAudioFeature(Func<TrackAudioFeatures, Task> action)
    {
        NewAudioFeatureCallbacks.Add(hubConnection.On(nameof(OnNewAudioFeature), action));
    }

    public void OnNewPlayCount(Func<PlayCount, Task> action)
    {
        NewPlayCountCallbacks.Add(hubConnection.On(nameof(OnNewPlayCount), action));
    }

    public void OnNewCard(Func<Card, Task> action)
    {
        NewCardCallbacks.Add(hubConnection.On(nameof(OnNewCard), action));
    }

    public Task OnConnected()
    {
        return hubConnection.InvokeAsync(nameof(OnConnected));
    }

    public Task PlayDensityFacts(string track, string artist, string album, string albumArtist)
    {
        return hubConnection.InvokeAsync(nameof(PlayDensityFacts), track, artist, album, albumArtist);
    }

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
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var callback in NewPlayingCallbacks
                             .Concat(NewAudioFeatureCallbacks)
                             .Concat(NewPlayCountCallbacks)
                             .Concat(NewCardCallbacks))
                {
                    callback.Dispose();
                }

                base.DisposeAsync();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}