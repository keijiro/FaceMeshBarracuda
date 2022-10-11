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
    /// <summary>Represents the pyramid containing all the elements that make up a window.</summary>
    [Serializable]
    public struct ViewPyramid {

        /// <summary>The actual window, may be null if the pyramid was created from a view or container.</summary>
        public EditorWindow Window {
            get {
                if (!m_window && m_windowInstanceID != 0)
                    m_window = (EditorWindow)EditorUtility.InstanceIDToObject(m_windowInstanceID);
                return m_window;
            }
            set {
                m_window = value;
                m_windowInstanceID = m_window ? m_window.GetInstanceID() : 0;
            }
        }

        /// <summary>View that controls how the window (and child view) are drawn.</summary>
        public View View {
            get {
                if (!m_view && m_viewInstanceID != 0)
                    m_view = (View)EditorUtility.InstanceIDToObject(m_viewInstanceID);
                return m_view;
            }
            set {
                value.EnsureOfType(Types.View);
                m_view = value;
                m_viewInstanceID = m_view ? m_view.GetInstanceID() : 0;
            }
        }

        /// <summary>The native window.</summary>
        public ContainerWindow Container {
            get {
                if (!m_container && m_containerInstanceID != 0)
                    m_container = (ContainerWindow)EditorUtility.InstanceIDToObject(m_containerInstanceID);
                return m_container;
            }
            set {
                value.EnsureOfType(Types.ContainerWindow);
                m_container = value;
                m_containerInstanceID = m_container ? m_container.GetInstanceID() : 0;
            }
        }

        [SerializeField] private EditorWindow m_window;
        [SerializeField] private View m_view;
        [SerializeField] private ContainerWindow m_container;

        [SerializeField] private int m_windowInstanceID;
        [SerializeField] private int m_viewInstanceID;
        [SerializeField] private int m_containerInstanceID;

        /// <summary>Create a new instance and automatically assigns the window, view and container.</summary>
        public ViewPyramid(ScriptableObject viewOrWindow) {

            if (!viewOrWindow) {
                m_window = null;
                m_view = null;
                m_container = null;
            } else if (viewOrWindow.IsOfType(typeof(EditorWindow))) {
                m_window = viewOrWindow as EditorWindow;
                m_view = m_window.GetFieldValue<View>("m_Parent");
                m_container = m_view.GetPropertyValue<ContainerWindow>("window");
            } else if (viewOrWindow.IsOfType(Types.View)) {
                m_window = null;
                m_view = viewOrWindow;
                m_container = m_view.GetPropertyValue<ContainerWindow>("window");
            } else if (viewOrWindow.IsOfType(Types.ContainerWindow)) {
                m_window = null;
                m_view = viewOrWindow.GetPropertyValue<ContainerWindow>("rootView");
                m_container = viewOrWindow;
            } else {
                throw new ArgumentException("Param must be of type EditorWindow, View or ContainerWindow", "viewOrWindow");
            }

            if (!m_window && m_view && m_view.IsOfType(Types.HostView))
                m_window = m_view.GetPropertyValue<EditorWindow>("actualView");

            m_windowInstanceID = m_window ? m_window.GetInstanceID() : 0;
            m_viewInstanceID = m_view ? m_view.GetInstanceID() : 0;
            m_containerInstanceID = m_container ? m_container.GetInstanceID() : 0;

        }

    }
}
