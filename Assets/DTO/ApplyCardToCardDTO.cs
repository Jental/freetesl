using System;

namespace Assets.DTO
{
    [Serializable]
    public record ApplyCardToCardDTO
    {
        public string cardInstanceID;
        public string opponentCardInstanceID;
    }
}
