using System;

namespace Assets.DTO
{
    [Serializable]
    public record ListDTO<T>
    {
        public T[] items;
    }
}
