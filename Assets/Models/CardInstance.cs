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
        public KeywordInstance[] Keywords { get; set; }
        public EffectInstance[] Effects { get; set; }
        public bool IsActive { get; set; }

        public Card Card { get; private set; }

        public CardInstance(Guid id, Card card, int power, int health, int cost, bool isActive)
        {
            ID = id;
            Power = power;
            Health = health;
            Cost = cost;
            IsActive = isActive;
            Card = card;
        }
    }
}
