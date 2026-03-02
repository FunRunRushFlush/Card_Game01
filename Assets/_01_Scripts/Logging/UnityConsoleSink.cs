using UnityEngine;

namespace Game.Logging
{
    public sealed class UnityConsoleSink : ILogSink
    {
        public void Write(LogLevel lvl, LogArea cat, string msg, Object ctx)
        {
            string ctxPart = "";
            if (ctx != null)
            {
                ctxPart = $"[{ctx.GetType().Name}:{ctx.name}] ";
            }

            var line = $"[{cat}] {ctxPart}{msg}";

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
