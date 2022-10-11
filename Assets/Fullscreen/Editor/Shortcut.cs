using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FullscreenEditor {

    [AttributeUsage(AttributeTargets.Field)]
    internal class DynamicMenuItemAttribute : Attribute {

        public bool AllowNoneValue { get; private set; }

        public DynamicMenuItemAttribute(bool allowNoneValue) {
            AllowNoneValue = allowNoneValue;
        }

    }

    internal class Shortcut {

        #region Fields
        //Always end with an space if the path has no shortcut
        [DynamicMenuItem(true)] public const string TOOLBAR_PATH = "Fullscreen/Show Toolbar _F8";
        [DynamicMenuItem(true)] public const string FULLSCREEN_ON_PLAY_PATH = "Fullscreen/Fullscreen On Play ";
        [DynamicMenuItem(true)] public const string PREFERENCES_PATH = "Fullscreen/Preferences... ";
        [DynamicMenuItem(false)] public const string CURRENT_VIEW_PATH = "Fullscreen/Focused View _F9";
        [DynamicMenuItem(false)] public const string GAME_VIEW_PATH = "Fullscreen/Game View _F10";
        [DynamicMenuItem(false)] public const string SCENE_VIEW_PATH = "Fullscreen/Scene View _F11";
        [DynamicMenuItem(false)] public const string MAIN_VIEW_PATH = "Fullscreen/Main View _F12";
        [DynamicMenuItem(false)] public const string MOSAIC_PATH = "Fullscreen/Mosaic %F10";
        [DynamicMenuItem(true)] public const string CLOSE_ALL_FULLSCREEN = "Fullscreen/Close All %F12";

        private const char CTRL_CHAR = '%';
        private const char SHIFT_CHAR = '#';
        private const char ALT_CHAR = '&';
        private const char NONE_CHAR = '_';

        private static readonly List<Shortcut> fieldsInfo = new List<Shortcut>();
        /* fixformat ignore:start */
        private static readonly string[] keys = new string[] {
            "None",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "LEFT", "RIGHT", "UP", "DOWN", "HOME", "END", "PGUP", "PGDN"
        };
        /* fixformat ignore:end */

        private static bool changed;
        #endregion

        #region Properties
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public int KeyCode { get; set; }

        public bool AllowNoneValue { get; private set; }
        public string FieldName { get; private set; }
        public string BaseString { get; private set; }
        public string Label { get { return BaseString.Substring(BaseString.LastIndexOf('/') + 1); } }

        private static bool IsSourceFile { get { return !string.IsNullOrEmpty(ThisFilePath) && File.Exists(ThisFilePath); } }
        private static string ThisFilePath {
            get {
                try {
                    return new StackFrame(true).GetFileName();
                } catch (Exception e) {
                    Logger.Exception(e);
                    return string.Empty;
                }
            }
        }
        #endregion

        #region Constructors
        static Shortcut() {
            var type = typeof(Shortcut);
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            if (fields != null)
                foreach (var field in fields) {
                    var att = field.GetCustomAttributes(typeof(DynamicMenuItemAttribute), false);

                    if (att != null)
                        for (var i = 0; i < att.Length; i++)
                            fieldsInfo.Add(new Shortcut((DynamicMenuItemAttribute)att[i], field));
                }
        }

        public Shortcut(DynamicMenuItemAttribute shortcutAttribute, FieldInfo field) {
            FieldName = field.Name;
            AllowNoneValue = shortcutAttribute.AllowNoneValue;

            var constant = (string)field.GetValue(null);
            var lastSpace = constant.LastIndexOf(' ') + 1;

            if (!constant.EndsWith(" "))
                BaseString = constant.Remove(lastSpace);
            else {
                BaseString = constant;
                return;
            }

            constant = constant.Substring(lastSpace);

            if (string.IsNullOrEmpty(constant))
                return;

            Ctrl = constant.Contains(CTRL_CHAR);
            Shift = constant.Contains(SHIFT_CHAR);
            Alt = constant.Contains(ALT_CHAR);

            constant = constant.Replace(CTRL_CHAR.ToString(), string.Empty);
            constant = constant.Replace(SHIFT_CHAR.ToString(), string.Empty);
            constant = constant.Replace(ALT_CHAR.ToString(), string.Empty);
            constant = constant.Replace(NONE_CHAR.ToString(), string.Empty);

            KeyCode = Array.IndexOf(keys, constant);

            if (KeyCode < 0 || KeyCode >= keys.Length) {
                Logger.Warning("Invalid shortcut term: {0}", constant);
                KeyCode = 0;
            }
        }
        #endregion

        public string GetShortcutString() {
            if (KeyCode == 0)
                return "";

            var result = new StringBuilder();

            if (!Ctrl && !Shift && !Alt)
                result.Append(NONE_CHAR);
            else {
                if (Ctrl)
                    result.Append(CTRL_CHAR);
                if (Shift)
                    result.Append(SHIFT_CHAR);
                if (Alt)
                    result.Append(ALT_CHAR);
            }

            result.Append(keys[KeyCode]);

            return result.ToString();
        }

        #region Methods
        public override string ToString() {
            return BaseString + GetShortcutString();
        }

        public static void DoShortcutsGUI() {
            GUI.changed = false;

            using(new EditorGUI.DisabledGroupScope(EditorApplication.isCompiling || !IsSourceFile)) {

                if (InternalEditorUtility.GetUnityVersion() >= new Version(2019, 1))
                    EditorGUILayout.HelpBox(string.Format("You can set custom shortcuts on a per user basis by editing them under {0} menu", FullscreenUtility.IsMacOS ? "Unity/Shortcuts" : "Edit/Shortcuts"), MessageType.Info);

                foreach (var field in fieldsInfo)
                    DrawShortcut(field);

                var duplicated = AnyDuplicates();
                var invalid = AnyInvalid();

                if (duplicated)
                    EditorGUILayout.HelpBox("Some menu items have the same keystroke, this is not allowed.", MessageType.Error);

                if (invalid)
                    EditorGUILayout.HelpBox("Some menu items don't have a valid keystroke, you won't be able to use their correspondent fullscreens.", MessageType.Warning);

                using(new EditorGUI.DisabledGroupScope(duplicated || !changed))
                if (GUILayout.Button("Apply Shortcuts"))
                    ApplyChanges();
            }

            if (GUI.changed)
                changed = true;
        }

        private static void ApplyChanges() {
            if (EditorApplication.isCompiling)
                return;

            AssetDatabase.StartAssetEditing();

            foreach (var field in fieldsInfo)
                ReplaceConstant(field.FieldName, field);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        private static bool AnyInvalid() {
            foreach (var field in fieldsInfo)
                if (field == null || !field.AllowNoneValue && field.KeyCode == 0)
                    return true;

            return false;
        }

        private static bool AnyDuplicates() {
            for (var i = 0; i < fieldsInfo.Count; i++)
                for (var j = i + 1; j < fieldsInfo.Count; j++) {
                    var fieldI = fieldsInfo[i];
                    var fieldJ = fieldsInfo[j];

                    if (fieldI == null || fieldJ == null ||
                        (fieldI.KeyCode != 0 && fieldI.GetShortcutString() == fieldJ.GetShortcutString()))
                        return true;
                }

            return false;
        }

        private static Shortcut DrawShortcut(Shortcut shortcut) {
            using(new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(shortcut.Label, GUILayout.Width(130f));

                shortcut.Ctrl = GUILayout.Toggle(shortcut.Ctrl, FullscreenUtility.IsMacOS ? "Cmd" : "Ctrl", EditorStyles.miniButtonLeft, GUILayout.Width(50f));
                shortcut.Shift = GUILayout.Toggle(shortcut.Shift, "Shift", EditorStyles.miniButtonMid, GUILayout.Width(50f));
                shortcut.Alt = GUILayout.Toggle(shortcut.Alt, "Alt", EditorStyles.miniButtonRight, GUILayout.Width(50f));
                shortcut.KeyCode = EditorGUILayout.Popup(shortcut.KeyCode, keys);

                if (GUILayout.Button(new GUIContent("X", "Clear Shortcut"))) {
                    shortcut.Ctrl = false;
                    shortcut.Shift = false;
                    shortcut.Alt = false;
                    shortcut.KeyCode = 0;
                }
            }

            return shortcut;
        }

        private static void ReplaceConstant(string constantName, object newValue) {
            try {
                if (!IsSourceFile) {
                    Logger.Error("Could not find the source code file to change value");
                    return;
                }

                var fileText = new StringBuilder();
                var changed = false;

                using(var file = File.OpenText(ThisFilePath))
                while (!file.EndOfStream) {
                    var line = file.ReadLine();

                    if (!line.Contains(constantName)) {
                        fileText.AppendLine(line);
                        continue;
                    }

                    var indexOfValue = line.IndexOf('=');

                    fileText.Append(line.Remove(indexOfValue));
                    fileText.AppendLine(string.Format("= \"{0}\";", newValue));
                    fileText.Append(file.ReadToEnd());

                    changed = true;
                }

                fileText = fileText.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

                if (changed)
                    File.WriteAllText(ThisFilePath, fileText.ToString());
                else
                    Logger.Warning("Failed to find field {0} on {1}", constantName, ThisFilePath);
            } catch (Exception e) {
                Logger.Exception(e);
                Logger.Error("Failed to save Fullscreen Editor shortcuts");
            }
        }
        #endregion

    }

}
