#nullable enable

using Assets.Models;
using Assets.Services;
using System;
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
        private ManaDisplayBehaviour? manaDisplayGameObject = null;

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
                    nameText.text = (string)cardInstance.Card.cardName.Clone();
                    descriptionText.text = (string)cardInstance.Card.description.Clone();
                    image.texture = cardInstance.Card.image.texture;
                    powerTextGameObject.text = cardInstance.Card.power.ToString();
                    healthTextGameObject.text = cardInstance.Card.health.ToString();
                    costTextGameObject.text = cardInstance.Card.cost.ToString();
                }
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

            this.anchoredPoitionBeforeDrag = rectTransform.anchoredPosition;
            var parentHandComponent = gameObject.GetComponentInParent<HandBehaviour>();
            isDraggedAsCard = parentHandComponent != null && parentHandComponent.playerType == PlayerType.Self;

            if (isDraggedAsCard)
            {
                if (cardInstance.Cost > GlobalStorage.Instance.PlayerMatchStateDTO.mana)
                {
                    Debug.Log("CardBehaviour.OnBeginDrag: aborted (not enough mana)");
                    eventData.pointerDrag = null;
                }
                imageComponent.raycastTarget = false;
            }
            else if (parentHandComponent == null)
            {
                var parentLaneCardsComponent = gameObject.GetComponentInParent<LaneCardsBehaviour>();
                if (parentLaneCardsComponent != null && parentLaneCardsComponent.playerType == PlayerType.Self)
                {
                    actionLineGameObject.enabled = true;
                    var pressPosition = new Vector3(eventData.pressPosition.x / canvasGameObject.scaleFactor, eventData.pressPosition.y / canvasGameObject.scaleFactor, -2.0f);
                    actionLineGameObject.SetPosition(0, pressPosition);
                }
                else
                {
                    Debug.Log("CardBehaviour.OnBeginDrag: aborted (invalid drag source)");
                    eventData.pointerDrag = null;
                    return;
                }
            }
            else
            {
                Debug.Log("CardBehaviour.OnBeginDrag: aborted (invalid drag source - opponent's hand)");
                eventData.pointerDrag = null;
                return;
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

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("CardBehaviour.OnDrop");
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
    }
}
