using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PureWebSockets;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MachineStreamCore.Services
{
    /// <summary>
    /// Implementation of the IWebSocket interface used to retrieve the stream of events from the web socket
    /// </summary>
    public class WebSocketService : IWebSocket
    {
        private readonly ILogger logger;
        private readonly WebSocketConfiguration webSocketConfiguration;
        private readonly PureWebSocketOptions socketOptions;
        private readonly IStoreData storeEvents;

        /// <summary>
        /// Constructor used by the DI
        /// </summary>
        /// <param name="storeEvents">The service used to store the data</param>
        /// <param name="optionsWebScoket">The IOptions that contains the web socket configurations</param>
        /// <param name="logger">The injected logger</param>
        public WebSocketService(IStoreData storeEvents, IOptions<WebSocketConfiguration> optionsWebScoket, ILogger<WebSocketService> logger)
        {
            this.logger = logger;
            webSocketConfiguration = optionsWebScoket.Value;
            //with these options the socket will never go over the 60 seconds timeout
            socketOptions = new PureWebSocketOptions()
            {
                DebugMode = webSocketConfiguration.DebugMode,
                SendDelay = webSocketConfiguration.SendDelay,
                MyReconnectStrategy = new ReconnectStrategy(
                    webSocketConfiguration.MinReconnectInterval,
                    webSocketConfiguration.MaxReconnectInterval,
                    null
                )
            };
            this.storeEvents = storeEvents;
        }

        /// <summary>
        /// Connect the WebSocket and register all the events
        /// </summary>
        /// <returns>true if the connection was successful, false otherwise</returns>
        public async Task<bool> ConnectAndRegisterAsync()
        {
            logger.LogInformation("ConnectAndRegisterAsync() is working.");
            PureWebSocket pureWebSocket = new PureWebSocket(webSocketConfiguration.FullUrl, socketOptions);
            pureWebSocket.OnStateChanged += WSOnStateChanged;
            pureWebSocket.OnMessage += WSOnMessage;
            pureWebSocket.OnOpened += WSOnOpened;
            pureWebSocket.OnError += WSOnError;
            logger.LogDebug("ConnectAndRegisterAsync() All custom events registered");
            return await pureWebSocket.ConnectAsync();
        }

        private void WSOnOpened()
        {
            logger.LogInformation("OnOpened - Access WebSocket");
        }

        private void WSOnError(Exception error)
        {
            logger.LogError(error, "OnError - Error {error}");
        }

        private void WSOnClosed(WebSocketCloseStatus reason)
        {
            logger.LogInformation($"OnClosed - Connection Closed: {reason.ToString()}");
        }

        private void WSOnMessage(string message)
        {
            logger.LogInformation($"OnMessage - New message: {message}");
            StreamEvent streamEvent = JsonConvert.DeserializeObject<StreamEvent>(message);
            //storeEvents.StoreEventAsync(streamEvent, webSocketConfiguration.FullUrl).GetAwaiter().GetResult();
            Task.Run(() => storeEvents.StoreEventAsync(streamEvent, webSocketConfiguration.FullUrl));
        }

        private void WSOnStateChanged(WebSocketState newState, WebSocketState prevState)
        {
            logger.LogInformation($"OnStateChanged - Status changed from {prevState} to {newState}");
        }

        /// <summary>
        /// Disconnect the WebSocket and dispose the objects involved
        /// </summary>
        public void DisconnectAndDispose()
        {
            logger.LogInformation("DisconnectAndDispose() is being called");
            PureWebSocket pureWebSocket = new PureWebSocket(webSocketConfiguration.FullUrl, socketOptions);
            pureWebSocket?.Disconnect();
            pureWebSocket?.Dispose();
        }
    }
}
