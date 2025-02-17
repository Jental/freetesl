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

namespace Assets.Behaviours
{
    public class LaneCardsBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        public CardBehaviour? CardPrefab = null;

        private List<CardInstance> cardsToShow = new List<CardInstance>();
        public byte? laneID;

        public void Init(LaneBehaviour laneGameObject)
        {
            if (laneGameObject != null)
            {
                laneID = laneGameObject.LaneID;
            }
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
                var gameObjectRect = gameObject.GetComponent<RectTransform>();
                float generalCardHeight = gameObjectRect.rect.height;
                float generalCardWidth = generalCardHeight * Constants.CARD_ASPECT_RATIO;
                float gameObjectWidth = cardsToShow.Count * generalCardWidth + (cardsToShow.Count - 1) * Constants.LANE_CARDS_GAP;
                gameObjectRect.anchorMin = new Vector2(0.5f, 0.0f);
                gameObjectRect.anchorMax = new Vector2(0.5f, 1.0f);
                gameObjectRect.sizeDelta = new Vector2(gameObjectWidth, 0.0f);
                gameObjectRect.anchoredPosition = new Vector2(0.0f, 0.0f);

                for (int i = 0; i < cardsToShow.Count; i++)
                {
                    var card = cardsToShow[i];

                    var dc =
                        Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a card prefab");
                    dc.transform.parent = gameObject.transform;
                    dc.cardInstance = card;
                    dc.showFront = true;
                    dc.isFloating = card.IsActive;

                    var cardRect = dc.gameObject.GetComponent<RectTransform>();
                    cardRect.anchorMin = new Vector2(0, 0.5f);
                    cardRect.anchorMax = new Vector2(0, 0.5f);
                    var cardHOffset = i * (generalCardWidth + Constants.LANE_CARDS_GAP) + generalCardWidth / 2;
                    cardRect.anchoredPosition = new Vector2(cardHOffset, 0.0f);
                    float cardHeight = card.IsActive ? generalCardHeight : (generalCardHeight - 5.0f);
                    float cardWidth = cardHeight * Constants.CARD_ASPECT_RATIO;
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