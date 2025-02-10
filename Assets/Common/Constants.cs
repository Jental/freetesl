namespace Assets.Common
{
    public static class Constants
    {
        public const float CARD_ASPECT_RATIO = 0.6f;
        public const float HAND_CARD_OVERFLOW = 0.3f;
        public const float LANE_CARDS_GAP = 15;
        public const int TEST_PLAYER_ID = 1;
        public const byte LEFT_LANE_ID = 0;
        public const byte RIGHT_LANE_ID = 1;

        public static class MethodNames
        {
            public const string MATCH_STATE_UPDATE = "matchStateUpdate";
            public const string MATCH_INFORMATION_UPDATE = "matchInformationUpdate";
            public const string MATCH_JOIN = "join";
            public const string END_TURN = "endTurn";
            public const string MOVE_CARD_TO_LANE = "moveCardToLane";
        }
    }
}
