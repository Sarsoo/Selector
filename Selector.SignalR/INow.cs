using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.SignalR;

public interface INowPlayingHubClient
{
    public Task OnNewPlaying(CurrentlyPlayingDTO context);
    public Task OnNewAudioFeature(TrackAudioFeatures features);
    public Task OnNewPlayCount(PlayCount playCount);
    public Task OnNewCard(Card card);
}

public interface INowPlayingHub
{
    Task OnConnected();
    Task PlayDensityFacts(string track, string artist, string album, string albumArtist);
    Task SendAudioFeatures(string trackId);
    Task SendFacts(string track, string artist, string album, string albumArtist);
    Task SendNewPlaying();
    Task SendPlayCount(string track, string artist, string album, string albumArtist);
}