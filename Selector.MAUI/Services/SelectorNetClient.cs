﻿using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Selector.SignalR;

namespace Selector.MAUI.Services;

public interface ISelectorNetClient
{
    Task<SelectorNetClient.TokenResponse> GetToken(string username, string password);
    Task<SelectorNetClient.TokenResponse> GetToken(string currentKey);
}

public class SelectorNetClient : ISelectorNetClient
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly NowHubClient _nowClient;
    private readonly PastHubClient _pastClient;

    public SelectorNetClient(HttpClient client, NowHubClient nowClient, PastHubClient pastClient)
    {
        _client = client;
        _nowClient = nowClient;
        _pastClient = pastClient;

        //var baseOverride = Environment.GetEnvironmentVariable("SELECTOR_BASE_URL");

        //if (!string.IsNullOrWhiteSpace(baseOverride))
        //{
        //    _baseUrl = baseOverride;
        //}
        //else
        //{
        //    _baseUrl = "https://selector.sarsoo.xyz";
        //}

        // _baseUrl = "http://localhost:5000";
        _baseUrl = "https://selector.sarsoo.xyz";
    }

    public async Task<TokenResponse> GetToken(string username, string password)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(username);
        ArgumentNullException.ThrowIfNullOrEmpty(password);

        var result = await _client.PostAsync(_baseUrl + "/api/auth/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "Username", username },
                { "Password", password }
            }));

        return FormTokenResponse(result);
    }

    public async Task<TokenResponse> GetToken(string currentKey)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(currentKey);

        var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/api/auth/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentKey);

        var result = await _client.SendAsync(request);

        return FormTokenResponse(result);
    }

    private TokenResponse FormTokenResponse(HttpResponseMessage result)
    {
        var ret = new TokenResponse();

        switch (result.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                ret.Status = TokenResponseStatus.Malformed;
                break;
            case HttpStatusCode.NotFound:
                ret.Status = TokenResponseStatus.UserSearchFailed;
                break;
            case HttpStatusCode.Unauthorized:
                ret.Status = TokenResponseStatus.BadCreds;
                break;
            case HttpStatusCode.Forbidden:
                ret.Status = TokenResponseStatus.ExpiredCreds;
                break;
            case HttpStatusCode.OK:
                ret.Status = TokenResponseStatus.OK;
                ret.Token = result.Content.ReadFromJsonAsync(MauiJsonContext.Default.TokenNetworkResponse).Result.Token;
                _nowClient.Token = ret.Token;
                _pastClient.Token = ret.Token;
                break;
            default:
                break;
        }

        return ret;
    }

    public class TokenResponse
    {
        public string Token { get; set; }
        public TokenResponseStatus Status { get; set; }
    }

    public class TokenNetworkResponse
    {
        public string Token { get; set; }
    }

    public enum TokenResponseStatus
    {
        Malformed,
        UserSearchFailed,
        BadCreds,
        ExpiredCreds,
        OK
    }

    private class TokenModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}