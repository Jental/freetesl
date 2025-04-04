using System;

namespace Assets.DTO
{
    [Serializable]
    public record PlayerMatchStateDTO
    {
        public int health;
        public byte runes;
        public int mana;
        public int maxMana;
        public CardInstanceStateDTO[] hand;
        public CardInstanceStateDTO[] leftLaneCards;
        public CardInstanceStateDTO[] rightLaneCards;
        public string cardInstanceWaitingForAction; // guid
    }
}
