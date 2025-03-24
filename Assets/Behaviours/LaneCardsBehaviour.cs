#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class LaneCardsBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        public CardBehaviour? CardPrefab = null;

        private List<CardInstance> cardsToShow = new List<CardInstance>();
        private byte? laneID;
        private RectTransform? laneRectTransform;

        public byte LaneID => laneID ?? throw new InvalidOperationException("LaneCardsBehaviour: not initialized");

        public void Init(LaneBehaviour laneGameObject)
        {
            if (laneGameObject != null)
            {
                laneID = laneGameObject.LaneID;

                laneRectTransform = laneGameObject.gameObject.GetComponent<RectTransform>();
            }
        }

        protected void OnDisable()
        {
            cardsToShow.Clear();
            changesArePresent = true;
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            try
            {
                var cardsDTO = laneID switch
                {
                    Constants.LEFT_LANE_ID => dto.leftLaneCards,
                    Constants.RIGHT_LANE_ID => dto.rightLaneCards,
                    _ => throw new InvalidOperationException($"Invalid lane id: '{laneID}'")
                };

                cardsToShow = cardsDTO.Select(ciState =>
                {
                    var cardInstance = GlobalStorage.Instance.AllCardInstances[ciState.CardInstanceGuid];
                    cardInstance.IsActive = ciState.isActive; // not very happy about mutating here, but it seems reasonable logic-wise
                    return cardInstance;
                }).ToList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

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
                var gameObjectWidthInShares = 4 * (1 + Constants.LANE_CARDS_GAP) + Constants.LANE_CARDS_GAP; // total width of lane cards in shares of card width
                var maxWidth = laneRectTransform!.rect.width;
                var cardWidth = maxWidth / gameObjectWidthInShares;
                var cardHeight = cardWidth / Constants.CARD_ASPECT_RATIO;

                var layoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.spacing = cardWidth * Constants.LANE_CARDS_GAP;
                }

                for (int i = 0; i < cardsToShow.Count; i++)
                {
                    var card = cardsToShow[i];

                    var dc =
                        Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a card prefab");
                    dc.transform.parent = gameObject.transform;
                    dc.UpdateDisplaySettings(
                        card,
                        Enums.CardDisplayMode.Light,
                        isFloating: card.IsActive
                    );

                    var cardRect = dc.gameObject.GetComponent<RectTransform>();
                    cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
                    cardRect.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        protected override void VerifyFields()
        {
            if (this.CardPrefab == null) throw new InvalidOperationException($"{nameof(CardPrefab)} prefab is expected to be passed. Call '{nameof(Init)}' method");
            if (this.laneID == null) throw new InvalidOperationException($"{nameof(laneID)} parameter is expected to be passed. Call '{nameof(Init)}' method");
        }

        public void AddCard(CardInstance cardInstance)
        {
            cardsToShow.Add(cardInstance);
            changesArePresent = true;
        }

        public void Redraw()
        {
            changesArePresent = true;
        }
    }
}