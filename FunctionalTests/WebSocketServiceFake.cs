using MachineStreamCore.Interfaces;
using System;
using System.Threading.Tasks;

namespace FunctionalTests
{
    public class WebSocketServiceFake : IWebSocket
    {
        public async Task<bool> ConnectAndRegisterAsync()
        {
            await Task.Delay(10);
            return true;
        }

        public void DisconnectAndDispose()
        {
        }
    }
}
