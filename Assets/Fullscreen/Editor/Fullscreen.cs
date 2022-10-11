using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleToAttribute("FullscreenTests")]

namespace FullscreenEditor {
    /// <summary>Main entry point for finding, creating and closing <see cref="FullscreenContainer"/>.</summary>
    public static class Fullscreen {

        private static FullscreenContainer[] cachedFullscreen;
        private static FullscreenContainer[] cachedFullscreenAll;

        [InitializeOnLoadMethod]
        private static void InitCache() {
            GetAllFullscreen(false, true);

            FullscreenCallbacks.beforeFullscreenOpen += (f) => GetAllFullscreen(false, true);
            FullscreenCallbacks.afterFullscreenOpen += (f) => GetAllFullscreen(false, true);
            FullscreenCallbacks.beforeFullscreenClose += (f) => GetAllFullscreen(false, true);
            FullscreenCallbacks.afterFullscreenClose += (f) => GetAllFullscreen(false, true);
        }

        /// <summary>Return all <see cref="FullscreenContainer"/> instances.</summary>
        /// <param name="cached">Allow returning cached content.</param>
        /// <param name="ignoreUnknownState">Do not return fullscreen containers that don't have a valid ContainerWindow.</param>
        public static FullscreenContainer[] GetAllFullscreen(bool cached = true, bool ignoreUnknownState = true) {

            if(cached && cachedFullscreen != null && cachedFullscreenAll != null)
                return ignoreUnknownState ?
                    cachedFullscreen :
                    cachedFullscreenAll;

            cachedFullscreenAll = Resources.FindObjectsOfTypeAll<FullscreenContainer>();

            if(!ignoreUnknownState)
                return cachedFullscreenAll;

            cachedFullscreen = cachedFullscreenAll
                .Where(fs => fs.m_dst.Container != null)
                .ToArray();

            return cachedFullscreen;
        }

        /// <summary>Get the <see cref="FullscreenContainer"/> on the given point, or null if there is none.</summary>
        public static FullscreenContainer GetFullscreenOnPoint(Vector2 point) {
            return GetAllFullscreen()
                .FirstOrDefault(fullscreen => fullscreen.Rect.Contains(point));
        }

        /// <summary>Get the <see cref="FullscreenContainer"/> that overlaps the given rect, or null if there is none.</summary>
        public static FullscreenContainer GetFullscreenOnRect(Rect rect) {
            return GetAllFullscreen()
                .FirstOrDefault(fullscreen => fullscreen.Rect.Overlaps(rect));
        }

        /// <summary>Returns the parent <see cref="FullscreenContainer"/> for a given view or window, or null if it's not in fullscreen.</summary>
        /// <param name="rootView">Compare by the root view, otherwise compare by the container.</param>
        public static FullscreenContainer GetFullscreenFromView(ScriptableObject viewOrWindow, bool rootView = true) {
            if(!viewOrWindow)
                return null;

            var pyramid = new ViewPyramid(viewOrWindow);

            return Fullscreen
                .GetAllFullscreen()
                .FirstOrDefault(fullscreen => rootView ?
                    fullscreen.ActualViewPyramid.View == pyramid.View :
                    fullscreen.ActualViewPyramid.Container == pyramid.Container
                );
        }

        /// <summary>Create a <see cref="FullscreenWindow"/> for a given window.</summary>
        /// <param name="window">The window that will go fullscreen. If null a new one will be instantiated based on the given type.</param>
        /// <typeparam name="T">The type of the window to instantiate if the given window is null.</typeparam>
        /// <returns>Returns the newly created <see cref="FullscreenWindow"/>.</returns>
        public static FullscreenWindow MakeFullscreen<T>(T window = null) where T : EditorWindow {
            return MakeFullscreen(typeof(T), window);
        }

        /// <summary>Create a <see cref="FullscreenWindow"/> for a given window.</summary>
        /// <param name="type">The type of the window to instantiate if the given window is null.</param>
        /// <param name="window">The window that will go fullscreen. If null a new one will be instantiated based on the given type.</param>
        /// <param name="disposableWindow">Set this to true when the target window was created solely for fullscreen,
        /// this will cause it to be destroyed once the fullscreen closes, it has no effects if the target window is null.</param>
        /// <returns>Returns the newly created <see cref="FullscreenWindow"/>.</returns>
        public static FullscreenWindow MakeFullscreen(Type type, EditorWindow window = null, bool disposableWindow = false) {
            var rect = FullscreenRects.GetFullscreenRect(FullscreenPreferences.RectSource, window);
            var fullscreen = ScriptableObject.CreateInstance<FullscreenWindow>();

            fullscreen.OpenWindow(rect, type, window, disposableWindow);
            return fullscreen;
        }

        /// <summary>Create a <see cref="FullscreenView"/> for a given view.</summary>
        /// <param name="view">The view that will go fullscreen, cannot be null.</param>
        /// <returns>Returns the newly created <see cref="FullscreenView"/>.</returns>
        public static FullscreenView MakeFullscreen(ScriptableObject view) {
            if(!view)
                throw new ArgumentNullException("view");

            view.EnsureOfType(Types.View);

            var rect = FullscreenRects.GetFullscreenRect(FullscreenPreferences.RectSource, view);
            var fullscreen = ScriptableObject.CreateInstance<FullscreenView>();

            fullscreen.OpenView(rect, view);

            return fullscreen;
        }

        /// <summary>Open a new fullscreen if there's none open, otherwise, close the one already open.</summary>
        /// <param name="window">The window that will go fullscreen. If null a new one will be instantiated based on the given type.</param>
        /// <typeparam name="T">The type of the window to instantiate if the given window is null.</typeparam>
        public static void ToggleFullscreen<T>(T window = null) where T : EditorWindow {
            ToggleFullscreen(typeof(T), window);
        }

        /// <summary>Open a new fullscreen if there's none open, otherwise, close the one already open.</summary>
        /// <param name="window">The window that will go fullscreen. If null a new one will be instantiated based on the given type.</param>
        /// <param name="type">The type of the window to instantiate if the given window is null.</param>
        public static void ToggleFullscreen(Type type, EditorWindow window = null) {
            var rect = FullscreenRects.GetFullscreenRect(FullscreenPreferences.RectSource, window);
            var oldFullscreen = GetFullscreenFromView(window);

            if(oldFullscreen) {
                oldFullscreen.Close();
                return;
            }

            oldFullscreen = GetFullscreenOnRect(rect);

            var newFullscreen = MakeFullscreen(type, window);

            newFullscreen.didPresent += () => {
                if(oldFullscreen)
                    oldFullscreen.Close();
            };
        }

        /// <summary>Open a new fullscreen if there's none open, otherwise, close the one already open.</summary>
        /// <param name="view">The view that will go fullscreen, cannot be null.</param>
        public static void ToggleFullscreen(ScriptableObject view) {
            var rect = FullscreenRects.GetFullscreenRect(FullscreenPreferences.RectSource, view);
            var oldFullscreen = GetFullscreenFromView(view);

            if(oldFullscreen) {
                oldFullscreen.Close();
                return;
            }

            oldFullscreen = GetFullscreenOnRect(rect);

            var newFullscreen = MakeFullscreen(view);

            newFullscreen.didPresent += () => {
                if(oldFullscreen)
                    oldFullscreen.Close();
            };
        }

    }
}
