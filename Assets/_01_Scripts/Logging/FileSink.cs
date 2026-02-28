using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Logging
{
    public sealed class FileSink : ILogSink
    {
        private readonly object _gate = new();
        private readonly string _dir;
        private readonly string _basePath;
        private readonly int _maxBytes;
        private readonly int _maxFiles;

        private FileStream _stream;
        private StreamWriter _writer;
        private long _currentBytes;

        // Optional: mehr/weniger flushen
        private readonly bool _flushOnError = true;

        public FileSink(LogSettings settings, string fileName = "game.log")
        {
            _maxBytes = Math.Max(64 * 1024, settings.maxFileBytes);
            _maxFiles = Math.Max(1, settings.maxFileCount);

            _dir = Path.Combine(Application.persistentDataPath, "logs");
            Directory.CreateDirectory(_dir);

            _basePath = Path.Combine(_dir, fileName);

            OpenOrCreate();
        }

        public void Write(LogLevel lvl, LogCat cat, string msg, UnityEngine.Object ctx)
        {
            if (msg == null) msg = "<null>";

            // ISO-8601 Zeitstempel + Level + Kategorie + optional Context-Name
            var ctxName = ctx ? ctx.name : "";
            var line = $"{DateTime.UtcNow:O} [{lvl}] [{cat}] {(string.IsNullOrEmpty(ctxName) ? "" : $"({ctxName}) ")}{msg}";

            // \n wird mitgeschrieben, Bytes dafür zählen
            var bytesToWrite = Encoding.UTF8.GetByteCount(line) + 1;

            lock (_gate)
            {
                EnsureOpen();

                if (_currentBytes + bytesToWrite > _maxBytes)
                    Rotate();

                _writer.WriteLine(line);
                _currentBytes += bytesToWrite;

                if (_flushOnError && lvl >= LogLevel.Error)
                    _writer.Flush();
            }
        }

        public void Flush()
        {
            lock (_gate)
            {
                if (_writer == null) return;
                _writer.Flush();
                _stream?.Flush(true);
            }
        }

        private void EnsureOpen()
        {
            if (_writer != null) return;
            OpenOrCreate();
        }

        private void OpenOrCreate()
        {
            // Append, damit Logs nicht verloren gehen
            _stream = new FileStream(_basePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = false
            };

            _currentBytes = _stream.Length;
        }

        private void Rotate()
        {
            CloseInternal();

            // Delete oldest
            var oldest = $"{_basePath}.{_maxFiles}";
            SafeDelete(oldest);

            // Shift: .(n-1) -> .n
            for (int i = _maxFiles - 1; i >= 1; i--)
            {
                var src = $"{_basePath}.{i}";
                var dst = $"{_basePath}.{i + 1}";
                if (File.Exists(src))
                    SafeMove(src, dst);
            }

            // Base -> .1
            if (File.Exists(_basePath))
                SafeMove(_basePath, $"{_basePath}.1");

            OpenOrCreate();
        }

        private void CloseInternal()
        {
            try { _writer?.Flush(); } catch { /* ignore */ }

            try { _writer?.Dispose(); } catch { /* ignore */ }
            try { _stream?.Dispose(); } catch { /* ignore */ }

            _writer = null;
            _stream = null;
            _currentBytes = 0;
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { /* ignore */ }
        }

        private static void SafeMove(string src, string dst)
        {
            try
            {
                if (File.Exists(dst))
                    File.Delete(dst);

                File.Move(src, dst);
            }
            catch { /* ignore */ }
        }
    }
}
