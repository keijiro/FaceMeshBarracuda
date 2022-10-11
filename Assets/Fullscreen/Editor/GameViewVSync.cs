using UnityEditor;

namespace FullscreenEditor {
    [InitializeOnLoad]
    public class GameViewVSync {

        static GameViewVSync() {
            FullscreenCallbacks.afterFullscreenOpen += (fs) => {
                RefreshViewVSync(fs.ActualViewPyramid.Window);
            };

            FullscreenCallbacks.afterFullscreenClose += (fs) => {
                RefreshViewVSync(fs.m_src.Window);
            };
        }

        private static void RefreshViewVSync(EditorWindow window) {
            if (window && window.HasProperty("vSyncEnabled")) {
                var vsyncEnabled = window.GetPropertyValue<bool>("vSyncEnabled");

                // reset vsync
                window.SetPropertyValue("vSyncEnabled", vsyncEnabled);

                var view = new ViewPyramid(window).View;

                // fallback when above doesn't work
                if (view.HasMethod("EnableVSync"))
                    view.InvokeMethod("EnableVSync", vsyncEnabled);
                else
                    Logger.Debug(string.Format("View {0} does not support vsync", view.GetType()));
            }
        }

    }
}
