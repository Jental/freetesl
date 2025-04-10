#nullable enable

using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private CardDragAndDropService? cardDragAndDropService;

        private const float HEALTH_START_ANGLE = 152.25f;
        private const float HEALTH_STEP_ANGLE = 10.5f;

        private int health = 0;
        private byte runes = 0;
        private string? imageName = null;

        protected new void Start()
        {
            base.Start();

            cardDragAndDropService = GameObject.Find("CardDragAndDropService")?.GetComponent<CardDragAndDropService>() ?? throw new InvalidOperationException($"CardDragAndDropService gameObject is not found");

            _ = destroyCancellationToken;
        }

        protected new void OnDisable()
        {
            base.OnDisable();
            health = 0;
            runes = 0;
            imageName = null;
            changesArePresent = true;
        }

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

            var runeObjects = runesGameObject!.GetComponentsInChildren<Image>().Select(img => img.gameObject).Where(rune => rune != runesGameObject).ToArray();
            var glyphImages = new List<RawImage>();
            foreach (var rune in runeObjects)
            {
                foreach (Transform t in rune.transform)
                {
                    var rimg = t.gameObject.GetComponent<RawImage>();
                    glyphImages.Add(rimg);
                }
            }
            if (runeObjects.Length > 0)
            {
                var g = runeObjects[0].GetComponentInChildren<RawImage>();
                foreach (Transform t in runeObjects[0].transform) {
                    var g1 = t.gameObject;
                    var ri1 = g1.GetComponent<RawImage>();
                }
            }
            Debug.Log($"Updating runes: {glyphImages.Count}");
            for (byte i = 0; i < glyphImages.Count; i++)
            {
                glyphImages[i].gameObject.SetActive(i < runes);
            }

            string imageNameNotNull = imageName ?? "DBH_NPC_CRDL_02_022_avatar_png";
            this.imageGameObject!.texture = Resources.Load<Texture>($"Avatars/{imageNameNotNull}");
        }

        protected override void VerifyFields()
        {
            if (this.healthText == null) throw new InvalidOperationException($"{nameof(healthText)} gameObject is expected to be set");
            if (this.healthSectorPrefab == null) throw new InvalidOperationException($"{nameof(healthSectorPrefab)} prefab is expected to be set");
            if (this.healthBarGameObject == null) throw new InvalidOperationException($"{nameof(healthBarGameObject)} gameObject is expected to be set");
            if (this.runesGameObject == null) throw new InvalidOperationException($"{nameof(runesGameObject)} gameObject is expected to be set");
            if (this.imageGameObject == null) throw new InvalidOperationException($"{nameof(imageGameObject)} gameObject is expected to be set");
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, MatchStateDTO _, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            this.health = dto.health;
            this.runes = dto.runes;

            Debug.Log($"Received runes count: {dto.runes}");

            return Task.CompletedTask;
        }

        protected override Task OnMatchInformationUpdateAsync(MatchInformationDTO dto, CancellationToken cancellationToken)
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

            var droppedCardGameObject = eventData.pointerDrag;
            var droppedCardBehaviour = droppedCardGameObject.GetComponent<CardBehaviour>();
            var droppedCardInstance =
                droppedCardBehaviour.CardInstance
                ?? throw new InvalidOperationException($"{droppedCardBehaviour.CardInstance} property of a dropped item is expected to be set");
            (var droppedCardSource, _) = CardDragAndDropService.GetCardDragSource(droppedCardGameObject);

            cardDragAndDropService!.CardDrop(
                droppedCardInstance,
                droppedCardSource,
                droppedCardBehaviour,
                CardDragSource.Face,
                targetIsOwn: playerType == PlayerType.Self,
                targetCardInstance: null,
                destroyCancellationToken
            );
        }
    }
}
