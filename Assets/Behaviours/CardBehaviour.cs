#nullable enable

using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class CardBehaviour: MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private CardInstance? cardInstance = null;
        private CardDisplayMode displayMode = CardDisplayMode.Cover;
        private bool isFloating = false;
        private TooltipBehaviour? tooltipGameObject = null;
        private bool isReturnBackRequested = false;

        [SerializeField] private TextMeshProUGUI? nameText;
        [SerializeField] private TextMeshProUGUI? descriptionText;
        [SerializeField] private RawImage? image;
        [SerializeField] private TextMeshProUGUI? powerTextGameObject;
        [SerializeField] private TextMeshProUGUI? healthTextGameObject;
        [SerializeField] private TextMeshProUGUI? costTextGameObject;

        [SerializeField] private GameObject? backGameObject;
        [SerializeField] private GameObject? borderGameObject;
        [SerializeField] private GameObject? shadowGameObject;
        [SerializeField] private GameObject? guardBorderGameObject;
        [SerializeField] private GameObject? guardShadowGameObject;
        [SerializeField] private GameObject? cardImageGameObject;
        [SerializeField] private GameObject? backgroundGameObject;
        [SerializeField] private GameObject? powerGameObject;
        [SerializeField] private GameObject? defenceGameObject;
        [SerializeField] private GameObject? raceGameObject;
        [SerializeField] private GameObject? costGameObject;
        [SerializeField] private GameObject? nameGameObject;
        [SerializeField] private GameObject? shackleGameObject;
        [SerializeField] private GameObject? coverGameObject;

        private Canvas? canvasGameObject;
        private CardDragAndDropService? cardDragAndDropService;
        private LineRenderer? actionLineGameObject;

        /// <summary>
        /// Children game objects that COULD be visible in defined mode (grandchildren are not expected to be here)
        /// </summary>
        private Dictionary<CardDisplayMode, GameObject[]> modesToGameObjects = new Dictionary<CardDisplayMode, GameObject[]>();
        private HashSet<GameObject> gameObjectsForCurrentMode = new HashSet<GameObject>();
        private HashSet<GameObject> allChildrenGameObjects = new HashSet<GameObject>();

        private RectTransform? rectTransform;
        private Image? imageComponent;
        private Vector2? anchoredPoitionBeforeDrag;
        private bool isDraggedAsCard = false; // if dragging drags card itself (true) or just shows an arrow (false)

        public CardInstance CardInstance =>
            cardInstance
            ?? throw new InvalidOperationException($"CardBehaviour: Not initialized. Call {nameof(UpdateDisplaySettings)} method.");

        protected void Awake()
        {
            if (backGameObject == null) throw new InvalidOperationException($"{nameof(backGameObject)} gameObject is expected to be set");
            if (borderGameObject == null) throw new InvalidOperationException($"{nameof(borderGameObject)} gameObject is expected to be set");
            if (shadowGameObject == null) throw new InvalidOperationException($"{nameof(shadowGameObject)} gameObject is expected to be set");
            if (guardBorderGameObject == null) throw new InvalidOperationException($"{nameof(guardBorderGameObject)} gameObject is expected to be set");
            if (guardShadowGameObject == null) throw new InvalidOperationException($"{nameof(guardShadowGameObject)} gameObject is expected to be set");
            if (cardImageGameObject == null) throw new InvalidOperationException($"{nameof(cardImageGameObject)} gameObject is expected to be set");
            if (backgroundGameObject == null) throw new InvalidOperationException($"{nameof(backgroundGameObject)} gameObject is expected to be set");
            if (powerGameObject == null) throw new InvalidOperationException($"{nameof(powerGameObject)} gameObject is expected to be set");
            if (defenceGameObject == null) throw new InvalidOperationException($"{nameof(defenceGameObject)} gameObject is expected to be set");
            if (raceGameObject == null) throw new InvalidOperationException($"{nameof(raceGameObject)} gameObject is expected to be set");
            if (costGameObject == null) throw new InvalidOperationException($"{nameof(costGameObject)} gameObject is expected to be set");
            if (nameGameObject == null) throw new InvalidOperationException($"{nameof(nameGameObject)} gameObject is expected to be set");
            if (shackleGameObject == null) throw new InvalidOperationException($"{nameof(shackleGameObject)} gameObject is expected to be set");
            if (coverGameObject == null) throw new InvalidOperationException($"{nameof(coverGameObject)} gameObject is expected to be set");

            if (nameText == null) throw new InvalidOperationException($"{nameof(nameText)} gameObject is expected to be set");
            if (descriptionText == null) throw new InvalidOperationException($"{nameof(descriptionText)} gameObject is expected to be set");
            if (image == null) throw new InvalidOperationException($"{nameof(image)} gameObject is expected to be set");
            if (powerTextGameObject == null) throw new InvalidOperationException($"{nameof(powerTextGameObject)} gameObject is expected to be set");
            if (healthTextGameObject == null) throw new InvalidOperationException($"{nameof(healthTextGameObject)} gameObject is expected to be set");
            if (costTextGameObject == null) throw new InvalidOperationException($"{nameof(costTextGameObject)} gameObject is expected to be set");

            canvasGameObject = GameObject.Find("Canvas")?.GetComponent<Canvas>() ?? throw new InvalidOperationException($"Canvas gameObject is not found");
            actionLineGameObject = GameObject.Find("ActionLine")?.GetComponent<LineRenderer>() ?? throw new InvalidOperationException($"ActionLine gameObject is not found");
            cardDragAndDropService = GameObject.Find("CardDragAndDropService")?.GetComponent<CardDragAndDropService>() ?? throw new InvalidOperationException($"CardDragAndDropService gameObject is not found");

            modesToGameObjects = new Dictionary<CardDisplayMode, GameObject[]>
            {
                { CardDisplayMode.Cover, new GameObject[] { backGameObject } },
                { CardDisplayMode.Light, new GameObject[] {
                    borderGameObject,
                    guardBorderGameObject,
                    cardImageGameObject,
                    powerGameObject,
                    defenceGameObject,
                    shackleGameObject,
                    coverGameObject
                } },
                { CardDisplayMode.Full, new GameObject[] {
                    borderGameObject,
                    cardImageGameObject,
                    backgroundGameObject,
                    powerGameObject,
                    defenceGameObject,
                    raceGameObject,
                    costGameObject,
                    nameGameObject,
                    shackleGameObject,
                    coverGameObject
                } },
            };
            allChildrenGameObjects = modesToGameObjects.SelectMany(gos => gos.Value).ToHashSet();

            rectTransform = GetComponent<RectTransform>() ?? throw new InvalidOperationException($"RectTransform component is not found");
            imageComponent = GetComponent<Image>() ?? throw new InvalidOperationException($"Image component is not found");

            _ = destroyCancellationToken;
        }

        public void UpdateDisplaySettings(CardInstance cardInstance, CardDisplayMode displayMode, bool isFloating, TooltipBehaviour tooltipGameObject)
        {
            this.cardInstance = cardInstance;
            this.displayMode = displayMode;
            this.isFloating = isFloating;
            this.tooltipGameObject = tooltipGameObject;

            gameObjectsForCurrentMode =
                modesToGameObjects.GetValueOrDefault(displayMode)?.ToHashSet() 
                ?? throw new InvalidOperationException($"GameObjects for mode '{displayMode}' are not specified");
        }

        protected void Update()
        {
            if (cardInstance == null) throw new InvalidOperationException($"CardBehaviour: Not initialized. Call {nameof(UpdateDisplaySettings)} method.");
            if (rectTransform == null) throw new InvalidOperationException("RectTransform component is expected to be added");
            if (actionLineGameObject == null) throw new InvalidOperationException($"{nameof(actionLineGameObject)} gameObject is expected to be set");

            foreach (var go in allChildrenGameObjects)
            {
                go.SetActive(gameObjectsForCurrentMode.Contains(go));
            }

            bool isGuard = cardInstance.Keywords.Any(kw => kw.Keyword == Keyword.Guard);
            bool showGuardBorder = isGuard && displayMode == CardDisplayMode.Light;
            borderGameObject!.SetActive(!showGuardBorder && gameObjectsForCurrentMode.Contains(borderGameObject));
            shadowGameObject!.SetActive(isFloating && !showGuardBorder); // No gameObjectsForCurrentMode because shadowGameObject is in borderGameObject
            guardBorderGameObject!.SetActive(showGuardBorder && gameObjectsForCurrentMode.Contains(guardBorderGameObject));
            guardShadowGameObject!.SetActive(isFloating && showGuardBorder); // No gameObjectsForCurrentMode because guardShadowGameObject is in guardBorderGameObject

            if (displayMode != CardDisplayMode.Cover)
            {
                nameText!.text = (string)cardInstance.Card.ScriptableObject.cardName.Clone();
                descriptionText!.text = (string)cardInstance.Card.ScriptableObject.description.Clone();
                image!.texture = cardInstance.Card.ScriptableObject.image.texture;
                powerTextGameObject!.text = cardInstance.Power.ToString();
                healthTextGameObject!.text = cardInstance.Health.ToString();
                costTextGameObject!.text = cardInstance.Cost.ToString();

                bool isShackled = cardInstance.Effects.Any(eff => eff.EffectType == EffectType.Shackled);
                shackleGameObject!.SetActive(isShackled && gameObjectsForCurrentMode.Contains(shackleGameObject));

                bool withCover = cardInstance.Effects.Any(eff => eff.EffectType == EffectType.Cover);
                coverGameObject!.SetActive(withCover && gameObjectsForCurrentMode.Contains(coverGameObject));
            }

            if (isReturnBackRequested)
            {
                isReturnBackRequested = false;

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
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (displayMode == CardDisplayMode.Light)
            {
                var tooltipStr = $"{cardInstance!.Card.ScriptableObject.name}\n{cardInstance.Card.ScriptableObject.description}";
                if (cardInstance.Keywords.Length > 0)
                {
                    var keywordsStr =
                        string.Join(
                            '\n',
                            cardInstance!.Keywords.Select(kw => 
                                kw.SourceCardInstance != null
                                ? $"{kw.SourceCardInstance.Card.ScriptableObject.cardName}: {Enum.GetName(typeof(Keyword), kw.Keyword)}"
                                : Enum.GetName(typeof(Keyword), kw.Keyword)
                            )
                        );
                    tooltipStr = $"{tooltipStr}\n{keywordsStr}";
                };
                if (cardInstance.Effects.Length > 0)
                {
                    var effectsStr =
                        string.Join(
                            '\n',
                            cardInstance!.Effects.Select(eff =>
                                eff.SourceCardInstance != null
                                ? $"{eff.SourceCardInstance.Card.ScriptableObject.cardName}: {eff.Description}"
                                : eff.Description
                            )
                        );
                    tooltipStr = $"{tooltipStr}\n{effectsStr}";
                };
                tooltipGameObject!.Show(tooltipStr);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (displayMode == CardDisplayMode.Light)
            {
                tooltipGameObject!.Hide();
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

            var matchStateDTO = GlobalStorage.Instance.MatchStateDTO;

            if (matchStateDTO.waitingForOtherPlayerAction)
            {
                Debug.Log("CardBehaviour.OnBeginDrag: aborted (waiting for opponent action)");
                eventData.pointerDrag = null;
                return;
            }

            if (cardInstance == null || !cardInstance.IsActive)
            {
                Debug.Log("CardBehaviour.OnBeginDrag: aborted (card not active)");
                eventData.pointerDrag = null;
                return;
            }

            (var cardDragSource, bool isOwn) = CardDragAndDropService.GetCardDragSource(gameObject);

            if (!isOwn)
            {
                Debug.Log("CardBehaviour.OnBeginDrag: only own cards can be dragged");
                eventData.pointerDrag = null;
                return;
            }

            if (cardDragSource == CardDragSource.Hand)
            {
                if (cardInstance.Cost > matchStateDTO.player.mana)
                {
                    Debug.Log("CardBehaviour.OnBeginDrag: aborted (not enough mana)");
                    eventData.pointerDrag = null;
                    return;
                }
            }

            // TODO: I think we should have 3 drag modes: draggedAsCard, ray, just apply (any drag target) - for supports and some actions
            //       for now, let's assume applying = dragging to lane
            isDraggedAsCard = cardInstance.Card.Type == CardType.Creature && (cardDragSource == CardDragSource.Hand || cardDragSource == CardDragSource.Prophecy);

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
            Debug.Log("CardBehaviour.ReturnBack");
            isReturnBackRequested = true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("CardBehaviour.OnDrop");
            if (cardInstance == null) throw new InvalidOperationException($"{nameof(cardInstance)} field is expected to be filled");

            var droppedCardGameObject = eventData.pointerDrag;
            var droppedCardBehaviour = droppedCardGameObject.GetComponent<CardBehaviour>();
            var droppedCardInstance =
                droppedCardBehaviour.cardInstance
                ?? throw new InvalidOperationException($"{droppedCardBehaviour.cardInstance} property of a dropped item is expected to be set");
            (var droppedCardSource, _) = CardDragAndDropService.GetCardDragSource(droppedCardGameObject);

            (var targetCardSource, bool targetIsOwn) = CardDragAndDropService.GetCardDragSource(this.gameObject);

            cardDragAndDropService!.CardDrop(
                droppedCardInstance,
                droppedCardSource,
                droppedCardBehaviour,
                targetCardSource,
                targetIsOwn,
                this.cardInstance,
                destroyCancellationToken
            );
        }
    }
}
