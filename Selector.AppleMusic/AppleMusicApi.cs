using System.Net;
using System.Net.Http.Json;
using Selector.AppleMusic.Exceptions;
using Selector.AppleMusic.Model;

namespace Selector.AppleMusic;

public class AppleMusicApi(HttpClient client, string developerToken, string userToken)
{
    private static readonly string _apiBaseUrl = "https://api.music.apple.com/v1";
    private readonly AppleJsonContext _appleJsonContext = AppleJsonContext.Default;

    private async Task<HttpResponseMessage> MakeRequest(HttpMethod httpMethod, string requestUri)
    {
        var request = new HttpRequestMessage(httpMethod, _apiBaseUrl + requestUri);
        request.Headers.Add("Authorization", "Bearer " + developerToken);
        request.Headers.Add("Music-User-Token", userToken);
        var response = await client.SendAsync(request);

        return response;
    }

    private void CheckResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorisedException { StatusCode = response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException { StatusCode = response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RateLimitException { StatusCode = response.StatusCode };
            }
            else
            {
                throw new AppleMusicException { StatusCode = response.StatusCode };
            }
        }
    }

    public async Task<RecentlyPlayedTracksResponse?> GetRecentlyPlayedTracks()
    {
        using var span = Trace.Tracer.StartActivity();
        var response = await MakeRequest(HttpMethod.Get, "/me/recent/played/tracks");

        CheckResponse(response);

        var parsed = await response.Content.ReadFromJsonAsync(_appleJsonContext.RecentlyPlayedTracksResponse);
        return parsed;
    }
}