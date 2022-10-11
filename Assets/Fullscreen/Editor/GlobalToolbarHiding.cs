using UnityEditor;

namespace FullscreenEditor {
    [InitializeOnLoad]
    public class GlobalToolbarHiding {

        private static readonly float defaultToolbarHeight;

        private static bool GlobalToolbarShouldBeHidden {
            get {
                return !FullscreenPreferences.ToolbarVisible &&
                    Fullscreen.GetAllFullscreen(false).Length > 0;
            }
        }

        static GlobalToolbarHiding() {
            defaultToolbarHeight = FullscreenUtility.GetToolbarHeight();

            FullscreenPreferences.UseGlobalToolbarHiding.OnValueSaved += v => {
                if (!v)
                    FullscreenUtility.SetToolbarHeight(defaultToolbarHeight);
            };

            FullscreenPreferences.ToolbarVisible.OnValueSaved += v => UpdateGlobalToolbarStatus();
            UpdateGlobalToolbarStatus();

            After.Frames(2, () => // Why? IDK
                UpdateGlobalToolbarStatus()
            );

            FullscreenCallbacks.afterFullscreenClose += fs => UpdateGlobalToolbarStatus();
            FullscreenCallbacks.afterFullscreenOpen += fs => UpdateGlobalToolbarStatus();
        }

        public static void UpdateGlobalToolbarStatus() {
            if (FullscreenPreferences.UseGlobalToolbarHiding)
                FullscreenUtility.SetToolbarHeight(GlobalToolbarShouldBeHidden ? 0f : defaultToolbarHeight);
        }

    }
}
