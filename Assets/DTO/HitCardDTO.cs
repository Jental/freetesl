using System;

namespace Assets.DTO
{
    [Serializable]
    public record HitCardDTO
    {
        public string cardInstanceID;
        public string opponentCardInstanceID;
    }
}
