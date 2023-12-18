namespace Properties {
    public static class RoomProps {
        /// <summary>
        /// currently ongoing turn number
        /// </summary>
        public static readonly string TurnPropKey = "Turn";

        /// <summary>
        /// start (server) time for currently ongoing turn (used to calculate end)
        /// </summary>
        public static readonly string TurnStartTimePropKey = "TStart";

        /// <summary>
        /// Finished Turn of Actor (followed by number)
        /// </summary>
        public static readonly string FinishedTurnPropKey = "FToA";

        /// <summary>
        /// get which players turn it is
        /// </summary>
        public static readonly string ActivePlayerPropKey = "ActivePlayer";
    }

    public static class PlayerProps {
        /// <summary>
        /// Mana Count of player
        /// </summary>
        public static readonly string ManaPropKey = "Mana";

        /// <summary>
        /// Max Mana of player
        /// </summary>
        public static readonly string MaxManaPropKey = "MaxMana";

        /// <summary>
        /// If player has played a move
        /// </summary>
        public static readonly string hasMovedPropKey = "HasMoved";

        /// <summary>
        /// If player has attacked yet
        /// </summary>
        public static readonly string hasAttackedPropKey = "HasAttacked";

        /// <summary>
        /// If player has attacked yet
        /// </summary>
        public static readonly string hasPutInManaZonePropKey = "HasPutInManaZone";

        /// <summary>
        /// If player has attacked yet
        /// </summary>
        public static readonly string hasPlacedCardPropKey = "HasPutInManaZone";
    }
}