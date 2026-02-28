using System.Collections.Generic;
using UnityEngine;

namespace Game.Logging
{
    public static class LogBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var settings = LogSettingsLoader.LoadMergedOrNull();
            if (settings == null)
            {
                Debug.LogWarning("[Logging] No LogSettings found in Resources.");
                return;
            }

            var sinks = new List<ILogSink>(4);

            if (settings.logToUnityConsole)
                sinks.Add(new UnityConsoleSink());

            if (settings.logToFile)
                sinks.Add(new FileSink(settings));

            Log.Initialize(settings, sinks.ToArray());

            Log.Info(LogCat.General, () => $"Logging initialized. GlobalLevel={settings.globalLevel}", null);
        }
    }
}
