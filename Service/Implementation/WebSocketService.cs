using PLCDataCollector.Model;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PLCDataCollector.Service.Implementation
{
    public interface IWebSocketService
    {
        Task AddConnectionAsync(WebSocket webSocket, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
        Task BroadcastProductionDataAsync(string lineId, ProductionData data);
        Task BroadcastGraphDataAsync(string lineId, RealTimeGraphData graphData);
        Task BroadcastAlertAsync(ProductionAlert alert);
    }

    public class WebSocketService : IWebSocketService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ILogger<WebSocketService> _logger;

        public WebSocketService(ILogger<WebSocketService> logger)
        {
            _logger = logger;
        }

        public async Task AddConnectionAsync(WebSocket webSocket, string connectionId)
        {
            _connections.TryAdd(connectionId, webSocket);
            _logger.LogInformation("WebSocket connection added: {ConnectionId}", connectionId);

            // Send welcome message
            await SendMessageAsync(webSocket, new
            {
                type = "connection",
                message = "Connected to PLC Data Collector",
                connectionId,
                timestamp = DateTime.Now
            });
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var webSocket))
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
                _logger.LogInformation("WebSocket connection removed: {ConnectionId}", connectionId);
            }
        }

        public async Task BroadcastProductionDataAsync(string lineId, ProductionData data)
        {
            var message = new
            {
                type = "production-data",
                lineId,
                data,
                timestamp = DateTime.Now
            };

            await BroadcastToAllAsync(message);
        }

        public async Task BroadcastGraphDataAsync(string lineId, RealTimeGraphData graphData)
        {
            var message = new
            {
                type = "graph-data",
                lineId,
                data = graphData,
                timestamp = DateTime.Now
            };

            await BroadcastToAllAsync(message);
        }

        public async Task BroadcastAlertAsync(ProductionAlert alert)
        {
            var message = new
            {
                type = "alert",
                data = alert,
                timestamp = DateTime.Now
            };

            await BroadcastToAllAsync(message);
        }

        private async Task BroadcastToAllAsync(object message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);

            var deadConnections = new List<string>();

            foreach (var connection in _connections)
            {
                try
                {
                    if (connection.Value.State == WebSocketState.Open)
                    {
                        await connection.Value.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    else
                    {
                        deadConnections.Add(connection.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message to connection {ConnectionId}", connection.Key);
                    deadConnections.Add(connection.Key);
                }
            }

            // Clean up dead connections
            foreach (var deadConnectionId in deadConnections)
            {
                await RemoveConnectionAsync(deadConnectionId);
            }
        }

        private async Task SendMessageAsync(WebSocket webSocket, object message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(jsonMessage);

                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }
}
