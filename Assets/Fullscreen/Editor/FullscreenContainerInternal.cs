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

        /// <summary>The m_src will have all it's fields null if we've instantiated the window by ourselves.
        /// <para>Window - The window to fullscreen, null if we're opening a view.</para> 
        /// <para>View - The view to fullscreen, or window.m_Parent if we're opening a window.</para> 
        /// <para>Container - The source container, from the source view.</para> 
        /// </summary>
        [SerializeField] public ViewPyramid m_src;

        /// <summary>
        /// <para>Window - The placeholder window or a window created specially for this fullscreen.</para> 
        /// <para>View - HostView created for this fullscreen.</para> 
        /// <para>Container - The container that will be used for fullscreen, with its mode set to "Popup".</para> 
        /// </summary>
        [SerializeField] public ViewPyramid m_dst;

        /// <summary>Create a ContainerWindow and HostView, then asigns a window to them and shows it as a popup.</summary>
        /// <param name="rect">The initial position of the ContainerWindow, can be changed later using the <see cref="Rect"/> property.</param>
        /// <param name="childWindow">The initial window for the newly created ContainerWindow and HostView.</param>
        /// <returns>Returns the pyramid of views we've created.</returns>
        protected ViewPyramid CreateFullscreenViewPyramid(Rect rect, EditorWindow childWindow) {
            var hv = CreateInstance(Types.HostView);
            var cw = CreateInstance(Types.ContainerWindow);

            hv.name = name;
            cw.name = name;
            childWindow.name = name;

            hv.SetPropertyValue("actualView", childWindow);

            // Order is important here: first set rect of container, then assign main view, then apply various settings, then show.
            // Otherwise the rect won't be set until first resize happens.
            cw.SetPropertyValue("position", rect);
            cw.SetPropertyValue("rootView", hv);

            childWindow.InvokeMethod("MakeParentsSettingsMatchMe");

            var loadPosition = false;
            var displayImmediately = true;
            var setFocus = true;

            if (cw.HasMethod("Show", new[] { typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int) }))
                cw.InvokeMethod("Show", (int)ShowMode.NoShadow, loadPosition, displayImmediately, setFocus, 0);
            else if (cw.HasMethod("Show", new[] { typeof(int), typeof(bool), typeof(bool) }))
                cw.InvokeMethod("Show", (int)ShowMode.NoShadow, loadPosition, displayImmediately);
            else
                cw.InvokeMethod("Show", (int)ShowMode.NoShadow, loadPosition, displayImmediately, setFocus);

            // set min/max size now that native window is not null so that it will e.g., use proper styleMask on macOS
            cw.InvokeMethod("SetMinMaxSizes", rect.size, rect.size); // min, max

            cw.SetFieldValue("m_ShowMode", (int)ShowMode.PopupMenu); // Prevents window decoration from being draw
            cw.SetFieldValue("m_DontSaveToLayout", true);

            Logger.Debug(this, "Created {0}, resolution {1:0}x{2:0}, pos {3:0}", name, rect.width, rect.height, rect.min);

            return new ViewPyramid() { Window = childWindow, View = hv, Container = cw };

        }

        /// <summary>Prevents any repaint on the container window. This fixes some glitches on macOS.</summary>
        /// <param name="containerWindow">The ContainerWindow to freeze the repaints.</param>
        /// <param name="freeze">Wheter to freeze or unfreeze the container.</param>
        protected void SetFreezeContainer(ContainerWindow containerWindow, bool freeze) {
            containerWindow.InvokeMethod("SetFreezeDisplay", freeze);
        }

        /// <summary>Method that will be called just before creating the ContainerWindow for this fullscreen.</summary>
        protected virtual void BeforeOpening() {

            FullscreenCallbacks.beforeFullscreenOpen(this);

            if (m_dst.Container)
                new Exception("Container already has a fullscreened view");
        }

        /// <summary>Method that will be called after the creation of the ContainerWindow for this fullscreen.</summary>
        protected virtual void AfterOpening() {

            After.Frames(2, () => {
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                didPresent.Invoke();
                didPresent = null;
            });

            Logger.Debug(this, "{6}\n\nSRC\nWindow: {0}\nView: {1}\nContainer: {2}\n\nDST\nWindow: {3}\nView: {4}\nContainer: {5}\n",
                m_src.Window,
                m_src.View,
                m_src.Container,
                m_dst.Window,
                m_dst.View,
                m_dst.Container,
                name
            );

            FullscreenCallbacks.afterFullscreenOpen(this);
        }

    }

}
