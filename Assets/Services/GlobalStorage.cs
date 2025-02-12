#nullable enable

using Assets.Common;
using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class GlobalStorage : IDisposable
    {
        private static object lockObj = new object();
        private static GlobalStorage instance = new GlobalStorage();

        private List<Action> unsubscribers = new List<Action>();
        private bool disposedValue;

        private PlayerMatchStateDTO? playerMatchStateDTO = null;

        private GlobalStorage() {
            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);
        }

        public static GlobalStorage Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new GlobalStorage();
                    }
                }
                return instance;
            }
        }

        public PlayerMatchStateDTO PlayerMatchStateDTO
        {
            get { return playerMatchStateDTO ?? throw new InvalidOperationException($"Expected not null {nameof(playerMatchStateDTO)}"); }
            set { this.playerMatchStateDTO = value; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var uss in unsubscribers)
                    {
                        uss();
                    }
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

        private Task OnMatchStateUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            try
            {
                ServerMessageDTO<MatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<MatchStateDTO>>(message);

                if (dto.body?.player != null)
                {
                    this.PlayerMatchStateDTO = dto.body.player;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return Task.CompletedTask;
        }
    }
}
