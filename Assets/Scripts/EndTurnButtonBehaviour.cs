#nullable enable

using Assets.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class EndTurnButtonBehaviour : AWithMatchStateSubscribtionBehaviour, IPointerClickHandler
    {
        public GameObject? ActivePlayButtonPrefab = null;
        public GameObject? InactivePlayButtonPrefab = null;

        private bool ownTurn = false;

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (!ownTurn) { return; }

            ownTurn = false;

            var dto = new EndTurnDTO { playerID = 1 };
            await Networking.Instance.SendMessageAsync(Constants.MethodNames.END_TURN, dto, destroyCancellationToken);
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, CancellationToken cancellationToken)
        {
            this.ownTurn = dto.ownTurn;
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

            var playButtonPrefab = ownTurn ? ActivePlayButtonPrefab : InactivePlayButtonPrefab;
            var playButton = Instantiate(playButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            playButton!.transform.parent = gameObject.transform;

            var playButtonRect = playButton.GetComponent<RectTransform>();
            playButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            playButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            playButtonRect.anchoredPosition = new Vector2(2.5f, 0.0f);
            playButtonRect.sizeDelta = new Vector2(18.765f, 20.85f);
            playButtonRect.localScale = new Vector3(1, 1, 1);
        }

        protected override void VerifyFields()
        {
            if (this.ActivePlayButtonPrefab == null) throw new InvalidOperationException($"{nameof(ActivePlayButtonPrefab)} prefab is expected to be set");
            if (this.InactivePlayButtonPrefab == null) throw new InvalidOperationException($"{nameof(InactivePlayButtonPrefab)} prefab is expected to be set");
        }
    }

}