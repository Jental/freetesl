#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class CardBehaviour: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
    {
        public CardInstance? cardInstance = null;
        public bool showFront = false;
        public bool isFloating = false;

        public TextMeshProUGUI? nameText = null;
        public TextMeshProUGUI? descriptionText = null;
        public RawImage? image = null;
        public TextMeshProUGUI? powerTextGameObject = null;
        public TextMeshProUGUI? healthTextGameObject = null;
        public TextMeshProUGUI? costTextGameObject = null;

        public Image? frontEl = null;
        public RawImage? backEl = null;
        public Image? shadowEl = null;

        private Canvas? canvasGameObject = null;
        private LineRenderer? actionLineGameObject = null;

        private RectTransform? rectTransform;
        private Image? imageComponent;
        private Vector2? anchoredPoitionBeforeDrag;
        private bool isDraggedAsCard = false; // if dragging drags card itself (true) or just shows an arrow (false)

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            imageComponent = GetComponent<Image>();

            canvasGameObject = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            actionLineGameObject = GameObject.Find("ActionLine")?.GetComponent<LineRenderer>();

            _ = destroyCancellationToken;
        }

        protected void Update()
        {
            if (frontEl == null) throw new InvalidOperationException($"{nameof(frontEl)} gameObject is expected to be set");
            if (backEl == null) throw new InvalidOperationException($"{nameof(backEl)} gameObject is expected to be set");
            if (shadowEl == null) throw new InvalidOperationException($"{nameof(shadowEl)} gameObject is expected to be set");
            if (nameText == null) throw new InvalidOperationException($"{nameof(nameText)} gameObject is expected to be set");
            if (descriptionText == null) throw new InvalidOperationException($"{nameof(descriptionText)} gameObject is expected to be set");
            if (image == null) throw new InvalidOperationException($"{nameof(image)} gameObject is expected to be set");
            if (powerTextGameObject == null) throw new InvalidOperationException($"{nameof(powerTextGameObject)} gameObject is expected to be set");
            if (healthTextGameObject == null) throw new InvalidOperationException($"{nameof(healthTextGameObject)} gameObject is expected to be set");
            if (costTextGameObject == null) throw new InvalidOperationException($"{nameof(costTextGameObject)} gameObject is expected to be set");

            frontEl.gameObject.SetActive(showFront);
            backEl.gameObject.SetActive(!showFront);
            shadowEl.gameObject.SetActive(isFloating);

            if (showFront)
            {
                if (cardInstance != null)
                {
                    nameText.text = (string)cardInstance.Card.ScriptableObject.cardName.Clone();
                    descriptionText.text = (string)cardInstance.Card.ScriptableObject.description.Clone();
                    image.texture = cardInstance.Card.ScriptableObject.image.texture;
                    powerTextGameObject.text = cardInstance.Power.ToString();
                    healthTextGameObject.text = cardInstance.Health.ToString();
                    costTextGameObject.text = cardInstance.Cost.ToString();
                }
            }
        }

        private static CardDragSource GetCardDragSource(GameObject draggedCard)
        {
            var parentHandComponent = draggedCard.GetComponentInParent<HandBehaviour>();
            var parentLaneCardsComponent = draggedCard.GetComponentInParent<LaneCardsBehaviour>();

            if (parentHandComponent != null)
            {
                return
                    parentHandComponent.playerType == PlayerType.Self
                    ? CardDragSource.Hand
                    : CardDragSource.Other; // opponent's hand
            }
            else if (parentLaneCardsComponent != null)
            {
                if (parentLaneCardsComponent.playerType != PlayerType.Self)
                {
                    return CardDragSource.Other; // opponent's lane
                }
                else
                {
                    return parentLaneCardsComponent.laneID switch
                    {
                        Constants.LEFT_LANE_ID => CardDragSource.LeftLane,
                        Constants.RIGHT_LANE_ID => CardDragSource.RightLane,
                        _ => CardDragSource.Other,
                    };
                }
            }
            else
            {
                return CardDragSource.Other;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null) throw new InvalidOperationException("RectTransform component is expected to be added");
            if (canvasGameObject == null) throw new InvalidOperationException($"{nameof(canvasGameObject)} gameObject is expected to be set");
            if (actionLineGameObject == null) throw new InvalidOperationException($"{nameof(actionLineGameObject)} gameObject is expected to be set");

            if (isDraggedAsCard)
            {
                rectTransform.anchoredPosition += eventData.delta / canvasGameObject.scaleFactor;
            }
            else
            {
                var currentPosition = new Vector3(eventData.position.x / canvasGameObject.scaleFactor, eventData.position.y / canvasGameObject.scaleFactor, -2.0f);
                actionLineGameObject.SetPosition(1, currentPosition);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("CardBehaviour.OnBeginDrag");
            if (rectTransform == null) throw new InvalidOperationException("RectTransform component is expected to be added");
            if (imageComponent == null) throw new InvalidOperationException("Image component is expected to be added");
            if (canvasGameObject == null) throw new InvalidOperationException($"{nameof(canvasGameObject)} gameObject is expected to be set");
            if (actionLineGameObject == null) throw new InvalidOperationException($"{nameof(actionLineGameObject)} gameObject is expected to be set");

            if (cardInstance == null || !cardInstance.IsActive)
            {
                Debug.Log("CardBehaviour.OnBeginDrag: aborted (card not active)");
                eventData.pointerDrag = null;
                return;
            }

            var cardDragSource = GetCardDragSource(gameObject);

            if (cardDragSource == CardDragSource.Other)
            {
                Debug.Log("CardBehaviour.OnBeginDrag: aborted (invalid drag source)");
                eventData.pointerDrag = null;
                return;
            }

            if (cardDragSource == CardDragSource.Hand)
            {
                if (cardInstance.Cost > GlobalStorage.Instance.PlayerMatchStateDTO.mana)
                {
                    Debug.Log("CardBehaviour.OnBeginDrag: aborted (not enough mana)");
                    eventData.pointerDrag = null;
                    return;
                }
            }

            isDraggedAsCard = cardDragSource == CardDragSource.Hand; // TODO: check cart type for actions later

            if (isDraggedAsCard)
            {
                this.anchoredPoitionBeforeDrag = rectTransform.anchoredPosition;
                imageComponent.raycastTarget = false;
            }
            else
            {
                actionLineGameObject.enabled = true;
                var pressPosition = new Vector3(eventData.pressPosition.x / canvasGameObject.scaleFactor, eventData.pressPosition.y / canvasGameObject.scaleFactor, -2.0f);
                actionLineGameObject.SetPosition(0, pressPosition);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("CardBehaviour.OnEndDrag");
            if (imageComponent == null) throw new InvalidOperationException("Image component is expected to be added");
            if (actionLineGameObject == null) throw new InvalidOperationException($"{nameof(actionLineGameObject)} gameObject is expected to be set");

            if (isDraggedAsCard)
            {
                imageComponent.raycastTarget = true;
            }
            else
            {
                actionLineGameObject.enabled = false;
            }
        }

        public void ReturnBack()
        {
            if (rectTransform == null) throw new InvalidOperationException("RectTransform component is expected to be added");
            if (actionLineGameObject == null) throw new InvalidOperationException($"{nameof(actionLineGameObject)} gameObject is expected to be set");

            if (isDraggedAsCard)
            {
                if (this.anchoredPoitionBeforeDrag != null)
                {
                    rectTransform.anchoredPosition = anchoredPoitionBeforeDrag.Value;
                }
            }
            else
            {
                actionLineGameObject.enabled = false;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("CardBehaviour.OnDrop");
            if (cardInstance == null) throw new InvalidOperationException($"{nameof(cardInstance)} field is expected to be filled");

            // TODO: this method should be updated with actions introduction

            var dropped = eventData.pointerDrag;
            var cardDragSource = GetCardDragSource(dropped);

            if (cardDragSource != CardDragSource.LeftLane && cardDragSource != CardDragSource.RightLane)
            {
                Debug.Log("CardBehaviour.OnDrop: drop not accepted - only lane sources are allowed");
                return;
            }

            var sourceDisplayCard = dropped.GetComponent<CardBehaviour>();
            var sourceCardInstance =
                sourceDisplayCard.cardInstance
                ?? throw new InvalidOperationException($"{sourceDisplayCard.cardInstance} property of a dropped item is expected to be set");
            
            var sourceParentLaneCardsComponent = dropped.GetComponentInParent<LaneCardsBehaviour>();
            var targetParentLaneCardsComponent = gameObject.GetComponentInParent<LaneCardsBehaviour>();

            if (targetParentLaneCardsComponent.playerType != PlayerType.Opponent)
            {
                Debug.Log("CardBehaviour.OnDrop: drop not accepted - only opponent cards are allowed");
                return;
            }

            if (targetParentLaneCardsComponent.laneID != sourceParentLaneCardsComponent.laneID)
            {
                Debug.Log("CardBehaviour.OnDrop: drop not accepted - other lane");
                return;
            }

            cardInstance.Health = cardInstance.Health - sourceCardInstance.Power;
            cardInstance.IsActive = false;

            _ = Task.Run(async () =>
            {
                var dto = new HitCardDTO
                {
                    cardInstanceID = sourceCardInstance.ID.ToString(),
                    opponentCardInstanceID = cardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.HIT_CARD, dto, destroyCancellationToken);
            });
        }
    }
}
