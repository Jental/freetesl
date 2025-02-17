using System;

namespace Assets.DTO
{
    [Serializable]
    public record CardDTO
    {
        public int id;
        public int power;
        public int health;
        public int cost;
        public byte type;
        public byte[] keywords;
    }
}
