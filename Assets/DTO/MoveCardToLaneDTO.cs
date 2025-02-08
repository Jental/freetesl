using System;

namespace Assets.DTO
{
    [Serializable]
    public record MoveCardToLaneDTO
    {
        public int playerID;
        public string cardInstanceID;
        public byte laneID;
    }
}
