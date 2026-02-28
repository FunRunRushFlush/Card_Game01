using System;

namespace Game.Logging
{
    [Serializable]
    public struct CategoryLevelOverride
    {
        public LogCat category;
        public LogLevel level;
    }
}
