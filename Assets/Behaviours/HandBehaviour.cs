#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Models;
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
        private Dictionary<int, Card>? allCards = null;

        private Dictionary<int, Card> AllCardsNotNull => allCards ?? throw new InvalidOperationException("All cards collection is not initialized");

        protected new void Start()
        {
            base.Start();

            allCards = Resources.LoadAll<Card>("CardObjects").ToDictionary(c => c.id, c => c);
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            cardsToShow = dto.hand.Select(c => new CardInstance(
                AllCardsNotNull[c.cardID],
                c.CardInstanceGuid ?? throw new InvalidOperationException($"Card instance id is null or is not a guid: '{c.cardInstanceID}'"),
                c.isActive
            )).ToList();

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
                    dc.cardInstance = card;
                    dc.showFront = playerType == PlayerType.Self;
                    dc.isFloating = false;

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