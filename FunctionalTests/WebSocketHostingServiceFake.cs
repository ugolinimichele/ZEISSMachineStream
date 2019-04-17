using MachineStreamCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalTests
{
    public class WebSocketHostingServiceFake : IHostedService
    {
        public IServiceProvider services { get; }

        public WebSocketHostingServiceFake(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = services.CreateScope())
            {
                IWebSocket iWebSocketScopedService = scope.ServiceProvider.GetRequiredService<IWebSocket>();
                var connect = await iWebSocketScopedService.ConnectAndRegisterAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = services.CreateScope())
            {
                IWebSocket iWebSocketScopedService = scope.ServiceProvider.GetRequiredService<IWebSocket>();
                iWebSocketScopedService.DisconnectAndDispose();
            }
            return Task.CompletedTask;
        }
    }
}
