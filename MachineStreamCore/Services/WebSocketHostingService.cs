using MachineStreamCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MachineStreamCore.Services
{
    /// <summary>
    /// Implementation of IHostedService that consume a scoped service
    /// This background service is used to connect with the WebSocket
    /// more info at: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
    /// </summary>
    public class WebSocketHostingService : IHostedService
    {
        private readonly ILogger logger;
        public IServiceProvider services { get; }

        /// <summary>
        /// Constructor used by the DI with AddHostedService method
        /// </summary>
        /// <param name="services">The service provider</param>
        /// <param name="logger">The injected logger</param>
        public WebSocketHostingService(IServiceProvider services, ILogger<WebSocketHostingService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        /// <summary>
        /// It contains the logic to start the background task.
        /// When using the Web Host, StartAsync is called after the server has started and IApplicationLifetime.ApplicationStarted is triggered.
        /// When using the Generic Host, StartAsync is called before ApplicationStarted is triggered.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("WebSocketHostingService is starting.");
            using (IServiceScope scope = services.CreateScope())
            {
                IWebSocket iWebSocketScopedService = scope.ServiceProvider.GetRequiredService<IWebSocket>();
                var connect = await iWebSocketScopedService.ConnectAndRegisterAsync();
                logger.LogInformation($"iWebSocketScopedService.ConnectAndRegisterAsync() is connected: {connect}");
            }
        }

        /// <summary>
        /// Triggered when the host is performing a graceful shutdown.
        /// It contains the logic to end the background task.
        /// Implement IDisposable and finalizers (destructors) to dispose of any unmanaged resources.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ThinkInPushHostingService is stopping.");
            using (IServiceScope scope = services.CreateScope())
            {
                IWebSocket iWebSocketScopedService = scope.ServiceProvider.GetRequiredService<IWebSocket>();
                iWebSocketScopedService.DisconnectAndDispose();
            }
            return Task.CompletedTask;
        }
    }
}
