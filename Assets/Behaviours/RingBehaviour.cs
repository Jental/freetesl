#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class RingBehaviour : AWithMatchStateAndInformationSubscribtionBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image? imageGameObject;
        [SerializeField] private Image? activeBackgroundGameObject;
        [SerializeField] private Sprite? ringWith3GemsImage;
        [SerializeField] private Sprite? ringWith2GemsImage;
        [SerializeField] private Sprite? ringWith1GemsImage;
        [SerializeField] private Sprite? ringWith0GemsImage;

        private bool hasRing = false;
        private bool isRingActive = false;
        private int gemCount = 0;

        protected override Task OnMatchInformationUpdateAsync(MatchInformationDTO dto, CancellationToken cancellationToken)
        {
            hasRing = playerType == Enums.PlayerType.Self ? dto.hasRing : dto.opponentHasRing;
            return Task.CompletedTask;
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, MatchStateDTO matchStateDTO, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            this.gemCount = dto.ringGemCount;
            this.isRingActive = dto.isRingActive;
            return Task.CompletedTask;
        }

        protected override void UpdateImpl()
        {
            imageGameObject!.gameObject.SetActive(hasRing);
            activeBackgroundGameObject!.gameObject.SetActive(hasRing);

            if (hasRing)
            {
                imageGameObject!.sprite = gemCount switch
                {
                    3 => ringWith3GemsImage!,
                    2 => ringWith2GemsImage!,
                    1 => ringWith1GemsImage!,
                    0 => ringWith0GemsImage!,
                    _ => throw new InvalidOperationException("Imvalid number of ring gems")
                };

                activeBackgroundGameObject!.gameObject.SetActive(playerType == Enums.PlayerType.Self && isRingActive);
            }
        }

        protected override void VerifyFields()
        {
            if (this.imageGameObject == null) throw new InvalidOperationException($"{nameof(imageGameObject)} gameObject is expected to be set");
            if (this.activeBackgroundGameObject == null) throw new InvalidOperationException($"{nameof(activeBackgroundGameObject)} gameObject is expected to be set");
            if (this.ringWith3GemsImage == null) throw new InvalidOperationException($"{nameof(ringWith3GemsImage)} spite is expected to be set");
            if (this.ringWith2GemsImage == null) throw new InvalidOperationException($"{nameof(ringWith2GemsImage)} spite is expected to be set");
            if (this.ringWith1GemsImage == null) throw new InvalidOperationException($"{nameof(ringWith1GemsImage)} spite is expected to be set");
            if (this.ringWith0GemsImage == null) throw new InvalidOperationException($"{nameof(ringWith0GemsImage)} spite is expected to be set");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _ = Task.Run(async () =>
            {
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.USE_RING, destroyCancellationToken);
            });
        }
    }
}
