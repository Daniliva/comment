using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Comments.Infrastructure.Services
{
    public class CustomWebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public string AddSocket(WebSocket socket)
        {
            string id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, socket);
            return id;
        }

        public async Task RemoveSocketAsync(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by the server", CancellationToken.None);
                }
                socket.Dispose();
            }
        }

        public async Task BroadcastAsync(object message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            var segment = new ArraySegment<byte>(buffer);

            var closedSockets = new List<string>();

            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    try
                    {
                        await pair.Value.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        closedSockets.Add(pair.Key);
                    }
                }
                else
                {
                    closedSockets.Add(pair.Key);
                }
            }

            foreach (var id in closedSockets)
            {
                await RemoveSocketAsync(id);
            }
        }
    }
}

