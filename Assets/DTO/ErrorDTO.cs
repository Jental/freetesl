using System;

namespace Assets.DTO
{
    [Serializable]
    public record ErrorDTO
    {
        public int errorCode;
        public string message;
    }
}
