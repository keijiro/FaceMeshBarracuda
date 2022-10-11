using UnityEditor;

namespace FullscreenEditor {
    [InitializeOnLoad]
    // Issue #93
    public class FixGameViewMouseInput {

        static FixGameViewMouseInput() {
            FullscreenCallbacks.afterFullscreenOpen += fs => UpdateGameViewArea(fs);
        }

        public static void UpdateGameViewArea(FullscreenContainer fs) {
            After.Frames(50, () => {
                var window = fs.ActualViewPyramid.Window;
                if (window && window.IsOfType(Types.PlayModeView)) {
                    Logger.Debug("Fixing game view area");
                    FullscreenUtility.FocusView(FullscreenUtility.GetMainView());

                    // Issue #95, fix Input.mouseScrollDelta
                    window.Focus();
                }
            });
        }

    }
}
