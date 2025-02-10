#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviours
{
    public abstract class AWithMatchStateAndInformationSubscribtionBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        protected new void Start()
        {
            base.Start();

            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_INFORMATION_UPDATE, OnMatchUnformationUpdateAsync);
            unsubscribers.Add(uss);
        }

        protected abstract Task OnMatchUnformationUpdateAsync(MatchInformationDTO dto, CancellationToken cancellationToken);

        protected async Task OnMatchUnformationUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            ServerMessageDTO<MatchInformationDTO> dto = JsonUtility.FromJson<ServerMessageDTO<MatchInformationDTO>>(message);

            try
            {
                await OnMatchUnformationUpdateAsync(dto.body, cancellationToken);
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
