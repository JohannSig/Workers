using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FrozenForge.Workers
{
    public class BackgroundTaskExecutor : BackgroundService, IBackgroundTaskExecutor
    {
        public BackgroundTaskExecutor(
            ILogger<BackgroundTaskExecutor> logger,
            IServiceProvider services)
        {
            Logger = logger;
            Services = services;
            Semaphore = new SemaphoreSlim(0);
            Tasks = new ConcurrentQueue<Func<IServiceProvider, CancellationToken, Task>>();
        }

        public ILogger<BackgroundTaskExecutor> Logger { get; }
        
        public IServiceProvider Services { get; }
        
        public SemaphoreSlim Semaphore { get; }

        public ConcurrentQueue<Func<IServiceProvider, CancellationToken, Task>> Tasks { get; set; } 

        public void Enqueue<TService>(Func<TService, Task> backgroundTask) => Enqueue<TService>((services, cancellationToken) => backgroundTask(services));

        public void Enqueue<TService>(Func<TService, CancellationToken, Task> backgroundTask)
        {
            Tasks.Enqueue((services, cancellationToken) => backgroundTask(services.CreateScope().ServiceProvider.GetRequiredService<TService>(), cancellationToken));
            Semaphore.Release();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Semaphore.WaitAsync(stoppingToken);

                if (!Tasks.TryDequeue(out var task))
                {
                    this.Logger.LogError($"Unable to dequeue a task");
                    continue;
                }

                await task(Services, stoppingToken);
            }
        }
    }
}
