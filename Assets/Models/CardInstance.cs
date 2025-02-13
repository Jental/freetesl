using System;

namespace Assets.Models
{
    public class CardInstance
    {
        public Card Card { get; private set; }
        public Guid ID { get; private set; }
        public int Power { get; private set; }
        public int Health { get; set; }
        public int Cost { get; private set; }
        public bool IsActive { get; set; }

        public CardInstance(Card card, Guid id, int power, int health, int cost, bool isActive)
        {
            Card = card;
            ID = id;
            Power = power;
            Health = health;
            Cost = cost;
            IsActive = isActive;
        }
    }
}
