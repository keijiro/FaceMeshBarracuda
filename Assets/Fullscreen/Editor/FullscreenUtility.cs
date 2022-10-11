using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullscreenEditor {

    /// <summary>Clone of the internal UnityEditor.ShowMode.</summary>
    public enum ShowMode {
        /// <summary>Show as a normal window with max, min & close buttons.</summary>
        NormalWindow = 0,
        /// <summary>Used for a popup menu. On mac this means light shadow and no titlebar.</summary>
        PopupMenu = 1,
        /// <summary>Utility window - floats above the app. Disappears when app loses focus.</summary>
        Utility = 2,
        /// <summary>Window has no shadow or decorations. Used internally for dragging stuff around.</summary>
        NoShadow = 3,
        /// <summary>The Unity main window. On mac, this is the same as NormalWindow, except window doesn't have a close button.</summary>
        MainWindow = 4,
        /// <summary>Aux windows. The ones that close the moment you move the mouse out of them.</summary>
        AuxWindow = 5,
        /// <summary>Like PopupMenu, but without keyboard focus.</summary>
        Tooltip = 6,
        // Show as fullscreen window
        Fullscreen = 8
    }

    /// <summary>Helper class for suppressing unity logs when calling a method that may show unwanted logs.</summary>
    internal class SuppressLog : IDisposable {

        private readonly bool lastState;

        internal static ILogger Logger {
            get {
#if UNITY_2017_1_OR_NEWER
                return Debug.unityLogger;
#else
                return Debug.logger;
#endif
            }
        }

        public SuppressLog() {
            lastState = Logger.logEnabled;
            Logger.logEnabled = false;
        }

        public void Dispose() {
            Logger.logEnabled = lastState;
        }

    }

    /// <summary>Miscellaneous utilities for Fullscreen Editor.</summary>
    [InitializeOnLoad]
    public static class FullscreenUtility {

        /// <summary>Contains a Texture2D icon loaded from a base64.</summary>
        public class Icon {

            private string m_base64;
            private Texture2D m_texture;

            public Texture2D Texture {
                get { return m_texture ? m_texture : (m_texture = FindOrLoadTexture(m_base64)); }
            }

            public Icon(string base64) {
                m_base64 = base64;
            }

            public Icon(string base64, string proVariantBase64) {
                m_base64 = EditorGUIUtility.isProSkin ? proVariantBase64 : base64;
            }

            public static implicit operator Texture2D(Icon icon) {
                return icon.Texture;
            }

        }

        private static Vector2 mousePosition;

        public static bool IsWindows { get { return Application.platform == RuntimePlatform.WindowsEditor; } }
        public static bool IsMacOS { get { return Application.platform == RuntimePlatform.OSXEditor; } }
        public static bool IsLinux { get { return Application.platform == RuntimePlatform.LinuxEditor; } }

        static FullscreenUtility() {

            var lastUpdate = EditorApplication.timeSinceStartup;

            EditorApplication.update += () => {
                if(EditorApplication.timeSinceStartup - lastUpdate > 0.5f && FullscreenPreferences.RectSource.Value == RectSourceMode.AtMousePosition) {
                    EditorApplication.RepaintHierarchyWindow();
                    EditorApplication.RepaintProjectWindow();
                    lastUpdate = EditorApplication.timeSinceStartup;
                }
            };

            EditorApplication.hierarchyWindowItemOnGUI += (rect, id) => RecalculateMousePosition();
            EditorApplication.projectWindowItemOnGUI += (rect, id) => RecalculateMousePosition();

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += sceneView => RecalculateMousePosition();
#else
            SceneView.onSceneGUIDelegate += sceneView => RecalculateMousePosition();
#endif

        }

        private static void RecalculateMousePosition() {
            mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        }

        /// <summary>Returns wheter the extension is running with debugging enabled.</summary>
        public static bool DebugModeEnabled {
            get {
#if FULLSCREEN_DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>The mouse position, can be called outside an OnGUI method.</summary>
        public static Vector2 MousePosition { get { return mousePosition; } }

        /// <summary>The icon of this plugin.</summary>
        public static readonly Icon FullscreenIcon = new Icon(
            "iVBORw0KGgoAAAANSUhEUgAAAC4AAAAuCAMAAABgZ9sFAAAApVBMVEUAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////+4/eNVAAAANnRSTlMA/AfHA2sLtBTy8MxYO/nk9u3Qj06wNCceGeCppF9LPhDot5iBewHYn1JDQS8sIyCJdkdluZF8FV+iAAAB6klEQVRIx+2SWbKbMBBFJcAYC8xswAMznme/pPe/tNASLgImVH5TeefLvhxaouuS/xTpHrwi0nJ/BZE04O2IeomrcqHnTL+RN4HLLH16qOJr/53YYiYI9pcmS/MmMdk+6OoTaLHOPErsNpqlPZ2KnIJpMzerk0x3bAq00efDujw5BuFNws+OwsCY7Md169SJfTaug/nVhsoWYFRHtkqTqZ4I6LA+e1j8qScJe8NHuB4M6+brtgJkreJmZPHty/hjkWLJpk9CmU/UJaLxk8y1RgwAcELSxbfA9gnR+A3WClGnAMAqlaC+OpE+FxcSvPGWwkbBHwXkxx1BXQ7JJ8upz1s5qS+ARAdRBvw/iDSYquSfRflDbCz4k6wyhGEUGl+Y7g9t5QlT1O4LyHlBjgAF7ltzaSx9LH1DAaenOpY+qGfPsF5pretgP3rL3G0AUDcsQOrjRdVZjDpA2bFFQ35ITxMdmmD0Jbo+yVDXezdHXcZmt31KHEA8Fx996G/283c4F+2nY7q8bNOry6MRfd3ZmVaM67OHMQ+jDHe1vF1T4ycd1SlQ07F8jLycmZQOTsdFthw0jKIFIMPTD8yGhlJpRpTvxHaKfsHux225kFcOq9rMc5yVvPGeSUR2Q6VcXk7z3888n68a+eav+AWMDWJNSUXp9QAAAABJRU5ErkJggg=="
        );

        /// <summary>A smaller icon of this plugin.</summary>
        public static readonly Icon FullscreenIconSmall = new Icon(
            "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAMAAAAMCGV4AAAAclBMVEUAAABmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmZmbn+Ue6AAAAJXRSTlMABfscDPdX3LmNgF9IKxPtoXdvQxiumWlTJPLjkYcx6M3KxrM+6Owz4AAAAKJJREFUCNdlzlkOwyAMBNDBkECALDRkb9LV979infazliz5eX4GS/Cd6wFE1/mQcWHm4qmhH4Vc5WmaGwXVBPq5pTYCmFdqxRNfPFOJwJx2sX55pK3CaCdUQwZMhtJKFtAG/5PVNzeS1wo47rWqbG+cLUHJoOLD8SY/pnijN67csl0A5bkoqBfzuki/xnTM4vHs3xlEe/aPqKd9GGoAwY1pbj58lQvfBIytyAAAAABJRU5ErkJggg==",
            "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAMAAAAMCGV4AAAAclBMVEUAAAC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLSyGfY5AAAAJXRSTlMABfsM91ccGNy5jYBfSCsT7aF3b0OumWlTJPLjkYcx6M3KxrM+ZTSY8wAAAKJJREFUCNdljtsSgyAMRJegAgIqeNdqr/n/X2ycPjYzO5Oz+3KwBp/cACC65INBz8zFy6B6FvKVF9PSKqg20I876iKAZaNOeObeM5UIzPkQrt4eea8x2Rn1aABtoColASqN/zOySbTsjQLOR6NqO2hnS1DWqPl0vEvHFO/0wY07tiugPBcFDcK8reLX6sQsPF3+SSPayz+imY9xbAAEN+Wl/QKAZAvuNVnEigAAAABJRU5ErkJggg=="
        );

        /// <summary>The icon to show on the game view toolbar when fullscreen on play is enabled.</summary>
        public static readonly Icon FullscreenOnPlayIcon = new Icon(
            "iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAMAAAAMs7fIAAAAZlBMVEUAAAAZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRnaqT0eAAAAIXRSTlMA9rSVTunBWTwgFNzOrvvYo4x1ZGA4MALTx6eae3dTCgkQ40nLAAAAcklEQVQY053OORaFIBBE0WpsBQSchz+qvf9NSmCHJlZ4g3cKj8ddbD3q6pWmSwqRnjH/SIxKQ1WN4yOkMpSuDKuhLX41ZOEt3IJxxe0mLA7Ww0LLb87NkGvDJYYk7fkPNYWK0H8G9yIqY2rzHx9ix3i6E/A3BM5M0CXHAAAAAElFTkSuQmCC",
            "iVBORw0KGgoAAAANSUhEUgAAABEAAAARCAMAAAAMs7fIAAAAY1BMVEUAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////+aRQ2gAAAAIHRSTlMA9rSV6cFZPCAU3M6uT/vYo4x1ZGBMODAJ08enmnt3U4mFYqQAAABvSURBVBjTnc43AoAwDATBkwHbOJBz1P9fiQvUUqByitXh98XG1xZlsYbhlYy5HTGdxEqkoqLEsjOJdLnJHRTd/pCQhtUwM/qPZwNmA22hIeUtpqZLte4VRRyWtIeqTITpmjC2zCJ9qNMe63wT8fce2Z8EsL04YKYAAAAASUVORK5CYII="
        );

        /// <summary>Show the notification of a newly opened <see cref="FullscreenView"/> about using the shortcut to close.</summary>
        /// <param name="window">The fullscreen window.</param>
        /// <param name="menuItemPath">The <see cref="MenuItem"/> path containing a shortcut.</param>
        internal static void ShowFullscreenExitNotification(EditorWindow window, string menuItemPath) {
            if(FullscreenPreferences.DisableNotifications)
                return;

            var notification = string.Format("Press {0} to exit fullscreen", TextifyMenuItemShortcut(menuItemPath));
            ShowFullscreenNotification(window, notification);
        }

        /// <summary>Show a fullscreen notification in an <see cref="EditorWindow"/>.</summary>
        /// <param name="window">The host of the notification.</param>
        /// <param name="message">The message to show.</param>
        public static void ShowFullscreenNotification(EditorWindow window, string message) {
            if(!window)
                return;

            window.ShowNotification(new GUIContent(message, FullscreenIcon));
            window.Repaint();

            if(EditorWindow.mouseOverWindow) // This definitely made sense when I made it, so I won't remove
                EditorWindow.mouseOverWindow.Repaint();
        }

        /// <summary>Does the given <see cref="MenuItem"/> path contains a key binding?</summary>
        public static bool MenuItemHasShortcut(string menuItemPath) {
            var index = menuItemPath.LastIndexOf(" ");

            if(index++ == -1)
                return false;

            var shortcut = menuItemPath.Substring(index).Replace("_", "");
            var evt = Event.KeyboardEvent(shortcut);

            shortcut = InternalEditorUtility.TextifyEvent(evt);

            return !shortcut.Equals("None", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Gets a human-readable shortcut.</summary>
        /// <param name="menuItemPath">The <see cref="MenuItem"/> path containing a shortcut.</param>
        /// <returns></returns>
        public static string TextifyMenuItemShortcut(string menuItemPath) {
            var index = menuItemPath.LastIndexOf(" ");

            if(index++ == -1)
                return "None";

            var shortcut = menuItemPath.Substring(index).Replace("_", "");
            var evt = Event.KeyboardEvent(shortcut);

            shortcut = InternalEditorUtility.TextifyEvent(evt);

            return shortcut;
        }

        private static Texture2D FindOrLoadTexture(string base64) {
            var found = GetRef<Texture2D>(base64);

            return found ?
                found :
                LoadTexture(base64);
        }

        private static Texture2D LoadTexture(string base64) {
            try {
                var texture = new Texture2D(0, 0, TextureFormat.ARGB32, false, true);
                var bytes = Convert.FromBase64String(base64);

                texture.name = base64;
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.LoadImage(bytes);

                return texture;
            } catch(Exception e) {
                Logger.Error("Failed to load texture: {0}", e);
                return null;
            }
        }

        /// <summary>Find an object by it's name and type.</summary>
        /// <param name="name">The name of the object to search for.</param>
        /// <typeparam name="T">The type of the object to search for.</typeparam>
        public static T GetRef<T>(string name) where T : UnityObject {
            return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(obj => obj.name == name);
        }

        /// <summary>Find an object by it's name and type.</summary>
        /// <param name="name">The name of the object to search for.</param>
        /// <param name="type">The type of the object to search for.</param> 
        public static UnityObject GetRef(Type type, string name) {
            return Resources.FindObjectsOfTypeAll(type).FirstOrDefault(obj => obj.name == name);
        }

        /// <summary>Get the main view.</summary>
        public static ScriptableObject GetMainView() {
            var containers = Resources.FindObjectsOfTypeAll(Types.MainView);

            if(containers.Length > 0)
                return containers[0] as ScriptableObject;

            throw new Exception("Couldn't find main view");
        }

        /// <summary>Get the main game view.</summary> 
        public static EditorWindow GetMainGameView() {
            if(Types.GameView.HasMethod("GetMainGameView")) { // Removed in 2019.3 alpha
                return Types.GameView.InvokeMethod<EditorWindow>("GetMainGameView");
            } else if(Types.PreviewEditorWindow.HasMethod("GetMainPreviewWindow")) { // Removed in 2019.3 beta
                return Types.PreviewEditorWindow.InvokeMethod<EditorWindow>("GetMainPreviewWindow");
            } else { // if (Types.PlayModeView.HasMethod("GetMainPlayModeView"))
                return Types.PlayModeView.InvokeMethod<EditorWindow>("GetMainPlayModeView");
            }
        }

        /// <summary>Get all the game views. This returns even the docked game views which are not visible.</summary> 
        public static EditorWindow[] GetGameViews() {
            return Resources
                .FindObjectsOfTypeAll(Types.GameView)
                .Cast<EditorWindow>()
                .ToArray();
        }

        /// <summary>Returns the focused view if it is a dock area with more than one tab, otherwise, returns the focused window.</summary> 
        public static ScriptableObject GetFocusedViewOrWindow() {
            var mostSpecificView = Types.GUIView.GetPropertyValue<ScriptableObject>("focusedView");

            if(!mostSpecificView)
                return null;

            // The most specific obj is a window, open it instead of the view
            if(mostSpecificView.IsOfType(Types.HostView, false))
                return EditorWindow.focusedWindow;

            var viewHierarchy = GetViewHierarchy(mostSpecificView);
            var leastSpecificView = viewHierarchy.LastOrDefault();

            // The view hierarchy has the same length of all the views in this ContainerWindow
            // This means there are no cousins views handled by a split group on the surroundings of this one
            // So, we're alone on the container
            var alone = leastSpecificView.GetPropertyValue<Array>("allChildren").Length == viewHierarchy.Length;

            if(alone && EditorWindow.focusedWindow.InvokeMethod<int>("GetNumTabs") > 1)
                alone = false; // But, we may not be the only tab on the host view

            // If the focused view is in the main view, or we are the only child on this view, then we open the window
            return alone || leastSpecificView.IsOfType(Types.MainView) ?
                EditorWindow.focusedWindow :
                leastSpecificView;
        }

        /// <summary>Returns all the parents of a given view.</summary>
        public static ScriptableObject[] GetViewHierarchy(ScriptableObject view) {
            if(!view)
                return new ScriptableObject[0];

            view.EnsureOfType(Types.View);

            var list = new List<ScriptableObject>() { view };
            var parent = view.GetPropertyValue<ScriptableObject>("parent");

            while(parent) { // Get the least specific view
                view = parent;
                list.Add(view);
                parent = view.GetPropertyValue<ScriptableObject>("parent");
            }

            return list.ToArray();
        }

        /// <summary>Get all the children view of a given view.</summary> 
        public static ScriptableObject[] GetAllViewChildren(ScriptableObject view) {
            if(!view)
                return new ScriptableObject[0];

            view.EnsureOfType(Types.View);

            return view.GetPropertyValue<Array>("allChildren")
                .Cast<ScriptableObject>()
                .ToArray();
        }

        /// <summary>Returns wheter a given view is focused or not.</summary> 
        public static bool IsViewFocused(ScriptableObject view) {
            if(!view)
                return false;

            view.EnsureOfType(Types.View);

            var focused = Types.GUIView.GetPropertyValue<ScriptableObject>("focusedView");
            var children = GetAllViewChildren(view);

            return children.Contains(focused);
        }

        /// <summary>Focus a view.</summary> 
        public static void FocusView(ScriptableObject guiView) {
            if(!guiView)
                return;

            // guiView.EnsureOfType(Types.GUIView);
            if(guiView.IsOfType(Types.GUIView))
                guiView.InvokeMethod("Focus");
            else {
                var vp = new ViewPyramid(guiView);
                var vc = vp.Container;
                var methodName = "Internal_BringLiveAfterCreation";

                if(vc) {
                    if(vc.HasMethod(methodName, new Type[] { typeof(bool), typeof(bool), typeof(bool) }))
                        // displayImmediately, setFocus, showMaximized
                        vc.InvokeMethod(methodName, false, true, false);
                    else
                        // displayImmediately, setFocus
                        vc.InvokeMethod(methodName, false, true);
                }
            }
        }

        /// <summary> Returns the display scaling of the editor, e.g. 1.5 if 125%</summary>
        public static float GetDisplayScaling() {
            return EditorGUIUtility.pixelsPerPoint;
        }

        /// <summary> Returns a screen rect corrected to fit the editor scaling</summary>
        public static Rect DpiCorrectedArea(Rect area) {
            var scaling = GetDisplayScaling();
            area.width /= scaling;
            area.height /= scaling;
            return area;
        }

        /// <summary>Get the default height of the editor toolbars.</summary>
        public static float GetToolbarHeight() {
            try {
                if(typeof(EditorGUI).HasField("kWindowToolbarHeight")) {
                    var result = typeof(EditorGUI).GetFieldValue<object>("kWindowToolbarHeight");
                    if(result is int)
                        return (int)result;
                    else
                        return result.GetPropertyValue<float>("value");
                } else
                    return 17f; // Default on < 2019.3 versions
            } catch(Exception e) {
                if(FullscreenUtility.DebugModeEnabled)
                    Debug.LogException(e);
                return 17f;
            }
        }

        /// <summary>Set the default height of the editor toolbars.</summary>
        public static bool SetToolbarHeight(float value) {
            try {
                // On Unity blow 2019.3 this is a const field and cannot be changed
                if(typeof(EditorGUI).HasField("kWindowToolbarHeight")) {
                    var result = typeof(EditorGUI).GetFieldValue<object>("kWindowToolbarHeight");
                    result.SetFieldValue("m_Value", value);
                    return true;
                } else {
                    return false;
                }
            } catch(Exception e) {
                if(FullscreenUtility.DebugModeEnabled)
                    Debug.LogException(e);
                return false;
            }
        }

        /// <summary>Set Game View target display.</summary>
        public static void SetGameViewDisplayTarget(EditorWindow gameView, int display) {
            gameView.EnsureOfType(Types.GameView);

            if(gameView.HasProperty("targetDisplay")) {
                gameView.SetPropertyValue("targetDisplay", display);
            } else if(gameView.HasField("m_TargetDisplay")) {
                gameView.SetFieldValue("m_TargetDisplay", display);
            } else {
                Logger.Error("Could not set Game View target display");
            }
        }

    }
}
