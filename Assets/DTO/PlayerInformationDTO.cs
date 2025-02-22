using System;

namespace Assets.DTO
{
    [Serializable]
    public record PlayerInformationDTO
    {
        public int id;
        public string name;
        public string avatarName;
        public byte state;
    }
}
