using System;
using System.IO;
using UnityEngine;

namespace Game.Logging
{
    public static class LogSettingsLoader
    {
        public const string ResourcesName = "LogSettings";
        public const string JsonFileName = "logsettings.json";

        public static LogSettings LoadMergedOrNull()
        {
            var baseSettings = Resources.Load<LogSettings>(ResourcesName);
            if (baseSettings == null)
                return null;

            // Runtime clone
            var runtime = ScriptableObject.Instantiate(baseSettings);

            // Optional JSON override
            TryApplyJsonOverride(runtime);

            return runtime;
        }

        private static void TryApplyJsonOverride(LogSettings settings)
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, JsonFileName);
                if (!File.Exists(path))
                    return;

                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                var ov = JsonUtility.FromJson<LogSettingsJsonOverride>(json);
                if (ov == null)
                    return;

                ApplyOverride(settings, ov);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Logging] Failed to load JSON override: {ex.Message}");
            }
        }

        private static void ApplyOverride(LogSettings s, LogSettingsJsonOverride ov)
        {
            if (ov.overrideGlobalLevel)
                s.globalLevel = ov.globalLevel;

            if (ov.overrideCategoryLevels)
                s.overrides = ov.categoryLevels;

            if (ov.overrideOutputs)
            {
                s.logToUnityConsole = ov.logToUnityConsole;
                s.logToFile = ov.logToFile;
            }

            if (ov.overrideStacktraces)
                s.includeStacktraceForWarnings = ov.includeStacktraceForWarnings;

            if (ov.overrideFileOptions)
            {
                s.maxFileBytes = ov.maxFileBytes;
                s.maxFileCount = ov.maxFileCount;
            }
        }
    }
}
