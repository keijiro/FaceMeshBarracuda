using System;

namespace FullscreenEditor {
    /// <summary>
    /// Utility callbacks for fullscreen state changes.
    /// </summary>
    public static class FullscreenCallbacks {

        /// <summary>
        /// Callback called before the views are restored to their original position.
        /// </summary>
        public static Action<FullscreenContainer> beforeFullscreenClose = (f) => { };

        /// <summary>
        /// Callback called before the container for the fullscreen view is created and
        /// the views are moved between ContainerWindows.
        /// </summary>
        public static Action<FullscreenContainer> beforeFullscreenOpen = (f) => { };

        /// <summary>
        /// Callback called in the OnDestroy method of the FullscreenContainer, after the
        /// views have been reverted to their orignal positions.
        /// </summary>
        public static Action<FullscreenContainer> afterFullscreenClose = (f) => { };

        /// <summary>
        /// Callback called after the fullscreen is opened and everything is already set up.
        /// </summary>
        public static Action<FullscreenContainer> afterFullscreenOpen = (f) => { };

    }
}
