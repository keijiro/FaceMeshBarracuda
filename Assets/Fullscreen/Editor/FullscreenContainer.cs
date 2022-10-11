using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using HostView = UnityEngine.ScriptableObject;
using View = UnityEngine.ScriptableObject;
using ContainerWindow = UnityEngine.ScriptableObject;

namespace FullscreenEditor {
    /// <summary>Manages the WindowContainers, Views and Windows that will be fullscreened.</summary>
    public abstract partial class FullscreenContainer : ScriptableObject {

        [SerializeField] private int m_ourIndex = -1;
        [SerializeField] private bool m_old = false;

        public Action didPresent = () => Logger.Debug("'Did Present' called");

        private static int CurrentIndex {
            get { return EditorPrefs.GetInt("FullscreenIdx", 0); }
            set { EditorPrefs.SetInt("FullscreenIdx", value); }
        }

        /// <summary>The true view pyramid of this fullscreen container.</summary>
        public ViewPyramid ActualViewPyramid {
            get { return new ViewPyramid(m_dst.Container); }
        }

        /// <summary>The view that is currently fullscreened.</summary>
        public View FullscreenedView {
            get { return ActualViewPyramid.View; }
        }

        /// <summary>Position and size of the WindowContainer created for this fullscreen.</summary>
        public Rect Rect {
            get {
                return m_dst.Container ?
                    m_dst.Container.GetPropertyValue<Rect>("position") :
                    new Rect();
            }
            set {
                if (m_dst.Container) {
                    m_dst.Container.InvokeMethod("SetMinMaxSizes", value.size, value.size);
                    m_dst.Container.SetPropertyValue("position", value);
                    Logger.Debug("Set {0} rect to {1}", this.name, value);
                } else
                    Logger.Debug("No container on {0}, rect will not be set", this.name);
            }
        }

        private void Update() {
            if (!m_dst.Container)
                Close(); // Forcefully closed
        }

        protected virtual void OnEnable() {

            if (m_ourIndex == -1) {
                m_ourIndex = CurrentIndex++;
                name = string.Format("Fullscreen #{0}", m_ourIndex);
                hideFlags = HideFlags.HideAndDontSave;
            }

            #if UNITY_2018_1_OR_NEWER
            EditorApplication.wantsToQuit += WantsToQuit;
            #endif

            if (m_old && !m_dst.Container) {
                Logger.Warning("{0} wasn't properly closed", name);
                // After 1 frame to prevent OnDisable and OnDestroy from being called before this methods returns
                After.Frames(1, () => DestroyImmediate(this, true));
            }

            m_old = true;
            EditorApplication.update += Update;
        }

        protected virtual void OnDisable() {
            EditorApplication.update -= Update;
            #if UNITY_2018_1_OR_NEWER
            EditorApplication.wantsToQuit += WantsToQuit;
            #endif
        }

        protected virtual void OnDestroy() {
            Logger.Debug(name + " destroyed");

            if (m_dst.Container) {
                m_dst.Container.InvokeMethod("Close");
                Logger.Warning("Destroying {0} which has open containers, always close the fullscreen before destroying it", name);
            }

            FullscreenCallbacks.afterFullscreenClose(this);
        }

        /// <summary>Destroy this container and exit fullscreen.</summary>
        public virtual void Close() {

            FullscreenCallbacks.beforeFullscreenClose(this);

            if (!m_dst.Window && m_dst.Container)
                Logger.Error("Placeholder window has been closed, Fullscreen Editor won't be able to restore window position");

            if (m_dst.Container) // Container may have been destroyed by Alt+F4
                m_dst.Container.InvokeMethod("Close"); // Closes the container, all its views and the windows

            DestroyImmediate(this, true);

        }

        /// <summary>Focus the view of this fullscreen.</summary>
        public virtual void Focus() {
            if (FullscreenedView && FullscreenedView.IsOfType(Types.GUIView))
                FullscreenUtility.FocusView(FullscreenedView);
        }

        /// <summary>Gets wheter the view of this fullscreen is focused or not.</summary>
        public virtual bool IsFocused() {
            return FullscreenUtility.IsViewFocused(FullscreenedView);
        }

        #if UNITY_2018_1_OR_NEWER
        private bool WantsToQuit() {
            // Close the fullscreen before closing the editor, this way we have a better
            // ensurance that the fullscreen container will not be saved to the layout.
            // ContainerWindow.m_DontSaveToLayout is set to true, so in Unity < 2018.1 the
            // fullscreen will behave the same as if it was closed by Alt+F4.
            Close();
            return true;
        }
        #endif

    }
}
