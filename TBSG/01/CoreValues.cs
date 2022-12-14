namespace EEANWorks.Games.TBSG._01
{
    public static class CoreValues
    {
        public const string GAME_VERSION = "0.5.2";

        //public const string SERVER_URL = "https://www.eeangames.com:8180/tbsg-01-game-server/MainServlet";
        public const string SERVER_URL = "http://eeangames:8180/tbsg-01-game-server/MainServlet";

        public const int FRAMES_BEFORE_REUPDATING_CONNECTION = 25;

        public const string SESSION_ERROR_MESSAGE = "Your session has expired due to inactivity!"
                                                    + "\nPlease log in again.";

        public const float BUTTON_MAX_SECONDS_FOR_DOUBLE_CLICK = 0.5f;
        public const float BUTTON_SECONDS_FOR_LONG_PRESS = 1f;

        public const decimal DEFAULT_CRITICAL_RATE = 0.05m;
        public const int MIN_SP_COST = 1;
        public const int MAX_SP = 10;
        //public const int MIN_BASE_ATTRIBUTE_VALUE = 1;
        //public const int MAX_BASE_VIT_VALUE = 99;
        public const int MAX_BASE_STR_AND_RES_VALUE = 999;
        //public const int MAX_BASE_HP_VALUE = 9999;
        public const int REQUIRED_EXPERIENCE_FOR_FIRST_LEVEL_UP = 9;
        public const int SIZE_OF_A_SIDE_OF_BOARD = 7;
        public const int MAX_MEMBERS_PER_TEAM = 5;
        public const int MAX_LEGENDARY_ITEMS_PER_TEAM = 1;
        public const int MAX_EPIC_ITEMS_PER_TEAM = 1;
        public const int MAX_RARE_ITEMS_PER_TEAM = 3;
        public const int MAX_UNCOMMON_ITEMS_PER_TEAM = 5;
        public const int MAX_COMMON_ITEMS_PER_TEAM = 10;
        public const int MAX_ITEMS_PER_TEAM = MAX_LEGENDARY_ITEMS_PER_TEAM + MAX_EPIC_ITEMS_PER_TEAM + MAX_RARE_ITEMS_PER_TEAM + MAX_UNCOMMON_ITEMS_PER_TEAM + MAX_COMMON_ITEMS_PER_TEAM;
        public const int DAMAGE_BASE_VALUE = 50;
        public const int MAX_NUM_OF_ELEMENTS_IN_RECIPE = 5;
        public const int LEVEL_DIFFERENCE_BETWEEN_RARITIES = 20;
        public const int MAX_SKILL_LEVEL = 10;
        public const decimal MULTIPLIER_FOR_ELEMENT_MATCH = 1.2m;
        public const decimal MULTIPLIER_FOR_EFFECTIVE_ELEMENT = 1.2m;
        public static readonly decimal MULTIPLIER_FOR_INEFFECTIVE_ELEMENT = (1.2m).Reciprocal();
        public const decimal MULTIPLIER_FOR_CRITICALHIT = 2.0m;
        public const decimal MULTIPLIER_FOR_TILETYPEMATCH = 1.2m;
        public const decimal LEVEL_EXPERIENCE_MULTIPLIER = 1.0294118m; // This value allows the max accumulated experience value to be the closest to 5,000,000. The value would be 5,050,601. 
        public const decimal POW_ADJUSTMENT_CONST_D = 19m / 99m;

        public const decimal MULTIPLIER_FOR_BONUS_ELEMENT_MATCHING_UNIT = 2m;
        public const int OBJECT_ENHANCEMENT_COST_MULTIPLIER = 100;
    }   
}
