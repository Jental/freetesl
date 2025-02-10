using System;

namespace Assets.DTO
{
    [Serializable]
    public record MatchInformationDTO
    {
        public PlayerInformationDTO player;
        public PlayerInformationDTO opponent;
    }
}
