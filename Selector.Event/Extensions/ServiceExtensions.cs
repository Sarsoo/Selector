using Microsoft.Extensions.DependencyInjection;

namespace Selector.Events
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services)
        {
            services.AddEventBus();
            services.AddEventMappingAgent();

            services.AddTransient<IUserEventFirerFactory, UserEventFirerFactory>();
            services.AddTransient<UserEventFirerFactory>();

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>();
            services.AddSingleton<IEventBus, UserEventBus>(sp => sp.GetRequiredService<UserEventBus>());

            return services;
        }

        public static IServiceCollection AddEventMappingAgent(this IServiceCollection services)
        {
            services.AddHostedService<EventMappingService>();

            return services;
        }
    }
}
