using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UMP.Editor
{
    [CustomEditor(typeof(UMPSettings))]
    [CanEditMultipleObjects]
    public class UMPSettingsEditor : UnityEditor.Editor
    {
        private const string VLC_VERSION = "3.0.6";

        private SerializedProperty _assetPathProp;
        private SerializedProperty _useAudioSourceProp;
        private SerializedProperty _useExternalLibrariesProp;
        private SerializedProperty _librariesPathProp;
        private SerializedProperty _youtubeDecryptFunctionProp;

        private static bool[] playersAndroid = null;
        private static bool[] playersIPhone = null;
        private static bool _showExportedPaths = false;
        private static int _exportedPathsSize;
        private static string[] _cachedExportedPaths;
        private static int _chosenMobilePlatform;

        private static GUIStyle _warningLabel = null;
        private static GUIStyle _buttonStyleToggled = null;

        private static Vector2 scrollPos;

        void OnEnable()
        {
            _assetPathProp = serializedObject.FindProperty("_assetPath");
            _useAudioSourceProp = serializedObject.FindProperty("_useAudioSource");
            _useExternalLibrariesProp = serializedObject.FindProperty("_useExternalLibraries");
            _librariesPathProp = serializedObject.FindProperty("_librariesPath");
            _youtubeDecryptFunctionProp = serializedObject.FindProperty("_youtubeDecryptFunction");
        }

        public override void OnInspectorGUI()
        {
            var settings = UMPSettings.Instance;
            var cachedLabelWidth = EditorGUIUtility.labelWidth;

            var installedMobilePlatforms = settings.GetInstalledPlatforms(UMPSettings.Mobile);

            if (_buttonStyleToggled == null)
            {
                _buttonStyleToggled = new GUIStyle(EditorStyles.miniButton);
                _buttonStyleToggled.normal.background = _buttonStyleToggled.active.background;
            }

            if (_warningLabel == null)
            {
                _warningLabel = new GUIStyle(EditorStyles.label);
                _warningLabel.padding = new RectOffset();
                _warningLabel.margin = new RectOffset();
                _warningLabel.wordWrap = true;
            }

            if (playersAndroid == null)
            {
                playersAndroid = new bool[Enum.GetNames(typeof(PlayerOptionsAndroid.PlayerTypes)).Length];
                for (int i = 0; i < playersAndroid.Length; i++)
                {
                    var playerType = (PlayerOptionsAndroid.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));
                    if ((settings.PlayersAndroid & playerType) == playerType)
                        playersAndroid[i] = true;
                }
            }

            if (playersIPhone == null)
            {
                playersIPhone = new bool[Enum.GetNames(typeof(PlayerOptionsIPhone.PlayerTypes)).Length];
                for (int i = 0; i < playersIPhone.Length; i++)
                {
                    var playerType = (PlayerOptionsIPhone.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));
                    if ((settings.PlayersIPhone & playerType) == playerType)
                        playersIPhone[i] = true;
                }
            }

            // Display the asset path
            #region Asset Path
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("Asset Path", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            ShowMessageBox(MessageType.None, _assetPathProp.stringValue);
            EditorGUI.EndDisabledGroup();

            if (settings.IsValidAssetPath)
                ShowMessageBox(MessageType.Info, "Path is correct");
            else
                ShowMessageBox(MessageType.Error, "Can't find asset folder");


            GUI.color = Color.green;
            if (!settings.IsValidAssetPath)
            {
                if (GUILayout.Button("Find Asset Folder"))
                {
                    GUI.FocusControl(null);
                    _assetPathProp.stringValue = FindAssetFolder("Assets");
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
            #endregion

            // Display the Editor/Desktop options
            #region Editor/Desktop
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Editor/Desktop", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useAudioSourceProp, new GUIContent("Use 'Audio Source'", "Will be using Unity 'Audio Source' component for audio output for all UMP instances (global) by default (supported only on desktop platforms)"));

            /*EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Use 'Audio Source' component", "Will be using Unity 'Audio Source' component for audio output for all UMP instances (global) by default."));
            _useAudioSourceProp.boolValue = EditorGUILayout.Toggle(_useAudioSourceProp.boolValue);
            EditorGUILayout.EndHorizontal();*/


            var useInstalled = !UMPSettings.ContainsLibVLC(settings.GetLibrariesPath(UMPSettings.RuntimePlatform, false));

            if (useInstalled)
                ShowMessageBox(MessageType.Warning, "Can't find internal LibVLC libraries in current project, so will be used installed VLC player software by default. To have possibility to use internal libraries please correctly import UMP (Win, Mac, Linux) package");


            if (_useExternalLibrariesProp.boolValue)
                EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.PropertyField(_useExternalLibrariesProp, new GUIContent("Use installed VLC", "Use external/installed VLC player libraries for all UMP instances (global). Path to installed VLC player directory will be obtained automatically"));

            if (useInstalled)
                _useExternalLibrariesProp.boolValue = true;

            if (settings.UseExternalLibraries)
            {
                var librariesPath = settings.GetLibrariesPath(UMPSettings.RuntimePlatform, true);

                if (!UMPSettings.ContainsLibVLC(librariesPath))
                {
                    librariesPath = string.Empty;

                    GUI.color = Color.yellow;
                    EditorGUILayout.BeginVertical(EditorStyles.textArea);

                    EditorGUILayout.LabelField("Warning: Can't find installed VLC player software, please make sure that:\n" +
                        "* Installed VLC player bit equals Unity Editor bit, eg., 'VLC Player 64 bit' == 'Unity Editor 64 bit'\n\n" +
                        "* Use installer version from official site", _warningLabel);

                    GUI.color = Color.white;
                    var vlcUrl = string.Empty;

                    switch (UMPSettings.RuntimePlatform)
                    {
                        case UMPSettings.Platforms.Win:
                            vlcUrl = string.Format("http://get.videolan.org/vlc/{0}/win64/vlc-{0}-win64.exe", VLC_VERSION);

                            if (UMPSettings.EditorBitMode == UMPSettings.BitModes.x86)
                                vlcUrl = string.Format("http://get.videolan.org/vlc/{0}/win32/vlc-{0}-win32.exe", VLC_VERSION);
                            break;

                        case UMPSettings.Platforms.Mac:
                            vlcUrl = string.Format("http://get.videolan.org/vlc/{0}/macosx/vlc-{0}.dmg", VLC_VERSION);
                            break;
                    }

                    if (!string.IsNullOrEmpty(vlcUrl))
                    {
                        _warningLabel.normal.textColor = Color.blue;
                        EditorGUILayout.LabelField(string.Format("{0} (Editor {1})", vlcUrl, UMPSettings.EditorBitModeFolderName), _warningLabel);
                        _warningLabel.normal.textColor = Color.black;
                        Rect urlRect = GUILayoutUtility.GetLastRect();

                        if (Event.current.type == EventType.MouseUp && urlRect.Contains(Event.current.mousePosition))
                            Application.OpenURL(vlcUrl);
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent("Libraries path", @"Path to installed VLC player libraries, eg., 'C:\Program Files\VideoLAN\VLC'"));

                if (!librariesPath.Equals(string.Empty))
                {
                    _librariesPathProp.stringValue = librariesPath;

                    EditorGUI.BeginDisabledGroup(true);
                    ShowMessageBox(MessageType.None, _librariesPathProp.stringValue);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    ShowMessageBox(MessageType.Warning, "Path to installed VLC player directory can't be obtained automatically, try to use custom path to your libVLC libraries");
                    _librariesPathProp.stringValue = EditorGUILayout.TextField(_librariesPathProp.stringValue);
                }
                
                if (UMPSettings.ContainsLibVLC(_librariesPathProp.stringValue))
                    ShowMessageBox(MessageType.Info, "Path is correct");
                else
                    ShowMessageBox(MessageType.Error, @"Can't find VLC player libraries, try to check if your path is correct, eg., 'C:\Program Files\VideoLAN\VLC'");
            }

            if (_useExternalLibrariesProp.boolValue)
                EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            #endregion

            // Display the Mobile options
            #region Mobile
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (installedMobilePlatforms.Length > 0)
            {
                EditorGUILayout.LabelField("Mobile", EditorStyles.boldLabel);
                _chosenMobilePlatform = GUILayout.SelectionGrid(_chosenMobilePlatform, installedMobilePlatforms, installedMobilePlatforms.Length, EditorStyles.miniButton);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField(new GUIContent("Player Types", "Choose player types that will be used in your project"));
                ShowMessageBox(MessageType.Info, "Disabled players will be not included into your build (reducing the file size of your build)");

                GUILayout.BeginHorizontal();

                if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.Android.ToString())
                {
                    for (int i = 0; i < playersAndroid.Length; i++)
                    {
                        if (GUILayout.Button(Enum.GetName(typeof(PlayerOptionsAndroid.PlayerTypes), (i * 2) + (i == 0 ? 1 : 0)), playersAndroid[i] ? _buttonStyleToggled : EditorStyles.miniButton))
                        {
                            var playerType = (PlayerOptionsAndroid.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));

                            if ((settings.PlayersAndroid & ~playerType) > 0)
                            {
                                playersAndroid[i] = !playersAndroid[i];
                                settings.PlayersAndroid = playersAndroid[i] ? settings.PlayersAndroid | playerType : settings.PlayersAndroid & ~playerType;

                                UpdateMobileLibraries(UMPSettings.Platforms.Android, settings.PlayersAndroid);
                            }
                        }
                    }
                }

                if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.iOS.ToString())
                {
                    for (int i = 0; i < playersIPhone.Length; i++)
                    {
                        if (GUILayout.Button(Enum.GetName(typeof(PlayerOptionsIPhone.PlayerTypes), (i * 2) + (i == 0 ? 1 : 0)), playersIPhone[i] ? _buttonStyleToggled : EditorStyles.miniButton))
                        {
                            var playerType = (PlayerOptionsIPhone.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));

                            if ((settings.PlayersIPhone & ~playerType) > 0)
                            {
                                playersIPhone[i] = !playersIPhone[i];
                                settings.PlayersIPhone = playersIPhone[i] ? settings.PlayersIPhone | playerType : settings.PlayersIPhone & ~playerType;

                                UpdateMobileLibraries(UMPSettings.Platforms.iOS, settings.PlayersIPhone);
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.Android.ToString())
                {
                    GUILayout.BeginVertical(GUI.skin.box);

                    if (GUILayout.Button(new GUIContent("Exported Video Paths", "'StreamingAssets' videos (or video parts) that will be copied to special cached destination on device (for possibilities to use playlist: videos that contains many parts)"), _showExportedPaths ? _buttonStyleToggled : EditorStyles.miniButton))
                        _showExportedPaths = !_showExportedPaths;

                    if (_showExportedPaths)
                    {
                        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(false));
                        _exportedPathsSize = EditorGUILayout.IntField(new GUIContent("Size", "Amount of exported videos"), settings.AndroidExportedPaths.Length, GUILayout.ExpandWidth(true));

                        if (_exportedPathsSize < 0)
                            _exportedPathsSize = 0;

                        _cachedExportedPaths = new string[_exportedPathsSize];

                        if (_exportedPathsSize >= 0)
                        {
                            _cachedExportedPaths = new string[_exportedPathsSize];

                            for (int i = 0; i < settings.AndroidExportedPaths.Length; i++)
                            {
                                if (i < _exportedPathsSize)
                                    _cachedExportedPaths[i] = settings.AndroidExportedPaths[i];
                            }
                        }

                        EditorGUIUtility.labelWidth = 60;

                        for (int i = 0; i < _cachedExportedPaths.Length; i++)
                            _cachedExportedPaths[i] = EditorGUILayout.TextField("Path " + i + ":", _cachedExportedPaths[i]);

                        EditorGUIUtility.labelWidth = cachedLabelWidth;

                        settings.AndroidExportedPaths = _cachedExportedPaths;

                        EditorGUILayout.EndScrollView();

                        var evt = Event.current;

                        switch (evt.type)
                        {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:

                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (evt.type == EventType.DragPerform)
                                {
                                    DragAndDrop.AcceptDrag();

                                    var filePaths = DragAndDrop.paths;

                                    if (filePaths.Length > 0)
                                    {
                                        var arrayLength = settings.AndroidExportedPaths.Length > filePaths.Length ? settings.AndroidExportedPaths.Length : filePaths.Length;
                                        _cachedExportedPaths = new string[arrayLength];

                                        for (int i = 0; i < arrayLength; i++)
                                        {
                                            if (i < settings.AndroidExportedPaths.Length)
                                                _cachedExportedPaths[i] = settings.AndroidExportedPaths[i];

                                            if (i < filePaths.Length)
                                                _cachedExportedPaths[i] = filePaths[i];
                                        }

                                        settings.AndroidExportedPaths = _cachedExportedPaths;
                                    }
                                }
                                break;
                        }
                    }
                    GUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();
            #endregion

            // Display the Services options
            #region Services
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Services", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(new GUIContent("Youtube Decrypt Function", "Uses for decrypt the direct links to Youtube video"));
            _youtubeDecryptFunctionProp.stringValue = EditorGUILayout.TextField(_youtubeDecryptFunctionProp.stringValue);
            EditorGUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        private static void ShowMessageBox(MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.None:
                    GUI.color = Color.white;
                    break;

                case MessageType.Info:
                    GUI.color = Color.green;
                    message = "Info: " + message;
                    break;

                case MessageType.Warning:
                    GUI.color = Color.yellow;
                    message = "Warning: " + message;
                    break;

                case MessageType.Error:
                    GUI.color = Color.red;
                    message = "Error: " + message;
                    break;
            }

            var textAreaStyle = EditorStyles.textArea;
            var wrap = textAreaStyle.wordWrap;

            textAreaStyle.wordWrap = true;
            EditorGUILayout.TextArea(message, textAreaStyle);
            textAreaStyle.wordWrap = wrap;

            GUI.color = Color.white;
        }

        private static string FindAssetFolder(string rootPath)
        {
            var projectFolders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            foreach (var folderPath in projectFolders)
            {
                if (Path.GetFileName(folderPath).Contains(UMPSettings.ASSET_NAME) && Directory.GetFiles(folderPath).Length > 0)
                    return folderPath.Replace(@"\", "/");
            }

            return string.Empty;
        }

        public static void UpdateMobileLibraries(UMPSettings.Platforms platform, Enum playerType)
        {
            if (!(playerType is PlayerOptionsAndroid.PlayerTypes) &&
                !(playerType is PlayerOptionsIPhone.PlayerTypes))
                throw new ArgumentException("Enum must be one of this enumerated type: 'PlayerOptionsAndroid.PlayerTypes' or 'PlayerOptionsAndroid.PlayerTypes'");

            var librariesPath = UMPSettings.Instance.GetLibrariesPath(platform, false);
            var playerValues = (int[])Enum.GetValues(playerType.GetType());
            var usedLibs = new List<string>();
            var flags = string.Empty;
#if UNITY_2018_3_OR_NEWER
            flags = "-DBGRA32 ";
#endif
            var addVLCLibs = false;

            if (playerType is PlayerOptionsAndroid.PlayerTypes)
                addVLCLibs = ((PlayerOptionsAndroid.PlayerTypes)playerType & PlayerOptionsAndroid.PlayerTypes.LibVLC) == PlayerOptionsAndroid.PlayerTypes.LibVLC;

            for (int i = 0; i < playerValues.Length; i++)
            {
                if (playerType is PlayerOptionsAndroid.PlayerTypes)
                {
                    var type = (PlayerOptionsAndroid.PlayerTypes)playerValues[i];

                    if (((PlayerOptionsAndroid.PlayerTypes)playerType & type) == type)
                    {
                        usedLibs.Add(type.ToString());
                    }
                }

                if (playerType is PlayerOptionsIPhone.PlayerTypes)
                {
                    var type = (PlayerOptionsIPhone.PlayerTypes)playerValues[i];

                    if (((PlayerOptionsIPhone.PlayerTypes)playerType & type) == type)
                    {
                        usedLibs.Add(type.ToString());
                        flags += "-D" + type.ToString().ToUpper() + " ";
                    }
                }
            }

            var librariesFiles = new List<string>();
            librariesFiles.AddRange(Directory.GetFiles(librariesPath));
            librariesFiles.AddRange(Directory.GetDirectories(librariesPath));

            if (playerType is PlayerOptionsAndroid.PlayerTypes)
            {
                librariesFiles.AddRange(Directory.GetFiles(librariesPath + "libs/armeabi-v7a"));
                librariesFiles.AddRange(Directory.GetFiles(librariesPath + "libs/x86"));
#if UNITY_2018_2_OR_NEWER
                librariesFiles.AddRange(Directory.GetFiles(librariesPath + "libs/arm64-v8a"));
#endif
            }

            var assetFiles = librariesFiles.Select(x => 
            x.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).
            Substring(x.LastIndexOf("Assets")));

            foreach (var assetFile in assetFiles)
            {
                var libraryName = Path.GetFileNameWithoutExtension(assetFile);
                var libraryImporter = AssetImporter.GetAtPath(assetFile) as PluginImporter;
                var isEnable = false;

                foreach (var name in usedLibs)
                {
                    if (libraryName.Contains("Player" + name) ||
                        libraryName.Contains("PlayerBase") ||
                        libraryName.Contains("MediaPlayer") ||
                        (addVLCLibs && libraryName.Contains("lib")))
                        isEnable = true;
                }

                if (libraryImporter != null)
                {
                    libraryImporter.SetCompatibleWithAnyPlatform(false);

                    switch (platform)
                    {
                        case UMPSettings.Platforms.Android:
                            libraryImporter.SetCompatibleWithPlatform(BuildTarget.Android, isEnable);

                            var cpuType = string.Empty;
                            if (assetFile.LastIndexOf("armeabi-v7a") > 0)
                                cpuType = "ARMv7";
                            else if (assetFile.LastIndexOf("x86") > 0)
                                cpuType = "x86";
                            else if (assetFile.LastIndexOf("arm64-v8a") > 0)
                                cpuType = "ARM64";

                            if (!string.IsNullOrEmpty(cpuType))
                                libraryImporter.SetPlatformData(BuildTarget.Android, "CPU", cpuType);
                            break;

                        case UMPSettings.Platforms.iOS:
                            libraryImporter.SetCompatibleWithPlatform(BuildTarget.iOS, isEnable);
                            libraryImporter.SetPlatformData(BuildTarget.iOS, "CompileFlags", flags.Trim());
                            break;
                    }

                    libraryImporter.SaveAndReimport();
                }
            }

            librariesFiles.Clear();
        }
    }
}