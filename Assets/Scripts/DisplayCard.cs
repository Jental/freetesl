#nullable enable

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DisplayCard: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public CardInstance? displayCard = null;
        public bool showFront = false;

        public TextMeshProUGUI? nameText = null;
        public TextMeshProUGUI? descriptionText = null;
        public RawImage? image = null;
        public TextMeshProUGUI? powerTextGameObject = null;
        public TextMeshProUGUI? healthTextGameObject = null;
        public TextMeshProUGUI? costTextGameObject = null;

        public Image? frontEl = null;
        public RawImage? backEl = null;

        public Canvas? canvas = null;

        private RectTransform? rectTransform;
        private Image? imageComponent;

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            imageComponent = GetComponent<Image>();
        }

        protected void Update()
        {
            if (frontEl == null) throw new InvalidOperationException($"{nameof(frontEl)} gameObject is expected to be set");
            if (backEl == null) throw new InvalidOperationException($"{nameof(backEl)} gameObject is expected to be set");
            if (nameText == null) throw new InvalidOperationException($"{nameof(nameText)} gameObject is expected to be set");
            if (descriptionText == null) throw new InvalidOperationException($"{nameof(descriptionText)} gameObject is expected to be set");
            if (image == null) throw new InvalidOperationException($"{nameof(image)} gameObject is expected to be set");
            if (powerTextGameObject == null) throw new InvalidOperationException($"{nameof(powerTextGameObject)} gameObject is expected to be set");
            if (healthTextGameObject == null) throw new InvalidOperationException($"{nameof(healthTextGameObject)} gameObject is expected to be set");
            if (costTextGameObject == null) throw new InvalidOperationException($"{nameof(costTextGameObject)} gameObject is expected to be set");

            frontEl.gameObject.SetActive(showFront);
            backEl.gameObject.SetActive(!showFront);

            if (showFront)
            {
                if (displayCard != null)
                {
                    nameText.text = (string)displayCard.Card.cardName.Clone();
                    descriptionText.text = (string)displayCard.Card.description.Clone();
                    image.texture = displayCard.Card.image.texture;
                    powerTextGameObject.text = displayCard.Card.power.ToString();
                    healthTextGameObject.text = displayCard.Card.health.ToString();
                    costTextGameObject.text = displayCard.Card.cost.ToString();
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null) throw new InvalidOperationException("RectTransform component is expected to be added");
            if (canvas == null) throw new InvalidOperationException($"{nameof(canvas)} gameObject is expected to be set");

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (imageComponent == null) throw new InvalidOperationException("Image component is expected to be added");

            imageComponent.raycastTarget = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (imageComponent == null) throw new InvalidOperationException("Image component is expected to be added");

            imageComponent.raycastTarget = true;
        }
    }
}
