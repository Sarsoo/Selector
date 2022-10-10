using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class DurationPuller
    {
        private readonly IDatabaseAsync Cache;
        private readonly ILogger<DurationPuller> Logger;

        protected readonly ITracksClient SpotifyClient;

        private int _retries = 0;


        public DurationPuller(
            ILogger<DurationPuller> logger,

            ITracksClient spotifyClient,
            IDatabaseAsync cache = null
        )
        {
            Cache = cache;
            Logger = logger;

            SpotifyClient = spotifyClient;
        }

        public async Task<int?> Get(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException("No uri provided");

            var trackId = uri.Split(":").Last();

            var cachedVal = await Cache?.HashGetAsync(Key.Track(trackId), Key.Duration);
            if (Cache is null || cachedVal == RedisValue.Null || cachedVal.IsNullOrEmpty)
            {
                try {
                    Logger.LogDebug("Missed cache, pulling");

                    var info = await SpotifyClient.Get(trackId);

                    await Cache?.SetTrackDuration(trackId, info.DurationMs, TimeSpan.FromDays(7));

                    _retries = 0;

                    return info.DurationMs;
                }
                catch (APIUnauthorizedException e)
                {
                    Logger.LogError("Unauthorised error: [{message}] (should be refreshed and retried?)", e.Message);
                    throw e;
                }
                catch (APITooManyRequestsException e)
                {
                    if(_retries <= 3)
                    {
                        Logger.LogWarning("Too many requests error, retrying ({}): [{message}]", e.RetryAfter, e.Message);
                        _retries++;
                        await Task.Delay(e.RetryAfter);
                        return await Get(uri);
                    }
                    else
                    {
                        Logger.LogError("Too many requests error, done retrying: [{message}]", e.Message);
                        throw e;
                    }
                }
                catch (APIException e)
                {
                    if (_retries <= 3)
                    {
                        Logger.LogWarning("API error, retrying: [{message}]", e.Message);
                        _retries++;
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        return await Get(uri);
                    }
                    else
                    {
                        Logger.LogError("API error, done retrying: [{message}]", e.Message);
                        throw e;
                    }
                }
            }
            else
            {
                return (int?) cachedVal;
            }
        }

        public async Task<IDictionary<string, int>> Get(IEnumerable<string> uri)
        {
            if (!uri.Any()) throw new ArgumentNullException("No URIs provided");

            var ret = new Dictionary<string, int>();
            var toPullFromSpotify = new List<string>();

            foreach (var input in uri.Select(x => x.Split(":").Last()))
            {
                var cachedVal = await Cache?.HashGetAsync(Key.Track(input), Key.Duration);

                if (Cache is null || cachedVal == RedisValue.Null || cachedVal.IsNullOrEmpty)
                {
                    toPullFromSpotify.Add(input);
                }
                else
                {
                    ret[input] = (int) cachedVal;
                }
            }

            var retries = new List<string>();

            foreach(var chunk in toPullFromSpotify.Chunk(50))
            {
                await PullChunk(chunk, ret);
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }            

            return ret;
        }

        private async Task PullChunk(IList<string> toPull, IDictionary<string, int> ret)
        {
            try
            {
                var info = await SpotifyClient.GetSeveral(new(toPull));

                foreach (var resp in info.Tracks)
                {
                    await Cache?.SetTrackDuration(resp.Id, resp.DurationMs, TimeSpan.FromDays(7));

                    ret[resp.Id] = (int)resp.DurationMs;
                }

                _retries = 0;
            }
            catch (APIUnauthorizedException e)
            {
                Logger.LogError("Unauthorised error: [{message}] (should be refreshed and retried?)", e.Message);
                throw e;
            }
            catch (APITooManyRequestsException e)
            {
                if (_retries <= 3)
                {
                    Logger.LogWarning("Too many requests error, retrying ({}): [{message}]", e.RetryAfter, e.Message);
                    _retries++;
                    await Task.Delay(e.RetryAfter);

                    await PullChunk(toPull, ret);
                }
                else
                {
                    Logger.LogError("Too many requests error, done retrying: [{message}]", e.Message);
                    throw e;
                }
            }
            catch (APIException e)
            {
                if (_retries <= 3)
                {
                    Logger.LogWarning("API error, retrying: [{message}]", e.Message);
                    _retries++;
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    await PullChunk(toPull, ret);
                }
                else
                {
                    Logger.LogError("API error, done retrying: [{message}]", e.Message);
                    throw e;
                }
            }
        }
    }
}
