using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BaseParallelPlayerConsumer<TWatcher, TArgs>(
    TWatcher? watcher,
    ILogger<BaseParallelPlayerConsumer<TWatcher, TArgs>> logger)
    : BasePlayerConsumer<TWatcher, TArgs>(watcher, logger), IConsumer<TArgs>
    where TWatcher : IWatcher<TArgs>
{
    protected new readonly ILogger<BaseParallelPlayerConsumer<TWatcher, TArgs>> Logger = logger;

    public override void Callback(object? sender, TArgs e)
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
}