using System;

namespace Assets.DTO
{
    [Serializable]
    public record HitFaceDTO
    {
        public int playerID;
        public string cardInstanceID;
    }
}
