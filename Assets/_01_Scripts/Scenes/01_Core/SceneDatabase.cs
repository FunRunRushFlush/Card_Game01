namespace Game.Scenes.Core
{
    public static class SceneDatabase
    {
        public static class Slots
        {
            public const string Menu = "Menu";
            public const string Session = "Session";
            //public const string SessionContent = "SessionContent";
            public const string SessionView = "SessionView";

            public const string EncounterSystems = "EncounterSystems"; 
            public const string EncounterLevel = "EncounterLevel";   
        }

        public static class Scenes
        {
            public const string MainMenu = "MainMenu";
            public const string HeroSelection = "HeroSelection";


            public const string Session = "Session";

            public const string Map = "Map";
            public const string Combat = "Combat";
            public const string CombatV2 = "CombatV2";

            public const string Arena_Forest_01 = "Arena_Forest_01";
            public const string Arena_Fire_01 = "Arena_Fire_01";
            public const string Arena_Ice_01 = "Arena_Ice_01";


            public const string Arena_Forest_Boss = "Arena_Forest_Boss";

            public const string Arena_Cave_01 = "Arena_Cave_01";

            public const string Shop = "Shop";
            public const string Event = "Event";
            public const string Loot = "Loot";
            public const string GameOver = "GameOver";
        }
    }
}