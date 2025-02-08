#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class LaneBehaviour : MonoBehaviour, IDropHandler
    {
        public LaneCardsBehaviour? LaneOwnCardsGameObject = null;
        public GameObject? HandGameObject = null;

        public void OnDrop(PointerEventData eventData)
        {
            if (LaneOwnCardsGameObject == null) throw new InvalidOperationException($"{LaneOwnCardsGameObject} gameObject is expected to be set");

            Debug.Log("OnDrop");

            var dropped = eventData.pointerDrag;
            var displayCard = dropped.GetComponent<DisplayCard>();
            var cardInstance =
                displayCard.displayCard
                ?? throw new InvalidOperationException($"{displayCard.displayCard} property of a dropped item is expected to be set");

            LaneOwnCardsGameObject.AddCard(cardInstance);

            Destroy(dropped.gameObject);
            Destroy(dropped);
        }
    }
}