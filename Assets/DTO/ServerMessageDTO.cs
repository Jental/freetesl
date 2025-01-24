using System;

namespace Assets.DTO
{
    [Serializable]
    public record ServerMessageDTO<T>
    {
        public string method;
        public T body;
    }
}
