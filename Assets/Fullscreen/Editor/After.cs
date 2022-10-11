using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace FullscreenEditor {
    /// <summary>Utility class for running async tasks within the main thread.</summary>
    public static class After {

        /// <summary>Wait for a condition to become true, then executes the callback.</summary>
        /// <param name="condition">Function that will be called every frame that returns whether to invoke the callback or not.</param>
        /// <param name="callback">The callback to be called when the condition becomes true.</param>
        /// <param name="timeoutMs">Maximum time to wait in milliseconds before cancelling the callback.</param>
        public static void Condition(Func<bool> condition, Action callback, double timeoutMs = 0d) {
            var update = new EditorApplication.CallbackFunction(() => { });
            var timeoutsAt = EditorApplication.timeSinceStartup + (timeoutMs / 1000d);
            var stack = new StackFrame(1, true);

            update = () => {
                if (timeoutMs > 0d && EditorApplication.timeSinceStartup >= timeoutsAt) {
                    EditorApplication.update -= update;
                    Logger.Error("Condition timedout at {0}:{1}", stack.GetFileName(), stack.GetFileLineNumber());
                    return;
                }

                if (condition()) {
                    EditorApplication.update -= update;
                    callback();
                }
            };

            EditorApplication.update += update;
        }

        /// <summary>Wait for the given amount of editor frames, then executes the callback.</summary>
        /// <param name="frames">The number of frames to wait for.</param>
        /// <param name="callback">The callback to be called after the specified frames.</param>
        public static void Frames(int frames, Action callback) {
            var f = 0;
            Condition(() => f++ >= frames, callback);
        }

        /// <summary>Wait for the given time, then executes the callback.</summary>
        /// <param name="milliseconds">How long to wait until calling the callback, in milliseconds.</param>
        /// <param name="callback">The callback to be called after the specified time.</param>
        public static void Milliseconds(double milliseconds, Action callback) {
            var end = EditorApplication.timeSinceStartup + (milliseconds / 1000f);
            Condition(() => EditorApplication.timeSinceStartup >= end, callback);
        }

    }
}
