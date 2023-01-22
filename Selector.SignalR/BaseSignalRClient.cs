using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Selector.SignalR;

public abstract class BaseSignalRClient: IAsyncDisposable
{
    private readonly string _baseUrl;
    protected HubConnection hubConnection;

	public BaseSignalRClient(string path)
	{
        var baseOverride = Environment.GetEnvironmentVariable("SELECTOR_BASE_URL");

        if (!string.IsNullOrWhiteSpace(baseOverride))
        {
            _baseUrl = baseOverride;
        }
        else
        {
            _baseUrl = "https://selector.sarsoo.xyz";
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(_baseUrl + "/" + path)
            .WithAutomaticReconnect()
            .Build();
    }

    public ValueTask DisposeAsync()
    {
        return ((IAsyncDisposable)hubConnection).DisposeAsync();
    }

    public async Task StartAsync()
    {
        await hubConnection.StartAsync();
    }
}

