using System.Threading.Tasks;

namespace Selector.SignalR;

public interface IPastHub
{
    Task OnConnected();
    Task OnSubmitted(IPastParams param);
}

public interface IPastHubClient
{
    public Task OnRankResult(IRankResult result);
}