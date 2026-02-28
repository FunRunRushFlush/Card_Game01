using UnityEngine;

namespace Game.Logging
{
    public interface ILogSink
    {
        void Write(LogLevel lvl, LogCat cat, string msg, Object ctx);
        void Flush() { }
    }
}
