#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviours
{
    public abstract class AWithMatchStateSubscribtionBehaviour : MonoBehaviour
    {
        protected List<Action> unsubscribers = new List<Action>();
        protected bool changesArePresent = false;
        public PlayerType playerType = PlayerType.Self;
        protected bool isEnabled = false;

        protected void Start()
        {
            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;
        }

        protected void OnDestroy()
        {
            foreach (var uss in unsubscribers)
            {
                uss();
            }
        }

        protected void Update()
        {
            if (!changesArePresent || !isEnabled) return;
            changesArePresent = false;

            VerifyFields();
            UpdateImpl();
        }

        protected void OnEnable()
        {
            isEnabled = true;
        }

        protected void OnDisable()
        {
            isEnabled = false;
        }

        protected abstract void VerifyFields();
        protected abstract void UpdateImpl();
        protected abstract Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, MatchStateDTO matchStateDTO, bool isPlayersTurn, CancellationToken cancellationToken);

        protected async Task OnMatchStateUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            if (!isEnabled) return;

            try
            {
                ServerMessageDTO<MatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<MatchStateDTO>>(message);

                (var playerState, bool isPlayersTurn) = playerType switch
                {
                    PlayerType.Self => (dto.body.player, dto.body.ownTurn),
                    PlayerType.Opponent => (dto.body.opponent, !dto.body.ownTurn),
                    _ => throw new InvalidOperationException($"Unsupported {nameof(playerType)}: '{playerType}'")
                }; 

                await OnMatchStateUpdateAsync(playerState, dto.body, isPlayersTurn, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            changesArePresent = true;
        }
    }
}
