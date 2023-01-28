using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Selector.SignalR;
namespace Selector.MAUI.Services;

public class HubManager
{
    private readonly NowHubClient nowClient;
    private readonly NowHubCache nowCache;
    private readonly PastHubClient pastClient;
    private readonly ILogger<HubManager> logger;

    public HubManager(NowHubClient nowClient, NowHubCache nowCache, PastHubClient pastClient, ILogger<HubManager> logger)
    {
        this.nowClient = nowClient;
        this.nowCache = nowCache;
        this.pastClient = pastClient;
        this.logger = logger;
    }


    public async Task EnsureConnected()
    {
        if (nowClient.State == HubConnectionState.Disconnected)
        {
            logger.LogInformation("Starting now hub connection");

            await nowClient.StartAsync();
            nowCache.BindClient();
            await nowClient.OnConnected();
        }

        if (pastClient.State == HubConnectionState.Disconnected)
        {
            logger.LogInformation("Starting past hub connection");

            await pastClient.StartAsync();
            await pastClient.OnConnected();
        }
    }
}

