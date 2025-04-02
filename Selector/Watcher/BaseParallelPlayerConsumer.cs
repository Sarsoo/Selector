using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BaseParallelPlayerConsumer<TWatcher, TArgs>(
    TWatcher watcher,
    ILogger<BaseParallelPlayerConsumer<TWatcher, TArgs>> logger) : IConsumer<TArgs>
    where TWatcher : IWatcher<TArgs>
{
    protected readonly ILogger<BaseParallelPlayerConsumer<TWatcher, TArgs>> Logger = logger;

    public void Callback(object sender, TArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                await ProcessEvent(e);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occured during callback");
            }
        });
    }

    protected abstract Task ProcessEvent(TArgs e);

    public void Subscribe(IWatcher? watch = null)
    {
        var watcher1 = watch ?? watcher ?? throw new ArgumentNullException("No watcher provided");

        if (watcher1 is TWatcher watcherCast)
        {
            watcherCast.ItemChange += Callback;
        }
        else
        {
            throw new ArgumentException("Provided watcher is not a PlayerWatcher");
        }
    }

    public void Unsubscribe(IWatcher? watch = null)
    {
        var watcher1 = watch ?? watcher ?? throw new ArgumentNullException("No watcher provided");

        if (watcher1 is TWatcher watcherCast)
        {
            watcherCast.ItemChange -= Callback;
        }
        else
        {
            throw new ArgumentException("Provided watcher is not a PlayerWatcher");
        }
    }
}