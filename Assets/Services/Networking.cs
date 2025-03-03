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
using System.Web;
using UnityEngine;

namespace Assets.Services
{
    public class Networking : IDisposable
    {
        private static object lockObj = new object();
        private static Networking? instance = null;
        private HttpClient? httpClient;
        private string? relativeUrlPrefix = null;
        private readonly ClientWebSocket webSocket;
        private Uri? websocketUrl;
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
        }

        public void Init(string serverUrl)
        {
            int firstSlashIdx = serverUrl.IndexOf("/");
            string baseUrl = firstSlashIdx < 0 ? serverUrl : serverUrl.Substring(0, firstSlashIdx);
            if (firstSlashIdx >= 0 && firstSlashIdx != serverUrl.Length - 1)
            {
                relativeUrlPrefix = serverUrl.Substring(firstSlashIdx, serverUrl.Length - firstSlashIdx);
                if (!relativeUrlPrefix.EndsWith("/"))
                {
                    relativeUrlPrefix = $"{relativeUrlPrefix}/";
                }
            }
            Debug.Log($"Networking.Init: baserUrl: {baseUrl}; prefix: {relativeUrlPrefix}");

            httpClient = new HttpClient() { BaseAddress = new Uri($"http://{baseUrl}") };
            websocketUrl = new Uri($"ws://{serverUrl}/ws");
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
                    await webSocket.ConnectAsync(websocketUrl, cancellationToken);
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
                            foreach (var sub in subscriptions[message.method].Select(s => s).ToArray()) // cloning - for ability to unsubscribe inside handlers
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
                    catch (Exception e)
                    {
                        Debug.LogError($"Received message in unknown format: '{messageStr}'. Or some other error: {e}");
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

        public async Task<R?> GetAsync<R>(string relativeUrl, Dictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            if (httpClient == null) throw new InvalidOperationException("Networking is not initialized");

            var fullUrl = relativeUrlPrefix == null ? relativeUrl : $"{relativeUrlPrefix}{relativeUrl}";
            if (parameters.Count > 0)
            {
                string parametersStr = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
                fullUrl = $"{relativeUrl}?{parametersStr}";
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalStorage.Instance.Token);

            var response = await httpClient.GetAsync(fullUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string str = await response.Content.ReadAsStringAsync();
            Debug.Log($"Networking.GetAsync: [{relativeUrl}]: resp: {str}");
            var respDTO = JsonUtility.FromJson<R>(str);
            if (respDTO == null)
            {
                Debug.LogError($"Networking.GetAsync: [{relativeUrl}]: received message in unknown format");
            }
            return respDTO;
        }

        public async Task<R?> PostAsync<T, R>(string relativeUrl, T body, CancellationToken cancellationToken)
        {
            if (httpClient == null) throw new InvalidOperationException("Networking is not initialized");

            var json = JsonUtility.ToJson(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalStorage.Instance.Token);

            var fullUrl = relativeUrlPrefix == null ? relativeUrl : $"{relativeUrlPrefix}{relativeUrl}";
            var response = await httpClient.PostAsync(fullUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string str = await response.Content.ReadAsStringAsync();
            Debug.Log($"Networking.PostAsync: [{relativeUrl}]: resp: {str}");
            var respDTO = JsonUtility.FromJson<R>(str);
            if (respDTO == null)
            {
                Debug.LogError($"Networking.PostAsync: [{relativeUrl}]: received message in unknown format");
            }
            return respDTO;
        }

        public async Task PostAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            if (httpClient == null) throw new InvalidOperationException("Networking is not initialized");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalStorage.Instance.Token);

            var fullUrl = relativeUrlPrefix == null ? relativeUrl : $"{relativeUrlPrefix}{relativeUrl}";
            var response = await httpClient.PostAsync(fullUrl, null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}