using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FullscreenEditor {
    /// <summary>Helper class for saving preferences.</summary>
    /// <typeparam name="T">The type you want to save, must be marked as <see cref="SerializableAttribute"/></typeparam>
    [Serializable]
    public sealed class PrefItem<T> {

        [SerializeField]
        private T savedValue;

        /// <summary>The key for saving the value.</summary>
        public string Key { get; private set; }

        /// <summary>The default value to use when there's none saved.</summary>
        public T DefaultValue { get; private set; }

        /// <summary>A label and an explanation of what this item is for.</summary>
        public GUIContent Content { get; private set; }

        /// <summary>Callback called whenever the saved value changes.</summary>
        public Action<T> OnValueSaved { get; set; }

        /// <summary>The value saved by this instance.</summary>
        public T Value {
            get { return savedValue; }
            set {
                if (!savedValue.Equals(value)) {
                    savedValue = value;
                    SaveValue();
                }
            }
        }

        public PrefItem(string key, T defaultValue, string text, string tooltip) {
            Key = "Fullscreen." + key;

            FullscreenPreferences.onLoadDefaults += DeleteValue;

            Content = new GUIContent(text, tooltip);
            FullscreenPreferences.contents.Add(Content);
            DefaultValue = savedValue = defaultValue;
            LoadValue();
        }

        private void LoadValue() {
            try {
                if (EditorPrefs.HasKey(Key))
                    JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(Key), this);
            } catch (Exception e) {
                Logger.Warning("Failed to load {0}, using default value: {1}", Key, e);
                savedValue = DefaultValue;
                SaveValue();
            }
        }

        public void SaveValue() {
            try {
                EditorPrefs.SetString(Key, JsonUtility.ToJson(this));
                Logger.Debug("Saved value to key {0}:\n{1}", Key, EditorPrefs.GetString(Key));

                if (OnValueSaved != null)
                    OnValueSaved.Invoke(Value);

                InternalEditorUtility.RepaintAllViews();
            } catch (Exception e) {
                Logger.Warning("Failed to save {0}: {1}", Key, e);
            }
        }

        public void DeleteValue() {
            EditorPrefs.DeleteKey(Key);
            savedValue = DefaultValue;

            if (OnValueSaved != null)
                OnValueSaved.Invoke(savedValue);
        }

        public static implicit operator T(PrefItem<T> pb) { return pb.Value; }

        public static implicit operator GUIContent(PrefItem<T> pb) { return pb.Content; }

    }

    /// <summary>Helper class for drawing the <see cref="PrefItem{T}"/> in the <see cref="PreferencesWindow"/>.</summary>
    public static class PrefItemGUI {

        public static void DoGUI(this PrefItem<int> pref) {
            pref.Value = EditorGUILayout.IntField(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<float> pref) {
            pref.Value = EditorGUILayout.FloatField(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<int> pref, int min, int max) {
            pref.Value = EditorGUILayout.IntSlider(pref.Content, pref.Value, min, max);
        }

        public static void DoGUI(this PrefItem<float> pref, float min, float max) {
            pref.Value = EditorGUILayout.Slider(pref.Content, pref.Value, min, max);
        }

        public static void DoGUI(this PrefItem<bool> pref) {
            pref.Value = EditorGUILayout.Toggle(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<string> pref) {
            pref.Value = EditorGUILayout.TextField(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<Color> pref) {
            pref.Value = EditorGUILayout.ColorField(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<Rect> pref) {
            pref.Value = EditorGUILayout.RectField(pref.Content, pref.Value);
        }

        public static void DoGUI(this PrefItem<RectSourceMode> pref) {
            pref.Value = (RectSourceMode)EditorGUILayout.EnumPopup(pref.Content, pref.Value);
        }

    }
}
