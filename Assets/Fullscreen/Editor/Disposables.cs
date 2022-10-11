using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace FullscreenEditor {

    public struct GUIBackgroundColor : IDisposable {
        private readonly Color before;

        public GUIBackgroundColor(Color color) {
            before = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        public void Dispose() {
            GUI.backgroundColor = before;
        }
    }

    public struct GUIContentColor : IDisposable {
        private readonly Color before;

        public GUIContentColor(Color color) {
            before = GUI.contentColor;
            GUI.contentColor = color;
        }

        public void Dispose() {
            GUI.contentColor = before;
        }
    }

    public struct GUIColor : IDisposable {
        private readonly Color before;

        public GUIColor(Color color) {
            before = GUI.color;
            GUI.color = color;
        }

        public GUIColor(Color color, float alpha) {
            before = GUI.color;
            color.a = alpha;
            GUI.color = color;
        }

        public void Dispose() {
            GUI.color = before;
        }
    }

    public sealed class GUIIndent : IDisposable {
        public GUIIndent() {
            EditorGUI.indentLevel++;
        }

        public GUIIndent(string label) {
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
        }

        public void Dispose() {
            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();
        }
    }

    public struct GUIEnabled : IDisposable {
        private readonly bool before;

        public GUIEnabled(bool enabled) {
            before = GUI.enabled;
            GUI.enabled = before && enabled;
        }

        public void Dispose() {
            GUI.enabled = before;
        }
    }

    public sealed class GUIFade : IDisposable {
        private AnimBool anim;

        public bool Visible { get; private set; }

        public GUIFade() {
            Visible = true;
        }

        public void SetTarget(bool target) {
            if (anim == null) {
                anim = new AnimBool(target);
                anim.valueChanged.AddListener(() => {
                    if (EditorWindow.focusedWindow)
                        EditorWindow.focusedWindow.Repaint();
                });
            }

            anim.target = target;
            Visible = EditorGUILayout.BeginFadeGroup(anim.faded);
        }

        public void Dispose() {
            EditorGUILayout.EndFadeGroup();
        }
    }

}
