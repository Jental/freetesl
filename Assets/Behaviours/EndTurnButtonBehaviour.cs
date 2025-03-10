#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class EndTurnButtonBehaviour : AWithMatchStateSubscribtionBehaviour, IPointerClickHandler
    {
        public GameObject? ActivePlayButtonPrefab = null;
        public GameObject? InactivePlayButtonPrefab = null;
        public CanvasService? CanvasService = null;

        private bool isPlayersTurn = false;
        private bool hasTurnChangedSinceLastUpdate = false;

        protected void OnDisable()
        {
            isPlayersTurn = false;
            hasTurnChangedSinceLastUpdate = false;
            changesArePresent = true;
        }

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (!isPlayersTurn) { return; }

            isPlayersTurn = false;

            await Networking.Instance.SendMessageAsync(Constants.MethodNames.END_TURN, destroyCancellationToken);
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            this.hasTurnChangedSinceLastUpdate = this.hasTurnChangedSinceLastUpdate || this.isPlayersTurn != isPlayersTurn;
            this.isPlayersTurn = isPlayersTurn;
            return Task.CompletedTask;
        }

        protected override void UpdateImpl()
        {
            var currentButtonImages = gameObject.transform.GetComponentsInChildren<RawImage>();
            foreach (var dc in currentButtonImages)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            var playButtonPrefab = isPlayersTurn && playerType == PlayerType.Self ? ActivePlayButtonPrefab : InactivePlayButtonPrefab;
            var playButton = Instantiate(playButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            playButton!.transform.parent = gameObject.transform;

            var playButtonRect = playButton.GetComponent<RectTransform>();
            playButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            playButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            playButtonRect.anchoredPosition = new Vector2(7.0f, 0.0f);
            playButtonRect.sizeDelta = new Vector2(50.0f, 50.0f);
            playButtonRect.localScale = new Vector3(1, 1, 1);

            if (hasTurnChangedSinceLastUpdate) {
                hasTurnChangedSinceLastUpdate = false;
                if (isPlayersTurn && playerType == PlayerType.Self)
                {
                    CanvasService!.ShowNotification("YOUR TURN");
                }
            }
        }

        protected override void VerifyFields()
        {
            if (this.ActivePlayButtonPrefab == null) throw new InvalidOperationException($"{nameof(ActivePlayButtonPrefab)} prefab is expected to be set");
            if (this.InactivePlayButtonPrefab == null) throw new InvalidOperationException($"{nameof(InactivePlayButtonPrefab)} prefab is expected to be set");
            if (this.CanvasService == null) throw new InvalidOperationException($"{nameof(CanvasService)} game object is expected to be set");
        }
    }

}