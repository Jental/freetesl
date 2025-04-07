#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class HandBehaviour : AWithMatchStateSubscribtionBehaviour, IDropHandler
    {
        public CardBehaviour? CardPrefab = null;

        private List<CardInstance> cardsToShow = new List<CardInstance>();
        private RectTransform? rectTransform;
        private HorizontalLayoutGroup? hLayoutGroup;
        private RectTransform? canvasRectTransform;
        private CardDragAndDropService? cardDragAndDropService;

        protected new void Start()
        {
            base.Start();

            rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null) throw new InvalidOperationException($"{nameof(RectTransform)} component is expected to be set");

            hLayoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (hLayoutGroup == null) throw new InvalidOperationException($"{nameof(HorizontalLayoutGroup)} component is expected to be set");

            var canvasGameObject = GameObject.Find("Canvas")?.GetComponent<Canvas>() ?? throw new InvalidOperationException($"Canvas gameObject is not found");
            canvasRectTransform = canvasGameObject.GetComponent<RectTransform>() ?? throw new InvalidOperationException($"Canvas RectTransform is not found");
            cardDragAndDropService = GameObject.Find("CardDragAndDropService")?.GetComponent<CardDragAndDropService>() ?? throw new InvalidOperationException($"CardDragAndDropService gameObject is not found");

            _ = destroyCancellationToken;
        }

        protected new void OnDisable()
        {
            base.OnDisable();
            cardsToShow.Clear();
            changesArePresent = true;
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, MatchStateDTO _, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            cardsToShow = dto.hand.Select(ciState =>
            {
                var cardInstance = GlobalStorage.Instance.AllCardInstances[ciState.CardInstanceGuid];
                cardInstance.IsActive = ciState.isActive; // not very happy about mutating here, but it seems reasonable logic-wise
                return cardInstance;
            }).ToList();

            return Task.CompletedTask;
        }

        protected override void UpdateImpl()
        {
            var children = gameObject.transform.GetComponentsInChildren<CardBehaviour>();
            foreach (var dc in children)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            if (cardsToShow.Count > 0)
            {
                var gameObjectWidthInShares = (Constants.MAX_HAND_CARDS - 1) * (1 - Constants.HAND_CARD_OVERFLOW) + 1.5f; // total width of hand cards in shares of card width
                var widthFromCanvas = canvasRectTransform!.rect.width;
                var maxWidth = widthFromCanvas * 0.68f;
                var cardWidth = maxWidth / gameObjectWidthInShares;
                var cardHeight = cardWidth / Constants.CARD_ASPECT_RATIO;

                if (cardsToShow.Count > 5)
                {
                    rectTransform!.anchorMin = new Vector2(0.05f, rectTransform.anchorMin.y);
                    rectTransform.anchorMax = new Vector2(0.68f, rectTransform.anchorMax.y);
                    hLayoutGroup!.childAlignment = TextAnchor.UpperRight;
                }
                else
                {
                    rectTransform!.anchorMin = new Vector2(0.33f, rectTransform.anchorMin.y);
                    rectTransform.anchorMax = new Vector2(0.69f, rectTransform.anchorMax.y);
                    hLayoutGroup!.childAlignment = TextAnchor.UpperCenter;
                }

                hLayoutGroup.spacing = -cardWidth * Constants.HAND_CARD_OVERFLOW;

                for (int i = 0; i < cardsToShow.Count; i++)
                {
                    var card = cardsToShow[i];

                    var dc =
                        Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a card prefab");
                    dc.transform.parent = gameObject.transform;
                    dc.UpdateDisplaySettings(
                        card,
                        playerType == PlayerType.Self ? CardDisplayMode.Full : CardDisplayMode.Cover,
                        isFloating: false
                    );

                    var cardRect = dc.gameObject.GetComponent<RectTransform>();
                    cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
                    cardRect.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        protected override void VerifyFields()
        {
            if (this.CardPrefab == null) throw new InvalidOperationException($"{nameof(CardPrefab)} prefab is expected to be set");
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (this.playerType != PlayerType.Self) return;

            Debug.Log("LaneBehaviour.OnDrop");

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
                CardDragSource.Hand,
                targetIsOwn: playerType == PlayerType.Self,
                targetCardInstance: null,
                destroyCancellationToken
            );
        }

        public void AddCard(CardInstance cardInstance)
        {
            cardsToShow.Add(cardInstance);
            changesArePresent = true;
        }
    }
}