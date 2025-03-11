#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Mappers;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviours
{
    public class PlayerListBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? ContentGameObject = null;
        [SerializeField] private PlayerListItemBehaviour? ListItemPrefab = null;

        private Player[] playersToShow = Array.Empty<Player>();
        private int? selectedPlayerIdx;

        private List<PlayerListItemBehaviour> itemGameObjects = new List<PlayerListItemBehaviour>();
        private bool beChangesArePresent = false;
        private bool changesArePresent = false;
        private Timer? playersPollingTimer = null;

        protected void Start()
        {
            if (this.ContentGameObject == null) throw new InvalidOperationException($"{nameof(ContentGameObject)} game object is expected to be set");
            if (this.ListItemPrefab == null) throw new InvalidOperationException($"{nameof(ListItemPrefab)} prefab is expected to be set");

            _ = destroyCancellationToken;
        }

        protected void OnEnable()
        {
            Debug.Log("PlayerListBehaviour.OnEnable");
            if (Application.isPlaying)
            {
                StartPlayerListPolling(destroyCancellationToken);
            }
        }

        protected void OnDisable()
        {
            Debug.Log("PlayerListBehaviour.OnDisable");
            selectedPlayerIdx = null;
            StopPlayerListPolling().Wait();
        }

        protected void Update()
        {
            if (beChangesArePresent)
            {
                beChangesArePresent = false;

                var children = ContentGameObject!.transform.GetComponentsInChildren<PlayerListItemBehaviour>();
                foreach (var dc in children)
                {
                    Destroy(dc.gameObject);
                    Destroy(dc);
                }
                itemGameObjects.Clear();

                for (int i = 0; i < playersToShow.Length; i++)
                {
                    var player = playersToShow[i];
                    var dc =
                        Instantiate(ListItemPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a player list item prefab");
                    dc.transform.parent = ContentGameObject.transform;
                    dc.Player = player;
                    var savedIdx = i;
                    dc.OnClick = () => OnItemClick(savedIdx);
                    dc.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    itemGameObjects.Add(dc);
                }
            }

            if (changesArePresent)
            {
                for (int i = 0; i < itemGameObjects.Count; i++)
                {
                    var igo = itemGameObjects[i];
                    igo.IsSelected = i == selectedPlayerIdx;
                }
            }
        }

        public Player? SelectedPlayer =>
            selectedPlayerIdx == null || selectedPlayerIdx >= playersToShow.Length 
            ? null
            : playersToShow[selectedPlayerIdx.Value];

        public void DeselectPlayer()
        {
            selectedPlayerIdx = null;
        }

        private void StartPlayerListPolling(CancellationToken cancellationToken)
        {
            playersPollingTimer = new Timer(
                async (_) => { await LoadPlayersAsync(cancellationToken); },
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(Constants.BACKEND_POLLING_INTERVAL)
            );
        }

        private async Task StopPlayerListPolling()
        {
            if (playersPollingTimer != null)
            {
                playersPollingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                await playersPollingTimer.DisposeAsync();
                playersPollingTimer = null;
            }
        }

        private async Task LoadPlayersAsync(CancellationToken cancellationToken)
        {
            ListDTO<PlayerInformationDTO>? dtos;
            try
            {
                dtos = await Networking.Instance.GetAsync<ListDTO<PlayerInformationDTO>>(
                    Constants.MethodNames.GET_PLAYERS,
                    new Dictionary<string, string>() { { "inGame", "true" } },
                    cancellationToken
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            if (dtos?.items == null)
            {
                Debug.LogError("Player list is null");
                return;
            }

            var newPlayersToShow = dtos.items.Select(GeneralMappers.MapFromPlayerInformationDTO).ToArray();
            beChangesArePresent = !playersToShow.SequenceEqual(newPlayersToShow) || playersToShow.Length != itemGameObjects.Count; // for case of unsync happened
            playersToShow = newPlayersToShow;
        }

        private void OnItemClick(int idx)
        {
            Debug.Log($"PlayerListBehaviour: OnItemClick: {idx}");
            selectedPlayerIdx =
                idx == selectedPlayerIdx
                ? null
                : idx;
            changesArePresent = true;
        }
    }
}