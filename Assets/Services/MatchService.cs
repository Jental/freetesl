#nullable enable

using Assets.Common;
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
            canvasService.ActiveCanvas = Enums.AppCanvas.JoinMatch;

            unsubscriber?.Invoke();

            return Task.CompletedTask;
        }
    }
}
