#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class MatchService : MonoBehaviour
    {
        private const string MATCH_STATE_UPDATE_METHOD_NAME = "matchStateUpdate";
        private const string MATCH_JOIN_METHOD_NAME = "join";

        private List<Action> unsubscribers = new List<Action>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var uss = Networking.Instance.Subscribe(MATCH_STATE_UPDATE_METHOD_NAME, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;

            var joinDTO = new JoinMatchDTO { playerID = 1 };
            _ = Task.Run(async () => await JoinMatchAsync());
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            foreach (var uss in unsubscribers)
            {
                uss.Invoke();
            }
        }

        private async Task JoinMatchAsync()
        {
            var joinDTO = new JoinMatchDTO { playerID = 1 };
            await Networking.Instance.SendMessageAsync(MATCH_JOIN_METHOD_NAME, joinDTO, destroyCancellationToken);
        }

        private async Task OnMatchStateUpdateAsync(string message, CancellationToken cancellationToken)
        {
            ServerMessageDTO<PlayerMatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<PlayerMatchStateDTO>>(message);
            Debug.Log($"Counts: {dto.body.deck.Length}, {dto.body.hand.Length}");
        }
    }
}
