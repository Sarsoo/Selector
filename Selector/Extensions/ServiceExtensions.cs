using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Selector.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddConsumerFactories(this IServiceCollection services)
        {
            services.AddTransient<IAudioFeatureInjectorFactory, AudioFeatureInjectorFactory>();
            services.AddTransient<IPlayCounterFactory, PlayCounterFactory>();
        }
    }
}
