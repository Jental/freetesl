#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Behaviours
{
    public class LaneBehaviour : MonoBehaviour, IDropHandler
    {
        public LaneCardsBehaviour? LaneOwnCardsGameObject = null;
        public LaneCardsBehaviour? LaneOpponentCardsGameObject = null;
        public byte LaneID = Constants.LEFT_LANE_ID;

        protected void Start()
        {
            _ = destroyCancellationToken;

            if (LaneOwnCardsGameObject == null) throw new InvalidOperationException($"{LaneOwnCardsGameObject} gameObject is expected to be set");
            if (LaneOpponentCardsGameObject == null) throw new InvalidOperationException($"{LaneOpponentCardsGameObject} gameObject is expected to be set");

            LaneOwnCardsGameObject.Init(this);
            LaneOpponentCardsGameObject.Init(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (LaneOwnCardsGameObject == null) throw new InvalidOperationException($"{LaneOwnCardsGameObject} gameObject is expected to be set");

            Debug.Log("LaneBehaviour.OnDrop");

            var dropped = eventData.pointerDrag;
            var displayCard = dropped.GetComponent<CardBehaviour>();
            var cardInstance =
                displayCard.CardInstance
                ?? throw new InvalidOperationException($"{displayCard.CardInstance} property of a dropped item is expected to be set");
            
            var currentParentHandComponent = displayCard.gameObject.GetComponentInParent<HandBehaviour>();
            if (currentParentHandComponent == null)
            {
                displayCard.ReturnBack();
                return;
            }

            cardInstance.IsActive = false;

            LaneOwnCardsGameObject.AddCard(cardInstance);

            Destroy(dropped.gameObject);
            Destroy(dropped);

            _ = Task.Run(async () =>
            {
                var dto = new MoveCardToLaneDTO {
                    cardInstanceID = cardInstance.ID.ToString(),
                    laneID = LaneID,
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.MOVE_CARD_TO_LANE, dto, destroyCancellationToken);
            });
        }
    }
}