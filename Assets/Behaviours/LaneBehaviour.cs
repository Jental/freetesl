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
        public GameObject? HandGameObject = null;

        protected void Start()
        {
            _ = destroyCancellationToken;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (LaneOwnCardsGameObject == null) throw new InvalidOperationException($"{LaneOwnCardsGameObject} gameObject is expected to be set");

            Debug.Log("OnDrop");

            var dropped = eventData.pointerDrag;
            var displayCard = dropped.GetComponent<CardBehaviour>();
            var cardInstance =
                displayCard.displayCard
                ?? throw new InvalidOperationException($"{displayCard.displayCard} property of a dropped item is expected to be set");

            LaneOwnCardsGameObject.AddCard(cardInstance);

            Destroy(dropped.gameObject);
            Destroy(dropped);

            _ = Task.Run(async () =>
            {
                var dto = new MoveCardToLaneDTO {
                    playerID = Constants.TEST_PLAYER_ID,
                    cardInstanceID = cardInstance.ID.ToString(),
                    laneID = Constants.LEFT_LANE_ID,
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.MOVE_CARD_TO_LANE, dto, destroyCancellationToken);
            });
        }
    }
}