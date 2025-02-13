using System;

namespace Assets.DTO
{
    [Serializable]
    public record HitCardDTO
    {
        public int playerID;
        public string cardInstanceID;
        public string opponentCardInstanceID;
    }
}
