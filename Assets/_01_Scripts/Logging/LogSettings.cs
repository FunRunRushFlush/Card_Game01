using UnityEngine;

namespace Game.Logging
{
    [CreateAssetMenu(menuName = "Logging/Log Settings")]
    public class LogSettings : ScriptableObject
    {
        [Header("Levels")]
        public LogLevel globalLevel = LogLevel.Info;
        public CategoryLevelOverride[] overrides;

        [Header("Outputs")]
        public bool logToUnityConsole = true;
        public bool logToFile = false; // FileSink kommt später

        [Header("Stacktraces")]
        public bool includeStacktraceForWarnings = false;

        [Header("File options (for later FileSink)")]
        public int maxFileBytes = 2_000_000;
        public int maxFileCount = 5;

        public LogLevel GetLevel(LogCat cat)
        {
            if (overrides != null)
            {
                for (int i = 0; i < overrides.Length; i++)
                    if (overrides[i].category == cat)
                        return overrides[i].level;
            }

            return globalLevel;
        }
    }
}
