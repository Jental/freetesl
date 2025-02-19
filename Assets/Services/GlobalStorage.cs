#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Mappers;
using Assets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<int, CardScriptableObject>? allCardScriptableObjects = null;
        private Dictionary<int, Card> allCards = new Dictionary<int, Card>();
        private Dictionary<Guid, CardInstance> allCardInstances = new Dictionary<Guid, CardInstance>();

        public string? Token { get; set; } = null;

        private GlobalStorage() {
            unsubscribers.Add(
                Networking.Instance.Subscribe(Constants.MethodNames.ALL_CARDS_UPDATE, OnCardsUpdateAsync)
            );
            unsubscribers.Add(
                Networking.Instance.Subscribe(Constants.MethodNames.ALL_CARD_INSTANCES_UPDATE, OnCardInstancesUpdateAsync)
            );
            unsubscribers.Add(
                Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync)
            );
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

        public void Init()
        {
            Debug.Log("GlobalStorage.Init");
            allCardScriptableObjects = Resources.LoadAll<CardScriptableObject>("CardObjects").ToDictionary(c => c.id, c => c);
        }

        public PlayerMatchStateDTO PlayerMatchStateDTO =>
            playerMatchStateDTO ?? throw new InvalidOperationException($"Expected not null {nameof(playerMatchStateDTO)}");

        public Dictionary<Guid, CardInstance> AllCardInstances => allCardInstances;

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
                    this.playerMatchStateDTO = dto.body.player;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return Task.CompletedTask;
        }

        private Task OnCardInstancesUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            Debug.Log("GlobalStorage.OnCardInstancesUpdateAsync");
            try
            {
                ServerMessageDTO<CardInstanceDTO[]> dto = JsonUtility.FromJson<ServerMessageDTO<CardInstanceDTO[]>>(message);

                if (dto.body != null)
                {
                    // TODO: it would be better to update existing instances if they are present
                    this.allCardInstances =
                        dto.body
                        .Select(ciDto =>
                            MatchMappers.MapFromCardInstanceDTO(
                                ciDto,
                                this.allCardInstances.GetValueOrDefault(ciDto.CardInstanceGuid),
                                this.allCards
                            )
                        )
                        .ToDictionary(ci => ci.ID, ci => ci);

                    Debug.Log("GlobalStorage.OnCardInstancesUpdateAsync: Loaded");
                }
            }
            catch (Exception e)
            {
                Debug.Log("GlobalStorage.OnCardInstancesUpdateAsync: Error");
                Debug.LogException(e);
            }

            return Task.CompletedTask;
        }

        private Task OnCardsUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            Debug.Log("GlobalStorage.OnCardsUpdateAsync");
            try
            {
                ServerMessageDTO<CardDTO[]> dto = JsonUtility.FromJson<ServerMessageDTO<CardDTO[]>>(message);

                if (dto.body != null)
                {
                    // TODO: it would be better to update existing cards if they are present
                    this.allCards =
                        dto.body
                        .Select(cDto =>
                            MatchMappers.MapFromCardDTO(
                                cDto,
                                allCardScriptableObjects!
                            )
                        )
                        .ToDictionary(c => c.ID, c => c);

                    Debug.Log("GlobalStorage.OnCardsUpdateAsync: Loaded");

                    // TODO: it would be a good idea to update cared instances here to actualize card links in them
                    // but it should not happen, as cards are loaded once on a game start and instances are loaded on a match start
                    // See also a big comment in Networking
                }
            }
            catch (Exception e)
            {
                Debug.Log("GlobalStorage.OnCardsUpdateAsync: Error");
                Debug.LogException(e);
            }

            return Task.CompletedTask;
        }
    }
}
