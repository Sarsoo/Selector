using Selector.Web.Apple;

namespace Selector.AppleMusic;

public class AppleMusicApiProvider(HttpClient client)
{
    public AppleMusicApi GetApi(string developerKey, string teamId, string keyId, string userKey)
    {
        var jwtGenerator = new TokenGenerator(developerKey, teamId, keyId);
        var developerToken = jwtGenerator.Generate();

        var api = new AppleMusicApi(client, developerToken, userKey);

        return api;
    }
}