#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class Networking : IDisposable
    {
        private const string SERVER_URL = "localhost:8080";

        private static object lockObj = new object();
        private static Networking? instance = null;
        private readonly HttpClient httpClient;
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
            httpClient = new HttpClient() { BaseAddress = new Uri($"http://{SERVER_URL}") };
            webSocket = new ClientWebSocket();
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

        public void ConnectAndListen(CancellationToken cancellationToken)
        {
            Debug.Log("Networking.ConnectAndListen");

            Uri serverUri = new Uri($"ws://{SERVER_URL}/ws");

            _ = Task.Run(async () =>
            {
                var token = GlobalStorage.Instance.Token;
                if (token == null)
                {
                    Debug.LogError("Networking.ConnectAndListen: Unathorized");
                    return;
                }

                try
                {
                    webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
                    await webSocket.ConnectAsync(serverUri, cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }

                Debug.Log("WebSocket connection opened");

                while (webSocket.State == WebSocketState.Open)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WebSocketReceiveResult result;
                    List<string> chunks = new List<string>();
                    do
                    {
                        var chunk = new ArraySegment<byte>(new byte[1024]);
                        result = await webSocket.ReceiveAsync(chunk, cancellationToken);
                        string messagePartStr = Encoding.UTF8.GetString(chunk.Array, 0, result.Count);
                        chunks.Add(messagePartStr);
                        // Debug.Log($"Received message part: '{messagePartStr}'. Count: {result.Count}");
                    }
                    while (!result.EndOfMessage && webSocket.State == WebSocketState.Open);

                    string messageStr = string.Join(string.Empty, chunks);
                    try
                    {
                        ServerMessageDTO<object> message = JsonUtility.FromJson<ServerMessageDTO<object>>(messageStr);
                        Debug.Log($"Received message with a method: '{message.method}'; message: {messageStr}");

                        if (subscriptions.ContainsKey(message.method))
                        {
                            foreach (var sub in subscriptions[message.method])
                            {
                                // _ = Task.Run(async () => await sub(message.method, messageStr, cancellationToken)); // async/await can be skipped there, but I left them to signal, that sub is async fn
                                await sub(message.method, messageStr, cancellationToken);
                                // switched to one-threaded message hadling for now
                                // TODO: return back to multithreaded, probably Rx will be required
                                // Scenario failed in simple multithreaded version (one commented out above):
                                //   3 updates are sent from be: allCards, allCardInstances, matchStateUpdate
                                //   allCardInstances and matchStateUpdated are handled in parallel
                                //   => at a moment HandBehaviour.OnMatchStateUpdate executed, GlobalStorage.AllCardInstances are not ready yet
                                // I see, that Rx may help us - HandBehaviour.OnMatchStateUpdate will be run on both allCardInstances and matchStateUpdate updates from BE
                                // Maybe something like this can be done manually in case it would be difficult to add Rx lib
                            }
                        }
                        else
                        {
                            Debug.Log($"Received message with unsupported method: '{message.method}'; message: {messageStr}");
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"Received message in unknown format: {messageStr}");
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

        public async Task SendMessageAsync(string methodName, CancellationToken cancellationToken)
        {
            var message = new ServerMessageDTO
            {
                method = methodName,
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

        public async Task<R?> PostAsync<T, R>(string methodName, T body, CancellationToken cancellationToken)
        {
            var json = JsonUtility.ToJson(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(methodName, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string str = await response.Content.ReadAsStringAsync();
            var respDTO = JsonUtility.FromJson<R>(str);
            if (respDTO == null)
            {
                Debug.LogError($"Networking.PostAsync: [{methodName}]: received message in unknown format");
            }
            return respDTO;
        }
    }
}