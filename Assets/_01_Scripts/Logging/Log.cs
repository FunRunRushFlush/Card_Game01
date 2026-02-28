using System;
using System.Diagnostics;
using UnityEngine;

namespace Game.Logging
{
    public static class Log
    {
        private static LogSettings _settings;
        private static ILogSink[] _sinks;

        public static bool IsInitialized => _settings != null;

        public static void Initialize(LogSettings settings, params ILogSink[] sinks)
        {
            _settings = settings;
            _sinks = sinks ?? Array.Empty<ILogSink>();
            ApplyStackTracePolicy(_settings);
        }

        public static bool IsEnabled(LogCat cat, LogLevel lvl)
            => _settings != null && lvl >= _settings.GetLevel(cat);

        public static void Info(LogCat cat, Func<string> msg, UnityEngine.Object ctx = null)
            => Write(LogLevel.Info, cat, msg, ctx);

        public static void Warn(LogCat cat, Func<string> msg, UnityEngine.Object ctx = null)
            => Write(LogLevel.Warning, cat, msg, ctx);

        public static void Error(LogCat cat, Func<string> msg, UnityEngine.Object ctx = null)
            => Write(LogLevel.Error, cat, msg, ctx);

        // Debug/Trace optional compile-time:
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("LOG_DEBUG")]
        public static void Debug(LogCat cat, Func<string> msg, UnityEngine.Object ctx = null)
            => Write(LogLevel.Debug, cat, msg, ctx);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("LOG_DEBUG")]
        public static void Trace(LogCat cat, Func<string> msg, UnityEngine.Object ctx = null)
            => Write(LogLevel.Trace, cat, msg, ctx);

        private static void Write(LogLevel lvl, LogCat cat, Func<string> msg, UnityEngine.Object ctx)
        {
            // Falls jemand vor Init loggt, nicht crashen:
            if (_settings == null || _sinks == null || _sinks.Length == 0)
            {
                // minimal fallback (ohne fancy format):
                if (lvl >= LogLevel.Error) UnityEngine.Debug.LogError(msg?.Invoke(), ctx);
                else if (lvl >= LogLevel.Warning) UnityEngine.Debug.LogWarning(msg?.Invoke(), ctx);
                else UnityEngine.Debug.Log(msg?.Invoke(), ctx);
                return;
            }

            if (!IsEnabled(cat, lvl)) return;

            // Lazy eval -> keine String Allocations wenn disabled
            var text = msg?.Invoke() ?? "<null>";

            for (int i = 0; i < _sinks.Length; i++)
                _sinks[i].Write(lvl, cat, text, ctx);
        }

        private static void ApplyStackTracePolicy(LogSettings s)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(
                LogType.Warning,
                s.includeStacktraceForWarnings ? StackTraceLogType.ScriptOnly : StackTraceLogType.None
            );
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
        }

        public static void Flush()
        {
            if (_sinks == null) return;
            for (int i = 0; i < _sinks.Length; i++)
                _sinks[i].Flush();
        }
    }
}
