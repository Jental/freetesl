using System;

namespace Assets.Models
{
    public class CardInstance
    {
        public Card Card { get; private set; }
        public Guid ID { get; private set; }

        public CardInstance(Card card, Guid id)
        {
            Card = card;
            ID = id;
        }
    }
}
