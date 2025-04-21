namespace Assets.Common
{
    public static class Constants
    {
        public const float CARD_ASPECT_RATIO = 0.6f;
        public const float HAND_CARD_OVERFLOW = 0.3f;
        public const float LANE_CARDS_GAP = 0.3f; // share of card width
        public const int MAX_HAND_CARDS = 10;
        public const int MAX_LANE_CARDS = 4;
        public const int BACKEND_POLLING_INTERVAL = 5; // in sec

        public static class MethodNames
        {
            public const string ALL_CARDS_UPDATE = "allCards";
            public const string ALL_CARD_INSTANCES_UPDATE = "allCardInstances";
            public const string MATCH_CREATE = "matchCreate";
            public const string MATCH_STATE_UPDATE = "matchStateUpdate";
            public const string MATCH_INFORMATION_UPDATE = "matchInformationUpdate";
            public const string DECK_STATE_UPDATE = "deckStateUpdate";
            public const string DISCARD_PILE_STATE_UPDATE = "discardPileStateUpdate";
            public const string END_TURN = "endTurn";
            public const string MOVE_CARD_TO_LANE = "moveCardToLane";
            public const string DRAW_CARD_TO_LANE = "drawCardToLane";
            public const string HIT_FACE = "hitFace";
            public const string HIT_CARD = "hitCard";
            public const string APPLY_CARD_TO_CARD = "applyCardToCard";
            public const string DRAW_CARD = "drawCard";
            public const string WAITED_USER_ACTIONS_COMPLETED = "waitedUserActionsCompleted";
            public const string USE_RING = "useRing";
            public const string MATCH_END = "matchEnd";
            public const string CONCEDE = "concede";
            public const string LOGIN = "login";
            public const string LOGOUT = "logout";
            public const string GET_PLAYERS = "players";
            public const string GET_CURRENT_PLAYER_INFO = "currentPlayerInfo";
            public const string GET_DECKS = "decks";
            public const string IMPORT_DECK = "decks/import";
            public const string EXPORT_DECK = "decks/export";
            public const string DELETE_DECK = "decks";
            public const string LOOKING_FOR_OPPONENT_START = "lookingForOpponentStart";
            public const string LOOKING_FOR_OPPONENT_STOP = "lookingForOpponentStop";
            public const string LOOKING_FOR_OPPONENT_STATUS = "lookingForOpponentStatus";
        }
    }
}
