using System;
using Microsoft.AspNetCore.Components;

namespace Selector.MAUI.Services;

public class StartPageManager
{
    private readonly NavigationManager navManager;

    public string[] StartPages { get; } = new[]
    {
        Home, Now, Past
    };

    public const string Home = "Home";
    public const string Now = "Now";
    public const string Past = "Past";

    public StartPageManager(NavigationManager navManager)
	{
        this.navManager = navManager;
    }

	public string GetStartPage()
	{
        var savedStartPage = Preferences.Default.Get(Constants.StartPagePrefKey, string.Empty);

        if (!string.IsNullOrWhiteSpace(savedStartPage))
        {
            return savedStartPage;
        }
        else
        {
            return Home;
        }
    }

    public void NavigateToStartPage()
    {
        var startPage = GetStartPage();

        switch (startPage)
        {
            case Now:
                navManager.NavigateTo("/now");
                break;
            case Past:
                navManager.NavigateTo("/past");
                break;
            case Home:
            default:
                navManager.NavigateTo("/app");
                break;
        }
    }

    public void SetStartPage(string value)
    {
        Preferences.Default.Set(Constants.StartPagePrefKey, value);
    }
}

