using System.Threading.Tasks;

namespace Selector.SignalR;

public interface IPastHub
{
    Task OnConnected();
    Task OnSubmitted(PastParams param);
}

public interface IPastHubClient
{
    public Task OnRankResult(IRankResult result);
}