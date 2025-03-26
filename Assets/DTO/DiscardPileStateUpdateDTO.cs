using System;

namespace Assets.DTO
{
    [Serializable]
    public record DiscardPileStateUpdateDTO
    {
        public CardInstanceStateDTO[] player;
        public CardInstanceStateDTO[] opponent;
    }
}
