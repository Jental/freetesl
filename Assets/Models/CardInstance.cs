using System;

namespace Assets.Models
{
    public class CardInstance
    {
        public Card Card { get; private set; }
        public Guid ID { get; private set; }

        public bool IsActive { get; set; }

        public CardInstance(Card card, Guid id, bool isActive)
        {
            Card = card;
            ID = id;
            IsActive = isActive;
        }
    }
}
