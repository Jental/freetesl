#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
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
    public class HandBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        public CardBehaviour? CardPrefab = null;

        private List<CardInstance> cardsToShow = new List<CardInstance>();

        protected void OnDisable()
        {
            cardsToShow.Clear();
            changesArePresent = true;
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            cardsToShow = dto.hand.Select(ciState =>
            {
                var cardInstance = GlobalStorage.Instance.AllCardInstances[ciState.CardInstanceGuid];
                cardInstance.IsActive = ciState.isActive; // not very happy about mutating here, but it seems reasonable logic-wise
                return cardInstance;
            }).ToList();

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
                var handRect = gameObject.GetComponent<RectTransform>();
                float cardHeight = handRect.rect.height;
                float cardWidth = cardHeight * Constants.CARD_ASPECT_RATIO;
                float cardWidthShare = cardWidth / handRect.rect.width;
                float totalCardWidthShare = cardWidthShare * (1 - Constants.HAND_CARD_OVERFLOW) * cardsToShow.Count + cardWidthShare * Constants.HAND_CARD_OVERFLOW;
                float marginWidthShare = Math.Max(0.0f, (1 - totalCardWidthShare) / 2.0f);

                for (int i = 0; i < cardsToShow.Count; i++)
                {
                    var card = cardsToShow[i];

                    var dc =
                        Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a card prefab");
                    dc.transform.parent = gameObject.transform;
                    dc.UpdateDisplaySettings(
                        card,
                        playerType == PlayerType.Self ? CardDisplayMode.Full : CardDisplayMode.Cover,
                        isFloating: false
                    );

                    var cardRect = dc.gameObject.GetComponent<RectTransform>();
                    cardRect.anchorMin = new Vector2(marginWidthShare + cardWidthShare * i * (1 - Constants.HAND_CARD_OVERFLOW), 0);
                    cardRect.anchorMax = new Vector2(marginWidthShare + cardWidthShare * (i * (1 - Constants.HAND_CARD_OVERFLOW) + 1), 1);
                    cardRect.offsetMin = new Vector2(0, 0);
                    cardRect.offsetMax = new Vector2(0, 0);
                    cardRect.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        protected override void VerifyFields()
        {
            if (this.CardPrefab == null) throw new InvalidOperationException($"{nameof(CardPrefab)} prefab is expected to be set");
        }
    }
}