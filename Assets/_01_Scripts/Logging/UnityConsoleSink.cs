using UnityEngine;

namespace Game.Logging
{
    public sealed class UnityConsoleSink : ILogSink
    {
        public void Write(LogLevel lvl, LogCat cat, string msg, Object ctx)
        {
            var line = $"[{cat}] {msg}";

            if (lvl >= LogLevel.Error)
            {
                Debug.LogError(line, ctx);
            }
            else if (lvl >= LogLevel.Warning)
            {
                Debug.LogWarning(line, ctx);
            }
            else
            {
                Debug.Log(line, ctx);
            }
        }
    }
}
