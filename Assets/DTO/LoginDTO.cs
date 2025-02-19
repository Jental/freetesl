using System;

namespace Assets.DTO
{
    [Serializable]
    public record LoginDTO
    {
        public string login;
        public string passwordSha512;
    }
}
