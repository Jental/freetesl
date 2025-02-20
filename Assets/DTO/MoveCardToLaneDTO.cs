using System;

namespace Assets.DTO
{
    [Serializable]
    public record MoveCardToLaneDTO
    {
        public string cardInstanceID;
        public byte laneID;
    }
}
