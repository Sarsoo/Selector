using System;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.SignalR;

public class NowHubCache
{
	private readonly NowHubClient _connection;
    private readonly ILogger<NowHubCache> logger;

    public TrackAudioFeatures LastFeature { get; private set; }
	public List<ICard> LastCards { get; private set; } = new();
	private readonly object updateLock = new();

	public PlayCount LastPlayCount { get; private set; } 
	public CurrentlyPlayingDTO LastPlaying { get; private set; }

	public event EventHandler NewAudioFeature;
	public event EventHandler NewCard;
	public event EventHandler NewPlayCount;
	public event EventHandler NewNowPlaying;

    public NowHubCache(NowHubClient connection, ILogger<NowHubCache> logger)
	{
		_connection = connection;
        this.logger = logger;
    }

	public void BindClient()
	{
		_connection.OnNewAudioFeature(af =>
		{
			lock (updateLock)
			{
				logger.LogInformation("New audio features received: {0}", af);
				LastFeature = af;
				NewAudioFeature?.Invoke(this, null);
			}
		});

		_connection.OnNewCard(c =>
		{
			lock(updateLock)
			{
                logger.LogInformation("New card received: {0}", c);
                LastCards.Add(c);
				NewCard?.Invoke(this, null);
            }
        });

		_connection.OnNewPlayCount(pc =>
		{
			lock (updateLock)
			{
				logger.LogInformation("New play count received: {0}", pc);
				LastPlayCount = pc;
				NewPlayCount?.Invoke(this, null);
            }
		});

		_connection.OnNewPlaying(async np =>
		{
			try
			{
				lock (updateLock)
				{
					logger.LogInformation("New now playing recieved: {0}", np);
					LastPlaying = np;
					LastCards.Clear();
					NewNowPlaying?.Invoke(this, null);
                }

				if (LastPlaying?.Track is not null)
				{
					if (!string.IsNullOrWhiteSpace(LastPlaying.Track.Id))
					{
						await _connection.SendAudioFeatures(LastPlaying.Track.Id);
					}

					await _connection.SendPlayCount(
						LastPlaying.Track.Name,
						LastPlaying.Track.Artists.FirstOrDefault()?.Name,
						LastPlaying.Track.Album?.Name,
						LastPlaying.Track.Album?.Artists.FirstOrDefault()?.Name
					);

					await _connection.SendFacts(
						LastPlaying.Track.Name,
						LastPlaying.Track.Artists.FirstOrDefault()?.Name,
						LastPlaying.Track.Album?.Name,
						LastPlaying.Track.Album?.Artists.FirstOrDefault()?.Name
					);
				}
			}catch(Exception e)
			{
				logger.LogError(e, "Error while handling new now playing");
			}
        });
	}
}

