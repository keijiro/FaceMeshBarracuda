using System;

namespace FullscreenEditor {
    /// <summary>Class containing types of UnityEditor internal classes.</summary>
    public static class Types {

        /// <summary>UnityEditor.HostView</summary>
        public static readonly Type HostView = ReflectionUtility.FindClass("UnityEditor.HostView");

        /// <summary>UnityEditor.ContainerWindow</summary>
        public static readonly Type ContainerWindow = ReflectionUtility.FindClass("UnityEditor.ContainerWindow");

        /// <summary>UnityEditor.View</summary>
        public static readonly Type View = ReflectionUtility.FindClass("UnityEditor.View");

        /// <summary>UnityEditor.GUIView</summary>
        public static readonly Type GUIView = ReflectionUtility.FindClass("UnityEditor.GUIView");

        /// <summary>UnityEditor.GameView</summary>
        public static readonly Type GameView = ReflectionUtility.FindClass("UnityEditor.GameView");

        /// <summary>UnityEditor.PreviewEditorWindow</summary>
        public static readonly Type PreviewEditorWindow = ReflectionUtility.FindClass("UnityEditor.PreviewEditorWindow");

        /// <summary>UnityEditor.PlayModeView</summary>
        public static readonly Type PlayModeView = ReflectionUtility.FindClass("UnityEditor.PlayModeView");

        /// <summary>UnityEditor.MainView</summary>
        public static readonly Type MainView = ReflectionUtility.FindClass("UnityEditor.MainView");

        /// <summary>UnityEditor.WindowLayout</summary>
        public static readonly Type WindowLayout = ReflectionUtility.FindClass("UnityEditor.WindowLayout");

        /// <summary>UnityEngine.EnumDataUtility</summary>
        public static readonly Type EnumDataUtility = ReflectionUtility.FindClass("UnityEngine.EnumDataUtility");

        /// <summary>UnityEditor.PlayModeView.EnterPlayModeBehavior</summary>
        // Enum type
        public static readonly Type EnterPlayModeBehavior = PlayModeView?.GetNestedType("EnterPlayModeBehavior");

    }
}
