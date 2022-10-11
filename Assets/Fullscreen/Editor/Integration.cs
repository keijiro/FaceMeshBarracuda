using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FullscreenEditor {
    /// <summary>Helper class for enabling/disabling compilation symbols.</summary>
    public static class Integration {

        private static string[] GetAllDefines() {
            var currentBuildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTarget);
            var split = scriptDefines.Split(new [] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return split;
        }

        private static void SetAllDefines(string[] value) {
            var currentBuildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var currentScriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentBuildTarget);
            var scriptDefines = value.Length > 0 ?
                value.Aggregate((a, b) => a + ";" + b) :
                string.Empty;

            if (currentScriptDefines == scriptDefines)
                return; // Nothing has changed

            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTarget, scriptDefines);

            RequestScriptReload();
        }

        public static void RequestScriptReload() {
            if (typeof(EditorUtility).HasMethod("RequestScriptReload")) {
                typeof(EditorUtility).InvokeMethod("RequestScriptReload");
            }
            if (typeof(InternalEditorUtility).HasMethod("RequestScriptReload")) {
                typeof(InternalEditorUtility).InvokeMethod("RequestScriptReload");
            } else {
                Logger.Error("Could not reload scripts");
            }

        }

        /// <summary>Toggle a given define symbol.</summary>
        /// <param name="directive">The define symbol to toggle.</param>
        public static void ToggleDirectiveDefined(string directive) {
            var defined = IsDirectiveDefined(directive);
            SetDirectiveDefined(directive, !defined);
        }

        /// <summary>Enable or disable a given define symbol.</summary>
        /// <param name="directive">The define symbol to set.</param>
        /// <param name="enabled">Wheter to enable or disable this directive.</param>
        public static void SetDirectiveDefined(string directive, bool enabled) {
            if (IsDirectiveDefined(directive) == enabled)
                return; // Flag already enabled/disabled

            if (enabled)
                SetAllDefines(GetAllDefines()
                    .Concat(new [] { directive })
                    .ToArray()
                );
            else
                SetAllDefines(GetAllDefines()
                    .Where(d => d != directive)
                    .ToArray()
                );

            Logger.Debug("Compiler directive {0} {1} defined", directive, enabled? "": "not");
        }

        /// <summary>Get wheter the given directive is enabled or not.</summary>
        /// <param name="directive">The name of the define symbol to check.</param> 
        public static bool IsDirectiveDefined(string directive) {
            return GetAllDefines().Any(d => d == directive);
        }

    }
}
