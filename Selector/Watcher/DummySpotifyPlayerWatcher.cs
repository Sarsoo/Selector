using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector
{
    public class DummySpotifyPlayerWatcher : SpotifyPlayerWatcher
    {
        private CurrentlyPlayingContext[] _contexts = new[]
        {
            new CurrentlyPlayingContext
            {
                Device = new Device
                {
                    Id = "Dev 1",
                    IsActive = true,
                    IsPrivateSession = false,
                    IsRestricted = false,
                    Name = "Dev 1",
                    Type = "Phone",
                    VolumePercent = 50
                },
                ShuffleState = false,
                Context = new Context
                {
                },
                IsPlaying = true,
                Item = new FullTrack
                {
                    Name = "Track 1",
                    Id = "track-1",
                    Href = "https://sarsoo.xyz",
                    Popularity = 50,
                    Uri = "spotify:track:1",
                    Album = new SimpleAlbum
                    {
                        Name = "Album 1",
                        Artists = new List<SimpleArtist>
                        {
                            new SimpleArtist
                            {
                                Name = "Artist 1"
                            }
                        }
                    },
                    Artists = new List<SimpleArtist>
                    {
                        new SimpleArtist
                        {
                            Name = "Artist 1"
                        }
                    }
                }
            },
            new CurrentlyPlayingContext
            {
                Device = new Device
                {
                    Id = "Dev 1",
                    IsActive = true,
                    IsPrivateSession = false,
                    IsRestricted = false,
                    Name = "Dev 1",
                    Type = "Phone",
                    VolumePercent = 50
                },
                ShuffleState = false,
                Context = new Context
                {
                },
                IsPlaying = true,
                Item = new FullTrack
                {
                    Name = "Track 2",
                    Id = "track-2",
                    Href = "https://sarsoo.xyz",
                    Popularity = 50,
                    Uri = "spotify:track:2",
                    Album = new SimpleAlbum
                    {
                        Name = "Album 1",
                        Artists = new List<SimpleArtist>
                        {
                            new SimpleArtist
                            {
                                Name = "Artist 2"
                            }
                        }
                    },
                    Artists = new List<SimpleArtist>
                    {
                        new SimpleArtist
                        {
                            Name = "Artist 2"
                        }
                    }
                }
            }
        };

        private int _contextIdx = 0;

        private DateTime _lastNext = DateTime.UtcNow;
        private TimeSpan _contextLifespan = TimeSpan.FromSeconds(30);

        public DummySpotifyPlayerWatcher(IEqual equalityChecker,
            ILogger<DummySpotifyPlayerWatcher> logger = null,
            int pollPeriod = 3000) : base(null, equalityChecker, logger, pollPeriod)
        {
        }

        private bool ShouldCycle() => DateTime.UtcNow - _lastNext > _contextLifespan;

        private void BumpContextIndex()
        {
            Logger.LogDebug("Next song");

            _contextIdx++;

            if (_contextIdx >= _contexts.Length)
            {
                _contextIdx = 0;
            }
        }

        private CurrentlyPlayingContext GetContext()
        {
            if (ShouldCycle()) BumpContextIndex();

            return _contexts[_contextIdx];
        }

        public override Task WatchOne(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var polledCurrent = GetContext();

            using var polledLogScope = Logger.BeginScope(new Dictionary<string, object>()
                { { "context", polledCurrent?.DisplayString() } });

            if (polledCurrent != null) StoreCurrentPlaying(polledCurrent);

            // swap new item into live and bump existing down to previous
            Previous = Live;
            Live = polledCurrent;

            OnNetworkPoll(GetEvent());

            CheckPlaying();
            CheckContext();
            CheckItem();
            CheckDevice();

            return Task.CompletedTask;
        }
    }
}