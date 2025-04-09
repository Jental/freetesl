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
    public class PlayerListBehaviour : AListBehaviour<Player, PlayerListItemBehaviour>
    {
        private Timer? playersPollingTimer = null;

        protected new void Start()
        {
            base.Start();

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

        protected new void OnDisable()
        {
            base.OnDisable();
            StopPlayerListPolling().Wait();
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

            ModelsToShow = dtos.items.Select(GeneralMappers.MapFromPlayerInformationDTO).ToArray();
        }
    }
}