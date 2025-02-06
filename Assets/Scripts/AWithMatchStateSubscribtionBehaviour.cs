#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class AWithMatchStateSubscribtionBehaviour : MonoBehaviour
    {
        private List<Action> unsubscribers = new List<Action>();
        private bool changesArePresent = false;

        void Start()
        {
            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;
        }

        void Update()
        {
            if (!changesArePresent) return;

            VerifyFields();
            UpdateImpl();

            changesArePresent = false;
        }

        protected abstract void VerifyFields();
        protected abstract void UpdateImpl();
        protected abstract Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, CancellationToken cancellationToken);

        private async Task OnMatchStateUpdateAsync(string message, CancellationToken cancellationToken)
        {
            ServerMessageDTO<PlayerMatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<PlayerMatchStateDTO>>(message);
            Debug.Log($"AvatarBehaviour.OnMatchStateUpdateAsync: Health and runes: {dto.body.health}, {dto.body.runes}");

            await OnMatchStateUpdateAsync(dto.body, cancellationToken);

            changesArePresent = true;
        }
    }
}
