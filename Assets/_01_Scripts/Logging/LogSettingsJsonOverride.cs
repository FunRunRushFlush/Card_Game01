using System;

namespace Game.Logging
{
    [Serializable]
    public class LogSettingsJsonOverride
    {
        public int version = 1;

        // Global level
        public bool overrideGlobalLevel;
        public LogLevel globalLevel;

        // Category overrides
        public bool overrideCategoryLevels;
        public CategoryLevelOverride[] categoryLevels;

        // Outputs
        public bool overrideOutputs;
        public bool logToUnityConsole;
        public bool logToFile;

        // Stacktraces
        public bool overrideStacktraces;
        public bool includeStacktraceForWarnings;

        // File options (for later)
        public bool overrideFileOptions;
        public int maxFileBytes;
        public int maxFileCount;
    }
}
