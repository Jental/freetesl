using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class HandCreator : MonoBehaviour
    {
        public DisplayCard cardPrefab;

        private Dictionary<int, Card> allCards;
        private List<Card> cardsToShow = new List<Card>();
        private DisplayCard[] displayCards = Array.Empty<DisplayCard>();
        private System.Random rnd = new System.Random();
        private DateTime lastUpdateTime;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            allCards = Resources.LoadAll<Card>("CardObjects").ToDictionary(c => c.id, c => c);
            lastUpdateTime = DateTime.Now;
        }

        // Update is called once per frame
        void Update()
        {
            if (allCards.Count > 0 && DateTime.Now - lastUpdateTime >= TimeSpan.FromSeconds(5) && allCards.Count < 10)
            {
                int cardIdx = rnd.Next(allCards.Count);
                var card = allCards.ElementAt(cardIdx).Value;
                cardsToShow.Add(card);

                foreach (var dc in displayCards)
                {
                    Destroy(dc);
                }

                displayCards = cardsToShow.Select((c, i) => {
                    var dc = Instantiate(cardPrefab, new Vector3(i * 100, 0, 0), Quaternion.identity);
                    dc.transform.parent = gameObject.transform;
                    dc.displayCard = c;
                    dc.showFront = true;
                    return dc;
                }).ToArray();

                lastUpdateTime = DateTime.Now;
            }
        }
    }
}
