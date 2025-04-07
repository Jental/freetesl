using System;

namespace Assets.DTO
{
    [Serializable]
    public record MatchStateDTO
    {
        public PlayerMatchStateDTO player;
        public PlayerMatchStateDTO opponent;
        public bool ownTurn;
        public bool waitingForOtherPlayerAction;
    }
}
