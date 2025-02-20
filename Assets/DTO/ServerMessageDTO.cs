using System;

namespace Assets.DTO
{
    [Serializable]
    public record ServerMessageDTO
    {
        public string method;
    }

    [Serializable]
    public record ServerMessageDTO<T> : ServerMessageDTO
    {
        public T body;
    }
}
