#nullable enable

using Assets.Common;
using Assets.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class MatchService : MonoBehaviour
    {
        [SerializeField] private CanvasService canvasService;

        private Action? unsubscriber;

        protected void Start()
        {
            _ = destroyCancellationToken;
            GlobalStorage.Instance.Init();
            Networking.Instance.ConnectAndListen(destroyCancellationToken);

            unsubscriber = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_END, OnMatchEnd);
        }

        private Task OnMatchEnd(string methodName, string message, CancellationToken cancellationToken)
        {
            Debug.Log("MatchService.OnMatchEnd");
            try
            {
                ServerMessageDTO<MatchEndDTO> dto = JsonUtility.FromJson<ServerMessageDTO<MatchEndDTO>>(message);
                canvasService.MatchEndHasWon = dto.body.hasWon;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return Task.CompletedTask;
            }

            
            canvasService.ActiveCanvas = Enums.AppCanvas.MatchEnd;

            unsubscriber?.Invoke();

            return Task.CompletedTask;
        }
    }
}
