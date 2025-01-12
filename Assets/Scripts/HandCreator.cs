using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts
{
    public class HandCreator : MonoBehaviour
    {
        private const float CARD_ASPECT_RATIO = 0.6f;
        private const float OVERFLOW = 0.3f;
        private static readonly TimeSpan TIMER_TICK = TimeSpan.FromSeconds(10);

        public DisplayCard cardPrefab;

        private Dictionary<int, Card> allCards;
        private List<CardInstance> cardsToShow = new List<CardInstance>();
        private System.Random rnd = new System.Random();
        private Timer addCardTimer;
        private bool changesArePresent = false;

        protected void Start()
        {
            allCards = Resources.LoadAll<Card>("CardObjects").ToDictionary(c => c.id, c => c);

            addCardTimer = new Timer((_) => AddCard(), null, TimeSpan.FromSeconds(0), TIMER_TICK);
        }

        protected void Update()
        {
            lock (this)
            {
                if (!changesArePresent)
                {
                    return;
                }

                var children = gameObject.transform.GetComponentsInChildren<DisplayCard>();
                foreach (var dc in children)
                {
                    Destroy(dc.gameObject);
                    Destroy(dc);
                }

                if (cardsToShow.Count > 0)
                {
                    var handRect = gameObject.GetComponent<RectTransform>();
                    float cardHeight = handRect.rect.height;
                    float cardWidth = cardHeight * CARD_ASPECT_RATIO;
                    float cardWidthShare = cardWidth / handRect.rect.width;
                    float totalCardWidthShare = cardWidthShare * (1 - OVERFLOW) * cardsToShow.Count + cardWidthShare * OVERFLOW;
                    float marginWidthShare = Math.Max(0.0f, (1 - totalCardWidthShare) / 2.0f);

                    for (int i = 0; i < cardsToShow.Count; i++)
                    {
                        var card = cardsToShow[i];

                        var dc = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        dc.transform.parent = gameObject.transform;
                        dc.displayCard = card;
                        dc.showFront = true;

                        var cardRect = dc.gameObject.GetComponent<RectTransform>();
                        cardRect.anchorMin = new Vector2(marginWidthShare + cardWidthShare * i * (1 - OVERFLOW), 0);
                        cardRect.anchorMax = new Vector2(marginWidthShare + cardWidthShare * (i * (1 - OVERFLOW) + 1), 1);
                        cardRect.offsetMin = new Vector2(0, 0);
                        cardRect.offsetMax = new Vector2(0, 0);
                        cardRect.localScale = new Vector3(1, 1, 1);
                    }
                }

                changesArePresent = false;
            }
        }

        private void AddCard()
        {
            lock (this)
            {
                if (cardsToShow.Count >= 10)
                {
                    addCardTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    return;
                }

                int cardIdx = rnd.Next(allCards.Count);
                var card = allCards.ElementAt(cardIdx).Value;
                var cardInstance = new CardInstance(card, Guid.NewGuid());
                cardsToShow.Add(cardInstance);
                changesArePresent = true;
            }
        }
    }
}
