using System;
using UnityEditor;
using UnityEngine;

namespace FullscreenEditor.Linux {
    /// <summary>wmctrl is a tool to interact with an X Window manager available on Linux platforms.</summary>
    public static class wmctrl {

        static wmctrl() {
            try {
                var stdout = string.Empty;
                var stderr = string.Empty;
                var exitCode = Cmd.Run("which wmctrl", false, out stdout, out stderr);

                IsInstalled = exitCode == 0;
            } catch (Exception e) {
                Logger.Debug("Could not run command 'which wmctrl': {0}", e);
                IsInstalled = false;
            }
        }

        public static readonly bool IsInstalled;

        private static string Run(string format, params object[] args) {
            if (!FullscreenUtility.IsLinux)
                throw new PlatformNotSupportedException("wmctrl is only available on Linux based platforms");

            if (FullscreenPreferences.DoNotUseWmctrl.Value) {
                Logger.Debug("wmctrl being invoked while DoNotUseWmctrl is enabled");
            }

            var result = Cmd.Run("wmctrl " + format, args);
            Logger.Debug("wmctrl exited with stdio: {0}", result);
            return result;
        }

        /// <summary>Enable or disable native fullscreen for a given window.</summary>
        /// <param name="fullscreen">Should the window be fullscreen or not.</param>
        /// <param name="window">The window to changed. If null the active window will be fullscreened.</param>
        public static void SetNativeFullscreen(bool fullscreen, EditorWindow window) {
            if (window)
                window.Focus();

            Run("-r ':ACTIVE:' -b {0},fullscreen", fullscreen ? "add" : "remove");
        }

        /// <summary>Enable or disable native fullscreen for a given view.</summary>
        /// <param name="fullscreen">Should the view be fullscreen or not.</param>
        /// <param name="view">The view to changed. If null the active view will be fullscreened.</param>
        public static void SetNativeFullscreen(bool fullscreen, ScriptableObject view) {
            if (view)
                FullscreenUtility.FocusView(view);

            Run("-r ':ACTIVE:' -b {0},fullscreen", fullscreen ? "add" : "remove");
        }

        /// <summary>Toggles native fullscreen for a given window.</summary>
        /// <param name="window">The window to be toggled fullscreen.</param>
        public static void ToggleNativeFullscreen(EditorWindow window) {
            if (window)
                window.Focus();

            Run("-r ':ACTIVE:' -b toggle,fullscreen");
        }

        /// <summary>Toggles native fullscreen for a given view.</summary>
        /// <param name="view">The view to be toggled fullscreen.</param>
        public static void ToggleNativeFullscreen(ScriptableObject view) {
            if (view)
                FullscreenUtility.FocusView(view);

            Run("-r ':ACTIVE:' -b toggle,fullscreen");
        }
    }
}
