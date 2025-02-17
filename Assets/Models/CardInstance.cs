using Assets.Enums;
using System;

namespace Assets.Models
{
    public class CardInstance
    {
        public Guid ID { get; private set; }
        public int Power { get; private set; }
        public int Health { get; set; }
        public int Cost { get; private set; }
        public Keyword[] Keywords { get; private set; }
        public bool IsActive { get; set; }

        public Card Card { get; private set; }

        public CardInstance(Guid id, Card card, int power, int health, int cost, Keyword[] keywords, bool isActive)
        {
            ID = id;
            Power = power;
            Health = health;
            Cost = cost;
            Keywords = keywords;
            IsActive = isActive;

            Card = card;
        }
    }
}
