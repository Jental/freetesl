using System;

namespace Assets.DTO
{
    [Serializable]
    public record PlayerMatchStateDTO
    {
        public CardInstanceDTO[] deck;
        public CardInstanceDTO[] hand;
    }
}
