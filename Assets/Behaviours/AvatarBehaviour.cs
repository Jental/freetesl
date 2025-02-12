#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Models;
using Assets.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class AvatarBehaviour : AWithMatchStateAndInformationSubscribtionBehaviour, IDropHandler
    {
        public TextMeshProUGUI? healthText = null;
        public GameObject? healthSectorPrefab = null;
        public GameObject? healthBarGameObject = null;
        public GameObject? runesGameObject = null;
        public RawImage? imageGameObject = null;

        private const float HEALTH_START_ANGLE = 152.25f;
        private const float HEALTH_STEP_ANGLE = 10.5f;

        private int health = 0;
        private byte runes = 0;
        private string? imageName = null;

        protected override void UpdateImpl()
        {
            this.healthText!.text = health.ToString();

            var currentHealthSectors = healthBarGameObject!.transform.GetComponentsInChildren<RawImage>();
            foreach (var dc in currentHealthSectors)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            for (int i = 0; i < health; i++)
            {
                var angle = HEALTH_START_ANGLE - i * HEALTH_STEP_ANGLE;
                var sector = Instantiate(healthSectorPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, angle)));
                sector!.transform.parent = healthBarGameObject.transform;

                var sectorRect = sector.GetComponent<RectTransform>();
                sectorRect.anchorMin = new Vector2(0, 0);
                sectorRect.anchorMax = new Vector2(1, 1);
                sectorRect.offsetMin = new Vector2(0, 0);
                sectorRect.offsetMax = new Vector2(0, 0);
                sectorRect.localScale = new Vector3(1, 1, 1);
            }

            var runeObjects = runesGameObject!.transform.GetComponentsInChildren<RawImage>();
            for (byte i = 0; i < runeObjects.Length; i++)
            {
                runeObjects[i].gameObject.SetActive(i < runes);
            }

            string imageNameNotNull = imageName ?? "DBH_NPC_CRDL_02_022_avatar_png";
            this.imageGameObject!.texture = Resources.Load<Texture>($"Player/{imageNameNotNull}");
        }

        protected override void VerifyFields()
        {
            if (this.healthText == null) throw new InvalidOperationException($"{nameof(healthText)} gameObject is expected to be set");
            if (this.healthSectorPrefab == null) throw new InvalidOperationException($"{nameof(healthSectorPrefab)} prefab is expected to be set");
            if (this.healthBarGameObject == null) throw new InvalidOperationException($"{nameof(healthBarGameObject)} gameObject is expected to be set");
            if (this.runesGameObject == null) throw new InvalidOperationException($"{nameof(runesGameObject)} gameObject is expected to be set");
            if (this.imageGameObject == null) throw new InvalidOperationException($"{nameof(imageGameObject)} gameObject is expected to be set");
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            this.health = dto.health;
            this.runes = dto.runes;

            return Task.CompletedTask;
        }

        protected override Task OnMatchUnformationUpdateAsync(MatchInformationDTO dto, CancellationToken cancellationToken)
        {
            this.imageName = playerType switch
            {
                PlayerType.Self => dto.player.avatarName,
                PlayerType.Opponent => dto.opponent.avatarName,
                _ => throw new InvalidOperationException($"Unsupported {nameof(playerType)}: '{playerType}'")
            };

            return Task.CompletedTask;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("AvatarBehaviour.OnDrop");

            var dropped = eventData.pointerDrag;
            var displayCard = dropped.GetComponent<CardBehaviour>();
            var cardInstance =
                displayCard.cardInstance
                ?? throw new InvalidOperationException($"{displayCard.cardInstance} property of a dropped item is expected to be set");

            var currentParentLaneCardsComponent = displayCard.gameObject.GetComponentInParent<LaneCardsBehaviour>();
            if (currentParentLaneCardsComponent == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                var dto = new HitFaceDTO
                {
                    playerID = Constants.TEST_PLAYER_ID,
                    cardInstanceID = cardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.HIT_FACE, dto, destroyCancellationToken);
            });
        }
    }
}
