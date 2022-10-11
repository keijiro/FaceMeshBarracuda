using UnityEditor;

namespace FullscreenEditor {
    [InitializeOnLoad]
    // Issues #98 #96 #97 and #99
    public class GameViewLowResolutionAspectRatios {

        static GameViewLowResolutionAspectRatios() {
            FullscreenCallbacks.afterFullscreenOpen += fs => {
                var window = fs.ActualViewPyramid.Window;

                if (window && window.HasProperty("lowResolutionForAspectRatios"))
                    window.SetPropertyValue("lowResolutionForAspectRatios", false);
            };
        }

    }
}
