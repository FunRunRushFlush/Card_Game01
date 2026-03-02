using UnityEngine;

namespace Game.Logging
{
    public interface ILogSink
    {
        void Write(LogLevel lvl, LogArea cat, string msg, Object ctx);
        void Flush() { }
    }
}
