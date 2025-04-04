#nullable enable

using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Behaviours
{
    public class LaneBehaviour : MonoBehaviour, IDropHandler
    {
        [SerializeField] private LaneCardsBehaviour? laneCardsGameObject;
        [SerializeField] private LaneType laneID = LaneType.Left;
        [SerializeField] private PlayerType playerType = PlayerType.Self;

        private CardDragAndDropService? cardDragAndDropService;

        public LaneType LaneID => laneID;

        protected void Start()
        {
            if (laneCardsGameObject == null) throw new InvalidOperationException($"{laneCardsGameObject} gameObject is expected to be set");

            cardDragAndDropService = GameObject.Find("CardDragAndDropService")?.GetComponent<CardDragAndDropService>() ?? throw new InvalidOperationException($"CardDragAndDropService gameObject is not found");

            _ = destroyCancellationToken;

            laneCardsGameObject.Init(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("LaneBehaviour.OnDrop");

            var droppedCardGameObject = eventData.pointerDrag;
            var droppedCardBehaviour = droppedCardGameObject.GetComponent<CardBehaviour>();
            var droppedCardInstance =
                droppedCardBehaviour.CardInstance
                ?? throw new InvalidOperationException($"{droppedCardBehaviour.CardInstance} property of a dropped item is expected to be set");
            (var droppedCardSource, _) = CardDragAndDropService.GetCardDragSource(droppedCardGameObject);

            var targetCardSource = this.laneID == LaneType.Left ? CardDragSource.LeftLane : CardDragSource.RightLane;
            bool targetIsOwn = this.playerType == PlayerType.Self;

            cardDragAndDropService!.CardDrop(
                droppedCardInstance,
                droppedCardSource,
                droppedCardBehaviour,
                targetCardSource,
                targetIsOwn,
                targetCardInstance: null,
                destroyCancellationToken
            );
        }
    }
}