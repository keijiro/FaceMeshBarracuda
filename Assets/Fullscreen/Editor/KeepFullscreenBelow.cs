using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FullscreenEditor {
    internal static class KeepFullscreenBelow {
        [InitializeOnLoadMethod]
        private static void InitPatch() {
            var eApp = typeof(EditorApplication);
            var callback = eApp.GetFieldValue<EditorApplication.CallbackFunction>("windowsReordered");
            callback += () => BringWindowsAbove();
            eApp.SetFieldValue("windowsReordered", callback);
            FullscreenCallbacks.afterFullscreenOpen += (f) => BringWindowsAbove();
        }

        // https://github.com/mukaschultze/fullscreen-editor/issues/54
        // This is needed because ContainerWindows created by ShowAsDropDown are not
        // returned by 'windows' property
        public static IEnumerable<ScriptableObject> GetAllContainerWindowsOrdered() {
            var ordered = Types.ContainerWindow
                .GetPropertyValue<ScriptableObject[]>("windows")
                .Reverse();

            var missing = Resources
                .FindObjectsOfTypeAll(Types.ContainerWindow)
                .Select(cw => cw as ScriptableObject);

            return ordered
                .Concat(missing)
                .Distinct();
        }

        public static void BringWindowsAbove() {

            if (!FullscreenPreferences.KeepFullscreenBelow)
                return;

            var fullscreens = Fullscreen.GetAllFullscreen();
            if (fullscreens.Length == 0)
                return;

            var methodName = "Internal_BringLiveAfterCreation";
            var windows = GetAllContainerWindowsOrdered()
                .Where(w => !Fullscreen.GetFullscreenFromView(w))
                .Where(w => {
                    if (w.GetPropertyValue<int>("showMode") == (int)ShowMode.MainWindow)
                        return false; // Main Window should be kept below everything

                    if (fullscreens.FirstOrDefault((f) => f.m_src.Container == w))
                        return false; // Keep other fullscreen containers below

                    return true;
                });

            foreach (var w in windows) {
                if (w.HasMethod(methodName, new Type[] { typeof(bool), typeof(bool), typeof(bool) }))
                    w.InvokeMethod(methodName, true, false, false);
                else
                    w.InvokeMethod(methodName, true, false);
            }
        }
    }
}
