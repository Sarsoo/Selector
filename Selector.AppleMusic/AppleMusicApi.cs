namespace Selector.AppleMusic;

public class AppleMusicApi(HttpClient client, string developerToken, string userToken)
{
    private static readonly string _apiBaseUrl = "https://api.music.apple.com/v1";

    private async Task<HttpResponseMessage> MakeRequest(HttpMethod httpMethod, string requestUri)
    {
        var request = new HttpRequestMessage(httpMethod, _apiBaseUrl + requestUri);
        request.Headers.Add("Authorization", "Bearer " + developerToken);
        request.Headers.Add("Music-User-Token", userToken);
        var response = await client.SendAsync(request);

        return response;
    }

    public async Task GetRecentlyPlayedTracks()
    {
        var response = await MakeRequest(HttpMethod.Get, "/me/recent/played/tracks");
    }
}