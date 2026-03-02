using System;

namespace Game.Logging
{
    [Serializable]
    public struct CategoryLevelOverride
    {
        public LogArea category;
        public LogLevel level;
    }
}
