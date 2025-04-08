using Microsoft.AspNetCore.SignalR.Client;

namespace Selector.SignalR;

public class PastHubClient : BaseSignalRClient, IPastHub, IDisposable
{
    private List<IDisposable> _searchResultCallbacks = new();
    private bool _disposedValue;

    public PastHubClient(string? token = null) : base("nowhub", token)
    {
    }

    public void OnRankResult(Action<IRankResult> action)
    {
        _searchResultCallbacks.Add(hubConnection.On(nameof(OnRankResult), action));
    }

    public Task OnConnected()
    {
        return hubConnection.InvokeAsync(nameof(OnConnected));
    }

    public Task OnSubmitted(PastParams param)
    {
        return hubConnection.InvokeAsync(nameof(OnSubmitted), param);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var callback in _searchResultCallbacks)
                {
                    callback.Dispose();
                }

                base.DisposeAsync();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}