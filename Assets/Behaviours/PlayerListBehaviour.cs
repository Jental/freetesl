#nullable enable

using Assets.Behaviours;
using Assets.Common;
using Assets.DTO;
using Assets.Enums;
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
        private bool changesArePresent = false;

        protected void Start()
        {
            if (this.ContentGameObject == null) throw new InvalidOperationException($"{nameof(ContentGameObject)} game object is expected to be set");
            if (this.ListItemPrefab == null) throw new InvalidOperationException($"{nameof(ListItemPrefab)} prefab is expected to be set");

            _ = destroyCancellationToken;

            _ = Task.Run(async () => { await LoadPlayersAsync(destroyCancellationToken); });
        }

        protected void Update()
        {
            if (!changesArePresent) { return; }
            changesArePresent = false;

            var children = ContentGameObject!.transform.GetComponentsInChildren<PlayerListItemBehaviour>();
            foreach (var dc in children)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            foreach (var player in playersToShow)
            {
                var dc =
                    Instantiate(ListItemPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                    ?? throw new InvalidOperationException("Failed to instantiate a player list item prefab");
                dc.transform.parent = ContentGameObject.transform;
                dc.player = player;
                dc.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }

        private async Task LoadPlayersAsync(CancellationToken cancellationToken)
        {
            ListDTO<PlayerInformationDTO>? dtos;
            try
            {
                dtos = await Networking.Instance.GetAsync<ListDTO<PlayerInformationDTO>>("/players", new Dictionary<string, string>(), cancellationToken);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return;
            }

            if (dtos?.items == null)
            {
                Debug.LogError("Player list is null");
                return;
            }

            playersToShow = dtos.items.Select(GeneralMappers.MapFromPlayerInformationDTO).ToArray();
            changesArePresent = true;
        }
    }
}