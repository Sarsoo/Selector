﻿using Microsoft.Extensions.Logging;
using Radzen;
using Selector.MAUI.Extensions;
using Selector.MAUI.Services;

namespace Selector.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddLogging(o =>
        {
            //o.AddConsole();
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddHttpClient()
            .AddTransient<ISelectorNetClient, SelectorNetClient>();

        builder.Services.AddSingleton<SessionManager>()
            .AddTransient<StartPageManager>();

        builder.Services.AddRadzenComponents();

        builder.Services.AddHubs();

        return builder.Build();
    }
}