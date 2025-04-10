using System;

namespace Assets.DTO
{
    [Serializable]
    public record DeckDTO
    {
        public int id;
        public string name;
        public string avatarName;
        public CardWithCountDTO[] cards;
        public string[] attributes;
    }
}
