using System;

namespace Assets.DTO
{
    [Serializable]
    public record PlayerMatchStateDTO
    {
        public CardInstanceDTO[] deck;
        public CardInstanceDTO[] hand;
        public int health;
        public byte runes;
        public int mana;
        public int maxMana;
        public CardInstanceDTO[] leftLaneCards;
    }
}
