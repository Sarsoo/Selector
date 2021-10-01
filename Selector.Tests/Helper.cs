using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpotifyAPI.Web;

namespace Selector.Tests
{
    static class Helper
    {
        public static FullTrack FullTrack(string name, string album = "album name", List<string> artists = null)
        {
            if (artists is null) artists = new List<string>() {"artist"};
            
            return new FullTrack()
            {
                Name = name,
                Uri = name,
                Album = SimpleAlbum(album, artists),
                Artists = artists.Select(a => SimpleArtist(a)).ToList()
            };
        }

        public static FullTrack FullTrack(string name, string album, string artist)
        {
            return FullTrack(name, album, new List<string>() { artist });
        }

        public static FullEpisode FullEpisode(string name, string show = null, string publisher = null)
        {
            return new FullEpisode()
            {
                Name = name,
                Uri = name,
                Show = SimpleShow(show ?? name, publisher: publisher)
            };
        }

        public static SimpleAlbum SimpleAlbum(string name, List<string> artists = null)
        {
            if (artists is null) artists = new List<string>() {"artist"};
            return new SimpleAlbum()
            {
                Name = name,
                Uri = name,
                Artists = artists.Select(a => SimpleArtist(a)).ToList()
            };
        }

        public static SimpleAlbum SimpleAlbum(string name, string artist)
        {
            return SimpleAlbum(name, new List<string>() { artist });
        }

        public static SimpleArtist SimpleArtist(string name)
        {
            return new SimpleArtist()
            {
                Name = name,
                Uri = name
            };
        }

        public static SimpleShow SimpleShow(string name, string publisher = null)
        {
            return new SimpleShow()
            {
                Name = name,
                Publisher = publisher ?? name,
                Uri = name
            };
        }

        public static CurrentlyPlaying CurrentlyPlaying(FullTrack track, bool isPlaying = true, string context = null)
        {
            return new CurrentlyPlaying()
            {
                Context = Context(context ?? track.Uri),
                IsPlaying = isPlaying,
                Item = track
            };
        }

        public static CurrentlyPlayingContext CurrentPlayback(FullTrack track, Device device = null, bool isPlaying = true, string context = "context")
        {
            return new CurrentlyPlayingContext()
            {
                Context = Context(context),
                Device = device ?? Device("device"),
                IsPlaying = isPlaying,
                Item = track
            };
        }

        public static CurrentlyPlaying CurrentlyPlaying(FullEpisode episode, bool isPlaying = true, string context = null)
        {
            return new CurrentlyPlaying()
            {
                Context = Context(context ?? episode.Uri),
                IsPlaying = isPlaying,
                Item = episode
            };
        }

        public static CurrentlyPlayingContext CurrentPlayback(FullEpisode episode, Device device = null, bool isPlaying = true, string context = null)
        {
            return new CurrentlyPlayingContext()
            {
                Context = Context(context ?? episode.Uri),
                Device = device ?? Device("device"),
                IsPlaying = isPlaying,
                Item = episode
            };
        }

        public static Context Context(string uri)
        {
            return new Context()
            {
                Uri = uri
            };
        }

        public static Device Device(string name, string id = null, int volume = 50)
        {
            return new Device()
            {
                Name = name,
                Id = id ?? name,
                Type = "computer",
                VolumePercent = volume
            };
        }
    }
}
