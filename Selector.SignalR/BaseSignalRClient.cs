using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Selector.SignalR;

public abstract class BaseSignalRClient: IAsyncDisposable
{
    private readonly string _baseUrl;
    protected HubConnection hubConnection;
    public string Token { get; set; }

	public BaseSignalRClient(string path, string token)
	{
        //var baseOverride = Environment.GetEnvironmentVariable("SELECTOR_BASE_URL");

        //if (!string.IsNullOrWhiteSpace(baseOverride))
        //{
        //    _baseUrl = baseOverride;
        //}
        //else
        //{
        //    _baseUrl = "https://selector.sarsoo.xyz";
        //}

        _baseUrl = "http://localhost:5000";

        hubConnection = new HubConnectionBuilder()
            .WithUrl(_baseUrl + "/" + path, options =>
            {
                options.AccessTokenProvider = () =>
                {
                    return Task.FromResult(Token);
                };
            })
            .WithAutomaticReconnect()
            .Build();
    }

    public HubConnectionState State => hubConnection.State;

    public ValueTask DisposeAsync()
    {
        return ((IAsyncDisposable)hubConnection).DisposeAsync();
    }

    public async Task StartAsync()
    {
        await hubConnection.StartAsync();
    }
}

