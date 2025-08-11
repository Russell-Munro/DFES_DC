using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;

using Newtonsoft.Json;

using UDC.Common;
using UDC.SchedulingAgent;

using SchedulingAgent.Models;

using static UDC.Common.Constants;

namespace SchedulingAgent.WebSocketService
{
    public class WebSocketServer
    {
        private static HttpListener Listener;

        private static CancellationTokenSource SocketLoopTokenSource;
        private static CancellationTokenSource ListenerLoopTokenSource;

        private static int SocketCounter = 0;
        private static bool ServerIsRunning = true;

        private static ConcurrentDictionary<Int32, ConnectedClient> Clients = new ConcurrentDictionary<Int32, ConnectedClient>();

        public static event EventHandler<SocketStateChangedEventArgs> SocketStateChanged;
        public class SocketStateChangedEventArgs : EventArgs
        {
            public LogTypes LogType { get; set; }
            public String Msg { get; set; }
        }

        public static event EventHandler<CLIEventArgs> CLIEvent;
        public class CLIEventArgs : EventArgs
        {
            public String Command { get; set; }
            public Int64 Arg { get; set; }
        }

        public WebSocketServer()
        {
            
        }

        protected static void OnSocketStateChanged(SocketStateChangedEventArgs e)
        {
            EventHandler<SocketStateChangedEventArgs> handler = SocketStateChanged;
            if (handler != null)
            {
                handler(typeof(WebSocketServer), e);
            }
        }
        private static void RaiseSocketStateChanged(String msg, LogTypes logType)
        {
            SocketStateChangedEventArgs args = new SocketStateChangedEventArgs();
            args.Msg = msg;
            args.LogType = logType;
            OnSocketStateChanged(args);
        }
        protected static void OnCLIEvent(CLIEventArgs e)
        {
            EventHandler<CLIEventArgs> handler = CLIEvent;
            if (handler != null)
            {
                handler(typeof(WebSocketServer), e);
            }
        }
        private static void RaiseCLIEvent(String command, Int64 arg)
        {
            CLIEventArgs args = new CLIEventArgs();
            args.Command = command;
            args.Arg = arg;
            OnCLIEvent(args);
        }

        public static void Start(String uriPrefix)
        {
            SocketLoopTokenSource = new CancellationTokenSource();
            ListenerLoopTokenSource = new CancellationTokenSource();
            Listener = new HttpListener();

/*

            Listener.Prefixes.Add("http://localhost:5005/");
            Listener.Prefixes.Add("http://localhost:5000/");*/
            Listener.Prefixes.Add(uriPrefix);
            Listener.Start();
            
            if (Listener.IsListening)
            {
                RaiseSocketStateChanged("Server listening: " + uriPrefix, LogTypes.Trace);
                Task.Run(() => ListenerProcessingLoopAsync().ConfigureAwait(false));
            }
            else
            {
                RaiseSocketStateChanged("Server failed to start.", LogTypes.Error);
            }
        }
        public static async Task StopAsync()
        {
            if (Listener?.IsListening ?? false && ServerIsRunning)
            {
                RaiseSocketStateChanged("\nServer is stopping.", LogTypes.Trace);

                ServerIsRunning = false;
                await CloseAllSocketsAsync();
                ListenerLoopTokenSource.Cancel();
                Listener.Stop();
                Listener.Close();
            }
        }
        public static void Broadcast(String message)
        {
            foreach (KeyValuePair<Int32, ConnectedClient> clientRef in Clients)
            {
                clientRef.Value.BroadcastQueue.Add(message);
            }
        }
        public static String GetConnectedClientList()
        {
            String retVal = "Connected Clients:\n";
            foreach (KeyValuePair<Int32, ConnectedClient> clientRef in Clients)
            {
                retVal += "Socket: " + clientRef.Value.SocketId + "\t" + clientRef.Value.Socket.State.ToString() + "\n";
            }
            return retVal;
        }

        private static async Task ListenerProcessingLoopAsync()
        {
            CancellationToken objCancellationToken = ListenerLoopTokenSource.Token;
            try
            {
                while (!objCancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await Listener.GetContextAsync();
                    if (ServerIsRunning)
                    {
                        if (context.Request.IsWebSocketRequest)
                        {
                            //HTTP is only the initial connection; upgrade to a client-specific websocket
                            HttpListenerWebSocketContext wsContext = null;
                            try
                            {
                                Int32 socketId = 0;
                                ConnectedClient objClient = null;

                                wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                                socketId = Interlocked.Increment(ref SocketCounter);
                                objClient = new ConnectedClient(socketId, wsContext.WebSocket);

                                Clients.TryAdd(socketId, objClient);
                                await Task.Run(() => SocketProcessingLoopAsync(objClient).ConfigureAwait(false));

                                RaiseSocketStateChanged("Socket " + socketId + ": connected", LogTypes.Trace);
                            }
                            catch (Exception)
                            {
                                //Server error if upgrade from HTTP to WebSocket fails
                                context.Response.StatusCode = 500;
                                context.Response.StatusDescription = "WebSocket upgrade failed";
                                context.Response.Close();
                                return;
                            }
                        }
                        else
                        {
                            if (context.Request.AcceptTypes.Contains("text/html"))
                            {
                                String html = "<!DOCTYPE html><meta charset=\"utf - 8\"/><title>Invalid Client</title><div>Invalid Client</div>";
                                ReadOnlyMemory<byte> HtmlPage = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(html));
                                context.Response.ContentType = "text/html; charset=utf-8";
                                context.Response.StatusCode = 200;
                                context.Response.StatusDescription = "OK";
                                context.Response.ContentLength64 = HtmlPage.Length;
                                await context.Response.OutputStream.WriteAsync(HtmlPage, CancellationToken.None);
                                await context.Response.OutputStream.FlushAsync(CancellationToken.None);

                                html = null;
                            }
                            else
                            {
                                context.Response.StatusCode = 400;
                            }
                            context.Response.Close();
                        }
                    }
                    else
                    {
                        //HTTP 409 Conflict (with server's current state)
                        context.Response.StatusCode = 409;
                        context.Response.StatusDescription = "Server is shutting down";
                        context.Response.Close();
                        return;
                    }
                }
            }
            catch (HttpListenerException ex) when (ServerIsRunning)
            {
                RaiseSocketStateChanged("Inbound socket connection error: " + ex.Message + "\n" + ex.StackTrace, LogTypes.Error);
            }
        }
        private static async Task SocketProcessingLoopAsync(ConnectedClient client)
        {
            WebSocket objSocket = client.Socket;
            CancellationToken objLoopToken = SocketLoopTokenSource.Token;
            CancellationTokenSource objBroadcastTokenSource = client.BroadcastLoopTokenSource;
            
            await Task.Run(() => client.BroadcastLoopAsync().ConfigureAwait(false));
            try
            {
                ArraySegment<Byte> arrBuffer = WebSocket.CreateServerBuffer(4096);
                while (objSocket.State != WebSocketState.Closed && objSocket.State != WebSocketState.Aborted && !objLoopToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult objReceiveResult = await client.Socket.ReceiveAsync(arrBuffer, objLoopToken);
                    if (!objLoopToken.IsCancellationRequested)
                    {
                        if (client.Socket.State == WebSocketState.CloseReceived && objReceiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            objBroadcastTokenSource.Cancel();
                            await objSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                        }
                        if (client.Socket.State == WebSocketState.Open)
                        {
                            SocketResponse objResponse = new SocketResponse(SocketFrameType.CommandResponse, 0, "OK", null);

                            if (objReceiveResult.MessageType == WebSocketMessageType.Text)
                            {
                                String payload = Encoding.UTF8.GetString(arrBuffer.Array, 0, objReceiveResult.Count);
                                SocketCommand objCmd = null;
                                
                                try
                                {
                                    objCmd = JsonConvert.DeserializeObject<SocketCommand>(payload);
                                }
                                catch(Exception ex){ }
                                if(objCmd != null)
                                {
                                    switch (objCmd.cmd)
                                    {
                                        case "ExecSync":
                                            Int64 ruleId = 0;
                                            if (objCmd.args != null && objCmd.args.Length == 1)
                                            {
                                                ruleId = GeneralHelpers.parseInt64(objCmd.args[0]);
                                            }
                                            if(ruleId > 0)
                                            {
                                                RaiseCLIEvent("ExecSync", ruleId);
                                            }
                                            else
                                            {
                                                objResponse.exitCode = 1;
                                                objResponse.message = "Argument not set: ruleId";
                                            }
                                            break;
                                        case "UpdateSchedules":
                                            RaiseCLIEvent("UpdateSchedules", 0);
                                            break;
                                        default:
                                            objResponse.exitCode = 1;
                                            objResponse.message = "InvalidCommand";
                                            break;
                                    }
                                }
                                else
                                {
                                    objResponse.exitCode = 1;
                                    objResponse.message = "InvalidCommand";
                                }

                                objCmd = null;
                                payload = null;
                            }
                            else
                            {
                                objResponse.exitCode = 1;
                                objResponse.message = "InvalidCommand";
                            }

                            client.BroadcastQueue.Add(JsonConvert.SerializeObject(objResponse));

                            objResponse = null;
                        }
                    }
                    objReceiveResult = null;
                }
                arrBuffer = null;
            }
            catch (OperationCanceledException) { } //Normal upon task/token cancellation...
            catch (Exception ex)
            {
                RaiseSocketStateChanged("Socket " + client.SocketId + ": " + ex.Message + "\n" + ex.StackTrace, LogTypes.Error);
            }
            finally
            {
                objBroadcastTokenSource.Cancel();

                RaiseSocketStateChanged("Socket " + client.SocketId + ": Ended processing loop in state " + objSocket.State, LogTypes.Trace);
                if (client.Socket.State != WebSocketState.Closed)
                {
                    client.Socket.Abort();
                }
                if (Clients.TryRemove(client.SocketId, out _))
                {
                    objSocket.Dispose();
                }
            }
        }
        private static async Task CloseAllSocketsAsync()
        {
            List<WebSocket> arrDisposeQueue = new List<WebSocket>(Clients.Count);

            while (Clients.Count > 0)
            {
                ConnectedClient objClient = Clients.ElementAt(0).Value;

                RaiseSocketStateChanged("Closing Socket " + objClient.SocketId, LogTypes.Trace);
                objClient.BroadcastLoopTokenSource.Cancel();
                if (objClient.Socket.State == WebSocketState.Open)
                {
                    var timeout = new CancellationTokenSource(Constants.CLOSE_SOCKET_TIMEOUT_MS);
                    try
                    {
                        await objClient.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
                    }
                    catch (OperationCanceledException ex) { } //Normal upon task/token cancellation...
                }
                if (Clients.TryRemove(objClient.SocketId, out _))
                {
                    arrDisposeQueue.Add(objClient.Socket);
                }
            }

            SocketLoopTokenSource.Cancel();

            foreach (var socket in arrDisposeQueue)
            {
                socket.Dispose();
            }

            arrDisposeQueue = null;
        }
    }
}