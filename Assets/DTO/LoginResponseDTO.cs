using System;

namespace Assets.DTO
{
    [Serializable]
    public record LoginResponseDTO
    {
        public bool valid;
        public string token;
    }
}
