using Microsoft.AspNetCore.SignalR.Client;

namespace Selector.SignalR;

public class PastHubClient: BaseSignalRClient, IPastHub, IDisposable
{
    private List<IDisposable> SearchResultCallbacks = new();
    private bool disposedValue;

    public PastHubClient(string token = null): base("nowhub", token)
	{
	}

    public void OnRankResult(Action<IRankResult> action)
    {
        SearchResultCallbacks.Add(hubConnection.On(nameof(OnRankResult), action));
    }

    public Task OnConnected()
    {
        return hubConnection.InvokeAsync(nameof(OnConnected));
    }

    public Task OnSubmitted(IPastParams param)
    {
        return hubConnection.InvokeAsync(nameof(OnSubmitted), param);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach(var callback in SearchResultCallbacks)
                {
                    callback.Dispose();
                }

                base.DisposeAsync();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

