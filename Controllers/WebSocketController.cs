using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PLCDataCollector.Service.Implementation;
using System.Net.WebSockets;

namespace PLCDataCollector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(IWebSocketService webSocketService, ILogger<WebSocketController> logger)
        {
            _webSocketService = webSocketService;
            _logger = logger;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var connectionId = Guid.NewGuid().ToString();
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                await _webSocketService.AddConnectionAsync(webSocket, connectionId);

                try
                {
                    var buffer = new byte[1024 * 4];
                    var cancellationToken = HttpContext.RequestAborted;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }

                        // Handle incoming messages if needed (ping/pong, subscriptions, etc.)
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                            _logger.LogDebug("Received WebSocket message from {ConnectionId}: {Message}", connectionId, message);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal when connection is closed
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket error for connection {ConnectionId}", connectionId);
                }
                finally
                {
                    await _webSocketService.RemoveConnectionAsync(connectionId);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
