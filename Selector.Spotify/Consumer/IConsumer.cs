namespace Selector.Spotify.Consumer;

public interface ISpotifyPlayerConsumer : IConsumer<SpotifyListeningChangeEventArgs>
{
}

public interface IPlaylistConsumer : IConsumer<PlaylistChangeEventArgs>
{
}