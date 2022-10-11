using UnityEditor;
using UnityEngine;

namespace FullscreenEditor {
    public class DisableSceneView {

        // TODO: Patcher resets the method to original after a while
        // TODO: or the shouldSkipRender is not being properly calculated
        private static Patcher patcher = null;

        public static bool RenderingDisabled {
            get {
                return patcher == null ? false : patcher.IsPatched();
            }
            set {
                if(patcher == null) return;

                if(value == patcher.IsPatched())
                    return;
                else if(!value)
                    patcher.Revert();
                else if(FullscreenPreferences.DisableSceneViewRendering)
                    patcher.SwapMethods();

                if(!value || FullscreenPreferences.DisableSceneViewRendering)
                    foreach(var c in SceneView.GetAllSceneCameras())
                        c.gameObject.SetActive(!value);

                Logger.Debug("{0} Scene View Rendering", value ? "Disabled" : "Enabled");
                SceneView.RepaintAll();
            }
        }

        [InitializeOnLoadMethod]
        private static void Init() {
            if(!Patcher.IsSupported()) return;

            var sceneGUIName = ReflectionUtility.HasMethod(typeof(SceneView), "OnSceneGUI") ? "OnSceneGUI" : "OnGUI";

            patcher = new Patcher(
                    typeof(SceneView).FindMethod(sceneGUIName), // Original method
                    typeof(DisableSceneView).FindMethod("OnGUI") // Replacement
                );

            SceneView.beforeSceneGui += OnBeforeSceneGUI;

            // Initial
            RenderingDisabled = Fullscreen.GetAllFullscreen().Length > 0;

            // On preferences change
            FullscreenPreferences.DisableSceneViewRendering.OnValueSaved += (v) =>
                RenderingDisabled = v && Fullscreen.GetAllFullscreen().Length > 0;

            // On fullscreen open
            FullscreenCallbacks.afterFullscreenOpen += (f) =>
                RenderingDisabled = true;

            // Disable the patching if we're the last fullscreen open
            FullscreenCallbacks.afterFullscreenClose += (f) => {
                if(Fullscreen.GetAllFullscreen().Length <= 1)
                    RenderingDisabled = false;
            };
        }

        private static void OnBeforeSceneGUI(SceneView sceneView) {
            var shouldRender = !RenderingDisabled || Fullscreen.GetFullscreenFromView(new ViewPyramid(sceneView).Container, false);
            sceneView.autoRepaintOnSceneChange = shouldRender;
        }

        // This should not be a static method, as static has no this
        // However, the 'this' in the method is unreliable and should be casted before using
        private void OnGUI() {
            var _this = (object)this as SceneView;
            var vp = new ViewPyramid(_this);
            var shouldRender = Fullscreen.GetFullscreenFromView(vp.Container, false); // Render if this window is in fullscreen

            if(shouldRender) {
                // Do this outside of OnGUI to prevent controls count errors
                After.Frames(1, () => EnableRenderingTemporarily());
            } else {
                _this.autoRepaintOnSceneChange = false;
                CustomOnGUI();
            }
        }

        private static class Styles {

            public static readonly GUIStyle textStyle = new GUIStyle("BoldLabel");
            public static readonly GUIStyle backgroundShadow = new GUIStyle("InnerShadowBg");
            public static readonly GUIStyle buttonStyle = new GUIStyle("LargeButton");
            public static readonly GUIStyle secondaryTextStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);

            static Styles() {
                textStyle.wordWrap = true;
                textStyle.alignment = TextAnchor.MiddleCenter;
            }

        }

        private void CustomOnGUI() {
            using(var mainScope = new EditorGUILayout.VerticalScope(Styles.backgroundShadow)) {
                using(new GUIColor(Styles.textStyle.normal.textColor * 0.05f))
                    GUI.DrawTexture(mainScope.rect, FullscreenUtility.FullscreenIcon, ScaleMode.ScaleAndCrop);

                GUILayout.FlexibleSpace();

                using(new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();

                    using(new GUIContentColor(Styles.textStyle.normal.textColor))
                        GUILayout.Label(FullscreenUtility.FullscreenIcon, Styles.textStyle);

                    using(new EditorGUILayout.VerticalScope()) {
                        GUILayout.Label("Scene View rendering has been disabled\nto improve fullscreen performance", Styles.textStyle);
                    }

                    GUILayout.FlexibleSpace();
                }

                using(new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button("Enable Temporarily", Styles.buttonStyle)) {
                        // Do this outside of OnGUI to prevent controls count errors
                        After.Frames(1, () => EnableRenderingTemporarily());
                        var _this = (object)this as SceneView;
                        _this.Focus();
                    }
                    if(GUILayout.Button("Enable Permanently", Styles.buttonStyle)) {
                        // Do this outside of OnGUI to prevent controls count errors
                        After.Frames(1, () => EnableRenderingPermanently());
                        var _this = (object)this as SceneView;
                        _this.Focus();
                    }
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Label("* This can be changed later in the preferences", Styles.secondaryTextStyle);
                GUILayout.FlexibleSpace();
            }
        }

        public static void EnableRenderingTemporarily() {
            RenderingDisabled = false;
        }

        public static void EnableRenderingPermanently() {
            FullscreenPreferences.DisableSceneViewRendering.Value = false;
        }

    }
}
