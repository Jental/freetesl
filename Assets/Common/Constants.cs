namespace Assets.Common
{
    public static class Constants
    {
        public const float CARD_ASPECT_RATIO = 0.6f;
        public const float HAND_CARD_OVERFLOW = 0.3f;
        public const float LANE_CARDS_GAP = 0.3f; // share of card width
        public const int MAX_HAND_CARDS = 10;
        public const int MAX_LANE_CARDS = 4;
        public const byte LEFT_LANE_ID = 0;
        public const byte RIGHT_LANE_ID = 1;
        public const int BACKEND_POLLING_INTERVAL = 5; // in sec

        public static class MethodNames
        {
            public const string ALL_CARDS_UPDATE = "allCards";
            public const string ALL_CARD_INSTANCES_UPDATE = "allCardInstances";
            public const string MATCH_CREATE = "matchCreate";
            public const string MATCH_STATE_UPDATE = "matchStateUpdate";
            public const string MATCH_INFORMATION_UPDATE = "matchInformationUpdate";
            public const string END_TURN = "endTurn";
            public const string MOVE_CARD_TO_LANE = "moveCardToLane";
            public const string HIT_FACE = "hitFace";
            public const string HIT_CARD = "hitCard";
            public const string APPLY_ACTION_TO_CARD = "applyActionToCard";
            public const string MATCH_END = "matchEnd";
            public const string CONCEDE = "concede";
            public const string LOGIN = "login";
            public const string LOGOUT = "logout";
            public const string GET_PLAYERS = "players";
            public const string GET_CURRENT_PLAYER_INFO = "currentPlayerInfo";
            public const string LOOKING_FOR_OPPONENT_START = "lookingForOpponentStart";
            public const string LOOKING_FOR_OPPONENT_STOP = "lookingForOpponentStop";
            public const string LOOKING_FOR_OPPONENT_STATUS = "lookingForOpponentStatus";
        }
    }
}
