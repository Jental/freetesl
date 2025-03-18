using System;

namespace Assets.DTO
{
    [Serializable]
    public record ApplyActionToCardDTO
    {
        public string cardInstanceID;
        public string opponentCardInstanceID;
    }
}
