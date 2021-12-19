using Microsoft.Extensions.DependencyInjection;

namespace Selector.Events
{
    public static class ServiceExtensions
    {
        public static void AddEvents(this IServiceCollection services)
        {
            services.AddEventBus();
            services.AddEventMappingAgent();
        }

        public static void AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>();
            services.AddSingleton<IEventBus, UserEventBus>(sp => sp.GetRequiredService<UserEventBus>());
        }

        public static void AddEventMappingAgent(this IServiceCollection services)
        {
            services.AddHostedService<EventMappingService>();
        }
    }
}
