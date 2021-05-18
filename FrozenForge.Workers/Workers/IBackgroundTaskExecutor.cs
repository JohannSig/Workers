using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FrozenForge.Workers
{
    public interface IBackgroundTaskExecutor
    {
        void Enqueue<TService>(Func<TService, Task> backgroundTask);
        
        void Enqueue<TService>(Func<TService, CancellationToken, Task> backgroundTask);
    }
}
