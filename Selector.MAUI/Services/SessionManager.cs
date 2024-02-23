using System;
using Microsoft.Extensions.Logging;

namespace Selector.MAUI.Services;

public class SessionManager
{
	private string lastStoredKey;
    private DateTime lastRefresh;
	private readonly ISelectorNetClient _selectorNetClient;
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(ISelectorNetClient selectorNetClient, ILogger<SessionManager> logger)
	{
		_selectorNetClient = selectorNetClient;
        _logger = logger;
    }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(lastStoredKey);

	public async Task LoadUserFromDisk()
	{
        //var lastToken = await SecureStorage.Default.GetAsync(jwt_keychain_key);
        var lastToken = Preferences.Default.Get(Constants.JwtPrefKey, string.Empty);

        lastStoredKey = lastToken;

        if(!string.IsNullOrWhiteSpace(lastToken))
        {
            await Authenticate();
        }
    }

    public async Task<SelectorNetClient.TokenResponseStatus> Authenticate(string username, string password)
    {
        _logger.LogDebug("Making login token request");

        var tokenResponse = await _selectorNetClient.GetToken(username, password);

        return await HandleTokenResponse(tokenResponse);
    }

    public async Task<SelectorNetClient.TokenResponseStatus> Authenticate()
    {
        _logger.LogDebug("Making token request with current key");

        var tokenResponse = await _selectorNetClient.GetToken(lastStoredKey);

        return await HandleTokenResponse(tokenResponse);
    }

    private Task<SelectorNetClient.TokenResponseStatus> HandleTokenResponse(SelectorNetClient.TokenResponse tokenResponse)
	{
        switch (tokenResponse.Status)
        {
            case SelectorNetClient.TokenResponseStatus.OK:
                _logger.LogInformation("Token response ok");
                lastStoredKey = tokenResponse.Token;
                lastRefresh = DateTime.Now;

                //await SecureStorage.Default.SetAsync(jwt_keychain_key, lastStoredKey);
                // I know, but I can't get secure storage to work
                Preferences.Default.Set(Constants.JwtPrefKey, lastStoredKey);

                break;
            case SelectorNetClient.TokenResponseStatus.Malformed:
                _logger.LogInformation("Token request failed, missing username or password");

                break;
            case SelectorNetClient.TokenResponseStatus.UserSearchFailed:
                _logger.LogInformation("Token request failed, no user by that name");

                break;
            case SelectorNetClient.TokenResponseStatus.ExpiredCreds:
                _logger.LogInformation("Token expired, log back in");
                lastStoredKey = null;
                lastRefresh = DateTime.Now;

                Preferences.Default.Remove(Constants.JwtPrefKey);

                break;
            case SelectorNetClient.TokenResponseStatus.BadCreds:
                _logger.LogInformation("Token request failed, bad password");

                break;
            default:
                throw new NotImplementedException();
        }

        return Task.FromResult(tokenResponse.Status);
    }

    public void SignOut()
    {
        lastStoredKey = null;
        Preferences.Default.Clear();
    }
}

