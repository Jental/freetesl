using System;

namespace Assets.DTO
{
    [Serializable]
    public record DeckStateUpdateDTO
    {
        public CardInstanceStateDTO[] player;
        public CardInstanceStateDTO[] opponent;
    }
}
