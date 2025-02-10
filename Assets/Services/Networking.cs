#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class Networking : IDisposable
    {
        private static object lockObj = new object();
        private static Networking? instance = null;
        private readonly ClientWebSocket webSocket;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool disposedValue;

        /// <summary>
        /// Subscriptions to received messages.
        /// key - method of a message
        /// value - list of handlers
        /// handler - async function with methodName, message and cancellationToken parameters
        /// </summary>
        private Dictionary<string, List<Func<string, string, CancellationToken, Task>>> subscriptions = new Dictionary<string, List<Func<string, string, CancellationToken, Task>>>();

        public static Networking Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new Networking();
                    }
                }
                return instance;
            }
        }

        private Networking()
        {
            webSocket = new ClientWebSocket();
            ConnectAndListen(cancellationTokenSource.Token);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancellationTokenSource.Cancel();
                    webSocket.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Subscribes to methods
        /// </summary>
        /// <param name="methods">method field of a message</param>
        /// <param name="handler">async function with methodName, message and cancellationToken parameters</param>
        /// <returns>unsubscriber</returns>
        public Action Subscribe(string[] methods, Func<string, string, CancellationToken, Task> handler)
        {
            foreach (var method in methods)
            {
                if (!subscriptions.ContainsKey(method))
                {
                    subscriptions.Add(method, new List<Func<string, string, CancellationToken, Task>>());
                }
                subscriptions[method].Add(handler);
            }

            return () =>
            {
                foreach (var method in methods)
                {
                    subscriptions[method].Remove(handler);
                }
            };
        }

        /// <summary>
        /// Subscribes to methods
        /// </summary>
        /// <param name="methods">method field of a message</param>
        /// <param name="handler">async function with methodName, message and cancellationToken parameters</param>
        /// <returns>unsubscriber</returns>
        public Action Subscribe(string method, Func<string, string, CancellationToken, Task> handler)
        {
            return Subscribe(new string[1] { method }, handler);
        }

        private void ConnectAndListen(CancellationToken cancellationToken)
        {
            Uri serverUri = new Uri("ws://localhost:8080/ws");

            _ = Task.Run(async () =>
            {
                webSocket.ConnectAsync(serverUri, cancellationToken).Wait();
                Debug.Log("WebSocket connection opened");

                while (webSocket.State == WebSocketState.Open)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WebSocketReceiveResult result;
                    List<ArraySegment<byte>> chunks = new List<ArraySegment<byte>>();
                    int totalCount = 0;
                    do
                    {
                        var chunk = new ArraySegment<byte>(new byte[1024]);
                        result = await webSocket.ReceiveAsync(chunk, cancellationToken);
                        chunks.Add(chunk);
                        totalCount = totalCount + result.Count;
                    }
                    while (!result.EndOfMessage && webSocket.State == WebSocketState.Open);

                    var messageBytes = chunks.SelectMany(c => c).ToArray();
                    string messageStr = Encoding.UTF8.GetString(messageBytes, 0, totalCount);
                    try
                    {
                        ServerMessageDTO<object> message = JsonUtility.FromJson<ServerMessageDTO<object>>(messageStr);
                        Debug.Log($"Received message with a method: '{message.method}'; message: {messageStr}");

                        if (subscriptions.ContainsKey(message.method))
                        {
                            foreach (var sub in subscriptions[message.method])
                            {
                                _ = Task.Run(async () => await sub(message.method, messageStr, cancellationToken)); // async/await can be skipped there, but I left them to signal, that sub is async fn
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Received message with unsupported method: '{message.method}'; message: {messageStr}");
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Received message in unknown format: {messageStr}");
                    }
                }

                Debug.Log("Finished");
            });
        }

        public async Task SendMessageAsync<T>(string methodName, T body, CancellationToken cancellationToken)
        {
            var message = new ServerMessageDTO<T>
            {
                method = methodName,
                body = body,
            };
            string json = JsonUtility.ToJson(message);
            ArraySegment<byte> sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            try
            {
                // There should be a better way to do it with ManualResetEvent or TaskCompletionSource
                while (webSocket.State != WebSocketState.Open)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(500);
                }
                await webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cancellationToken);
                Debug.Log($"Sent message with a method: '{methodName}': {json}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}