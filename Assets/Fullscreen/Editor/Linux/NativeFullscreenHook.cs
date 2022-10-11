using UnityEditor;

namespace FullscreenEditor.Linux {
    internal static class NativeFullscreenHooks {

        [InitializeOnLoadMethod]
        private static void Init() {
            if (!FullscreenUtility.IsLinux)
                return;

            FullscreenCallbacks.afterFullscreenOpen += (fs) => {
                if (wmctrl.IsInstalled && !FullscreenPreferences.DoNotUseWmctrl.Value)
                    wmctrl.SetNativeFullscreen(true, fs.m_dst.Container);
            };
            FullscreenCallbacks.beforeFullscreenClose += (fs) => {
                if (wmctrl.IsInstalled && !FullscreenPreferences.DoNotUseWmctrl.Value)
                    wmctrl.SetNativeFullscreen(false, fs.m_dst.Container);
            };
        }

    }
}
