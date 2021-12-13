using UnityEditor;
using UnityEngine;

namespace UMP.Editor
{
    public class UMPSettingsWindow : EditorWindow
    {
        private static bool _isCreated = false;
        private static bool _isInitialized = false;

        [MenuItem("Window/UMP Settings")]
        public static void Init()
        {
            // Close opened window
            if (_isInitialized || _isCreated)
            {
                var settingsWindow = GetWindow(typeof(UMPSettingsWindow));
                settingsWindow.Close();
                return;
            }

            _isCreated = true;

            {
                // Get existing window
                var settingsWindow = CreateInstance<UMPSettingsWindow>();
                if (settingsWindow != null)
                    settingsWindow.SetupWindow();
            }
        }

        public void SetupWindow()
        {
            var width = 256f;
            var height = 512f;

            _isCreated = true;
            position = new Rect((Screen.width * 0.5f) - (width * 0.5f), (Screen.height * 0.5f) - (height * 0.5f), width, height);
            minSize = new Vector2(256f, 512f);
            titleContent = new GUIContent("UMP Settings");

            _isInitialized = true;
            ShowUtility();
            Repaint();
        }

        void OnEnable()
        {
            if (!_isCreated)
                SetupWindow();
        }

        void OnDisable()
        {
            _isInitialized = false;
            _isCreated = false;
            Repaint();
        }

        void OnGUI()
        {
            if (!_isInitialized)
            {
                EditorGUILayout.LabelField("Initialising Settings Window...");
                return;
            }

            var settingsEditor = UnityEditor.Editor.CreateEditor(UMPSettings.Instance);
            settingsEditor.OnInspectorGUI();
        }
    }
}