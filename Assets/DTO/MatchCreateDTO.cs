﻿using System;

namespace Assets.DTO
{
    [Serializable]
    public record MatchCreateDTO
    {
        public int opponentID;
        public int deckID;
    }
}
