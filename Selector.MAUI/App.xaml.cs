using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Selector.SignalR;

namespace Selector.MAUI;

public partial class App : Application
{
    private readonly NowHubClient nowClient;
    private readonly ILogger<App> logger;

    public App(NowHubClient nowClient, ILogger<App> logger)
    {
        InitializeComponent();

        this.nowClient = nowClient;
        this.logger = logger;
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = new Window(new MainPage());

        window.Resumed += async (s, e) =>
        {
            try
            {
                logger.LogInformation("Window resumed, reconnecting hubs");

                if (nowClient.State == HubConnectionState.Disconnected)
                {
                    await nowClient.StartAsync();
                }

                await nowClient.OnConnected();

                logger.LogInformation("Hubs reconnected");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while reconnecting hubs");
            }
        };

        return window;
    }
}