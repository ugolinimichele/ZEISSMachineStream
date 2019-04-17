using System.Threading.Tasks;

namespace MachineStreamCore.Interfaces
{
    /// <summary>
    /// Interface to manage the connection to the WebSocket
    /// </summary>
    public interface IWebSocket
    {
        /// <summary>
        /// Connect the WebSocket and register all the events
        /// </summary>
        /// <returns>true if the connection was successful, false otherwise</returns>
        Task<bool> ConnectAndRegisterAsync();

        /// <summary>
        /// Disconnect the WebSocket and dispose the objects involved
        /// </summary>
        void DisconnectAndDispose();
    }
}
