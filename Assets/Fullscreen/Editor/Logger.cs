using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityDebug = UnityEngine.Debug;

namespace FullscreenEditor {
    /// <summary>Helper class for logging and debugging.</summary>
    public static class Logger {

        private const string LOG_PREFIX = "Fullscreen Editor: ";

        [Conditional("FULLSCREEN_DEBUG")]
        public static void Debug(string message, params object[] args) {
            UnityDebug.LogFormat(LOG_PREFIX + message, args);
        }

        [Conditional("FULLSCREEN_DEBUG")]
        public static void Debug(Object context, string message, params object[] args) {
            UnityDebug.LogFormat(context, LOG_PREFIX + message, args);
        }

        public static void Log(string message, params object[] args) {
            UnityDebug.LogFormat(LOG_PREFIX + message, args);
        }

        public static void Log(Object context, string message, params object[] args) {
            UnityDebug.LogFormat(context, LOG_PREFIX + message, args);
        }

        public static void Warning(string message, params object[] args) {
            UnityDebug.LogWarningFormat(LOG_PREFIX + message, args);
        }

        public static void Warning(Object context, string message, params object[] args) {
            UnityDebug.LogWarningFormat(context, LOG_PREFIX + message, args);
        }

        public static void Error(string message, params object[] args) {
            UnityDebug.LogErrorFormat(LOG_PREFIX + message, args);
        }

        public static void Error(Object context, string message, params object[] args) {
            UnityDebug.LogErrorFormat(context, LOG_PREFIX + message, args);
        }

        public static void Exception(Exception exception) {
            UnityDebug.LogException(exception);
        }

        public static void Exception(Object context, Exception exception) {
            UnityDebug.LogException(exception, context);
        }

    }
}
