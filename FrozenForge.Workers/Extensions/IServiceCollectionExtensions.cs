using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FrozenForge.Workers.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default <see cref="BackgroundTaskExecutor"/> as an <see cref="IBackgroundTaskExecutor"/> singleton service and hosted service to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBackgroundTaskExecutor(this IServiceCollection services)            
        {
            return services
                .AddBackgroundTaskExecutor<IBackgroundTaskExecutor, BackgroundTaskExecutor>();
        }

        public static IServiceCollection AddBackgroundTaskExecutor<TService, TImplementation>(this IServiceCollection services)
            where TService : class, IBackgroundTaskExecutor
            where TImplementation : class, TService, IHostedService
        {
            return services
                .AddSingleton<TService, TImplementation>()
                .AddHostedService(services => services.GetRequiredService<TService>() as IHostedService);
        } 
    }
}
