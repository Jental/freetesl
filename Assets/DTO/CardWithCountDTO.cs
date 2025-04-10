using System;

namespace Assets.DTO
{
    [Serializable]
    public record CardWithCountDTO
    {
        public int cardID;
        public string cardName;
        public int count;
    }
}
