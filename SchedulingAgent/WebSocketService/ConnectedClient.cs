using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace SchedulingAgent.WebSocketService
{
    public class ConnectedClient
    {
        public Int32 SocketId { get; private set; }
        public WebSocket Socket { get; private set; }
        public BlockingCollection<String> BroadcastQueue { get; } = new BlockingCollection<String>();
        public CancellationTokenSource BroadcastLoopTokenSource { get; set; } = new CancellationTokenSource();

        public ConnectedClient(Int32 socketId, WebSocket socket)
        {
            SocketId = socketId;
            Socket = socket;
        }
        public async Task BroadcastLoopAsync()
        {
            CancellationToken objCancellationToken = BroadcastLoopTokenSource.Token;
            while (!objCancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(Constants.BROADCAST_TRANSMIT_INTERVAL_MS, objCancellationToken);
                    if (!objCancellationToken.IsCancellationRequested && Socket.State == WebSocketState.Open && BroadcastQueue.TryTake(out String message))
                    {
                        ArraySegment<Byte> msgbuf = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message));
                        await Socket.SendAsync(msgbuf, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                catch (OperationCanceledException) { } //Normal upon task/token cancellation...
                catch (Exception ex)
                {
                    //Bubble event up?
                }
            }
        }

    }
}