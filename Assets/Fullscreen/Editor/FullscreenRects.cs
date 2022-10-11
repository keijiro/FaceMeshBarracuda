using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

using FullscreenEditor.Windows;

namespace FullscreenEditor {
    /// <summary>Helper for getting fullscreen rectangles.</summary>
    public static class FullscreenRects {

        /// <summary>Represents a callback for user defined fullscreen rect calculation.</summary>
        /// <param name="mode">The mode set in <see cref="FullscreenPreferences.RectSource"/></param>
        /// <param name="rect">A rect calculated based on custom logic.</param>
        /// <returns>Whether the rect calculated should be used or not.</returns>
        public delegate bool FullscreenRectCallback(RectSourceMode mode, out Rect rect);

        /// <summary>The number of monitors attached to this machine, returns -1 if the platform is not supported.</summary>
        public static int ScreenCount {
            get {
                if (!FullscreenUtility.IsWindows)
                    return -1;
                const int SM_CMONITORS = 80;
                return User32.GetSystemMetrics(SM_CMONITORS);
            }
        }

        /// <summary>Custom callback to allow the user to specify their own logic to how fullscreens will be arranged.
        /// Check the documentation for usage examples.</summary>
        public static FullscreenRectCallback CustomRectCallback { get; set; }

        /// <summary>Returns a fullscreen rect</summary>
        /// <param name="mode">The mode that will be used to retrieve the rect.</param>
        /// <param name="targetWindow">The window that will be set fullscreen.</param>
        public static Rect GetFullscreenRect(RectSourceMode mode, ScriptableObject targetWindow = null) {

            if (targetWindow != null && !targetWindow.IsOfType(typeof(EditorWindow)) && !targetWindow.IsOfType(Types.View)) {
                throw new ArgumentException("Target window must be of type EditorWindow or View or null", "targetWindow");
            }

            if (CustomRectCallback != null) {
                var rect = new Rect();
                var shouldUse = CustomRectCallback(mode, out rect);

                if (shouldUse)
                    return rect;
            }

            switch (mode) {
                case RectSourceMode.MainDisplay:
                    return GetMainDisplayRect();

                case RectSourceMode.WindowDisplay:
                    if (targetWindow == null || !FullscreenUtility.IsWindows)
                        return GetMainDisplayRect();

                    var views = new ViewPyramid(targetWindow);
                    var rect = views.Container.GetPropertyValue<Rect>("position");

                    return GetDisplayBoundsAtPoint(rect.center);

                case RectSourceMode.AtMousePosition:
                    return FullscreenUtility.IsWindows ?
                        GetDisplayBoundsAtPoint(FullscreenUtility.MousePosition) :
                        GetWorkAreaRect(true);

                case RectSourceMode.Span:
                    return FullscreenUtility.IsWindows ?
                        GetVirtualScreenBounds() :
                        GetWorkAreaRect(true);

                case RectSourceMode.Custom:
                    return GetCustomUserRect();

                case RectSourceMode.Display1:
                    return GetMonitorRect(0);
                case RectSourceMode.Display2:
                    return GetMonitorRect(1);
                case RectSourceMode.Display3:
                    return GetMonitorRect(2);
                case RectSourceMode.Display4:
                    return GetMonitorRect(3);
                case RectSourceMode.Display5:
                    return GetMonitorRect(4);
                case RectSourceMode.Display6:
                    return GetMonitorRect(5);
                case RectSourceMode.Display7:
                    return GetMonitorRect(6);
                case RectSourceMode.Display8:
                    return GetMonitorRect(7);

                default:
                    Logger.Warning("Invalid fullscreen mode, please fix this by changing the placement source mode in preferences.");
                    return new Rect(Vector2.zero, Vector2.one * 300f);
            }
        }

        /// <summary>Returns a rect with the dimensions of the main screen.
        /// (Note that the position may not be right for multiple screen setups)</summary>
        public static Rect GetMainDisplayRect() {

            if (FullscreenUtility.IsWindows) {
                var mainDisplay = DisplayInfo
                    .GetDisplays()
                    .FirstOrDefault(d => d.PrimaryDisplay);

                if (mainDisplay != null)
                    return mainDisplay.UnityCorrectedArea;

                Logger.Error("No main display??? This should not happen, falling back to Screen.currentResolution");
            }

            // Screen.currentResolution returns the resolution of the screen where
            // the currently focused window is located, not the main display resolution. 
            // This caused the bug #53 on windows.
            // The same behaviour was not tested on Linux as macOS
            return new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height);
        }

        /// <summary>Returns the rect of a given display index.</summary>
        public static Rect GetMonitorRect(int index) {

            if (!FullscreenUtility.IsWindows)
                return GetMainDisplayRect();

            var d = DisplayInfo.GetDisplay(index);

            if (d == null) {
                Logger.Error("Display {0} not connected", index + 1);
                return GetMainDisplayRect();
            }

            return d.UnityCorrectedArea;
        }

        /// <summary>Returns a rect defined by the user in the preferences.</summary>
        public static Rect GetCustomUserRect() {
            return FullscreenPreferences.CustomRect;
        }

        /// <summary>Returns a rect covering all the screen, except for the taskbar/dock.
        /// On Windows it adds a 4px border and does not account for scaling (can cause bugs when using scales different than 100%).
        /// On macOS this returns a fullscreen rect when the main window is maximized and mouseScreen is set to true.</summary>
        /// <param name="mouseScreen">Should we get the rect on the screen where the mouse pointer is?</param>
        public static Rect GetWorkAreaRect(bool mouseScreen) {
            return Types.ContainerWindow.InvokeMethod<Rect>("FitRectToScreen", new Rect(Vector2.zero, Vector2.one * 10000f), true, mouseScreen);
        }

        /// <summary>Returns a rect covering all the screen, except for the taskbar/dock.
        /// On Windows it adds a 4px border and does not account for scaling (can cause bugs when using scales different than 100%).
        /// On macOS this returns a fullscreen rect when the main window is maximized and mouseScreen is set to true.</summary>
        /// <param name="container">The ContainerWindow that will be used as reference for calulating border error.</param>
        /// <param name="mouseScreen">Should we get the rect on the screen where the mouse pointer is?</param>
        public static Rect GetWorkAreaRect(Object container, bool mouseScreen) {
            return container.InvokeMethod<Rect>("FitWindowRectToScreen", new Rect(Vector2.zero, Vector2.one * 10000f), true, mouseScreen);
        }

        /// <summary>Returns the bounds rect of the screen that contains the given point. (Windows only)</summary>
        /// <param name="point">The point relative to <see cref="RectSourceMode.Span"/></param>
        public static Rect GetDisplayBoundsAtPoint(Vector2 point) {
            return InternalEditorUtility.GetBoundsOfDesktopAtPoint(point);
        }

        /// <summary>Full virtual screen bounds, spanning across all monitors. (Windows only)</summary>
        public static Rect GetVirtualScreenBounds() {

            if (!FullscreenUtility.IsWindows)
                throw new NotImplementedException();

            const int SM_XVIRTUALSCREEN = 76;
            const int SM_YVIRTUALSCREEN = 77;
            const int SM_CXVIRTUALSCREEN = 78;
            const int SM_CYVIRTUALSCREEN = 79;

            var x = User32.GetSystemMetrics(SM_XVIRTUALSCREEN);
            var y = User32.GetSystemMetrics(SM_YVIRTUALSCREEN);
            var width = User32.GetSystemMetrics(SM_CXVIRTUALSCREEN);
            var height = User32.GetSystemMetrics(SM_CYVIRTUALSCREEN);

            var rect = new Rect {
                yMin = y,
                xMin = x,
                width = width,
                height = height,
            };

            return FullscreenUtility.DpiCorrectedArea(rect);
        }

    }
}
