using System;
using UnityEditor;
using UnityEngine;
using UMP;
using System.Collections.Generic;

[CustomEditor(typeof(UniversalMediaPlayer))]
[CanEditMultipleObjects]
[Serializable]
public class UMPEditor : Editor
{
    SerializedProperty _renderingObjectsProp;
    SerializedProperty _pathProp;
    SerializedProperty _autoPlayProp;
    SerializedProperty _loopProp;
    SerializedProperty _loopSmoothProp;
    SerializedProperty _muteProp;
    SerializedProperty _useAdvancedProp;
    SerializedProperty _useFixedSizeProp;
    SerializedProperty _fixedVideoWidthProp;
    SerializedProperty _fixedVideoHeightProp;
    SerializedProperty _chosenPlatformProp;
    SerializedProperty _volumeProp;
    SerializedProperty _playRateProp;
    SerializedProperty _positionProp;

    SerializedProperty _logDetailProp;
    SerializedProperty _lastEventMsgProp;
    SerializedProperty _pathPreparedEventProp;
    SerializedProperty _openingEventProp;
    SerializedProperty _bufferingEventProp;
    SerializedProperty _imageReadyEventProp;
    SerializedProperty _preparedEventProp;
    SerializedProperty _playingEventProp;
    SerializedProperty _pausedEventProp;
    SerializedProperty _stoppedEventProp;
    SerializedProperty _endReachedEventProp;
    SerializedProperty _encounteredErrorEventProp;
    SerializedProperty _timeChangedEventProp;
    SerializedProperty _positionChangedEventProp;
    SerializedProperty _snapshotTakenEventProp;

    #region Desktop Options
    SerializedProperty _desktopAudioOutputsProp;
    SerializedProperty _desktopHardwareDecodingProp;
    SerializedProperty _desktopVideoBufferSizeProp;
    SerializedProperty _desktopFlipVerticallyProp;
    SerializedProperty _desktopOutputToFileProp;
    SerializedProperty _desktopDisplayOutputProp;
    SerializedProperty _desktopOutputFilePathProp;
    SerializedProperty _desktopRtspOverTcpProp;
    SerializedProperty _desktopFileCachingProp;
    SerializedProperty _desktopLiveCachingProp;
    SerializedProperty _desktopDiskCachingProp;
    SerializedProperty _desktopNetworkCachingProp;
    #endregion

    #region Android Options
    SerializedProperty _androidPlayerTypeProp;
    SerializedProperty _androidHardwareAccelerationProp;
    SerializedProperty _androidOpenGLDecodingProp;
    SerializedProperty _androidVideoChromaProp;

    SerializedProperty _androidPlayInBackgroundProb;
    SerializedProperty _androidRtspOverTcpProp;
    SerializedProperty _androidNetworkCachingProp;
    #endregion

    #region IPhone Options
    SerializedProperty _iphonePlayerTypeProp;
    SerializedProperty _iphoneFlipVerticallyProp;
    SerializedProperty _iphoneVideoToolboxProp;
    SerializedProperty _iphoneVideoToolboxMaxFrameWidthProp;
    SerializedProperty _iphoneVideoToolboxAsyncProp;
    SerializedProperty _iphoneVideoToolboxWaitAsyncProp;
    SerializedProperty _iphonePlayInBackgroundProb;
    SerializedProperty _iphoneRtspOverTcpProp;
    SerializedProperty _iphonePacketBufferingProp;
    SerializedProperty _iphoneMaxBufferSizeProp;
    SerializedProperty _iphoneMinFramesProp;
    SerializedProperty _iphoneInfbufProp;
    SerializedProperty _iphoneFramedropProp;
    SerializedProperty _iphoneMaxFpsProp;
    #endregion

    private string[] _availablePlatforms;
    private string _externalPath = string.Empty;
    private bool _showEventsListeners;
    private FontStyle _cachedFontStyle;
    private Color _cachedTextColor;
    private float _cachedLabelWidth;
	private bool _cachedLabelWordWrap;
    private static GUIStyle _toggleButton = null;

    private void OnEnable()
    {
        // Setup the SerializedProperties
        _renderingObjectsProp = serializedObject.FindProperty("_renderingObjects");
        _pathProp = serializedObject.FindProperty("_path");
        _autoPlayProp = serializedObject.FindProperty("_autoPlay");
        _loopProp = serializedObject.FindProperty("_loop");
        _loopSmoothProp = serializedObject.FindProperty("_loopSmooth");
        _muteProp = serializedObject.FindProperty("_mute");

        _useAdvancedProp = serializedObject.FindProperty("_useAdvanced");
        _useFixedSizeProp = serializedObject.FindProperty("_useFixedSize");
        _fixedVideoWidthProp = serializedObject.FindProperty("_fixedVideoWidth");
        _fixedVideoHeightProp = serializedObject.FindProperty("_fixedVideoHeight");
        _chosenPlatformProp = serializedObject.FindProperty("_chosenPlatform");
        _volumeProp = serializedObject.FindProperty("_volume");
        _playRateProp = serializedObject.FindProperty("_playRate");
        _positionProp = serializedObject.FindProperty("_position");

        _logDetailProp = serializedObject.FindProperty("_logDetail");
        _lastEventMsgProp = serializedObject.FindProperty("_lastEventMsg");
        _pathPreparedEventProp = serializedObject.FindProperty("_pathPreparedEvent");
        _openingEventProp = serializedObject.FindProperty("_openingEvent");
        _bufferingEventProp = serializedObject.FindProperty("_bufferingEvent");
        _imageReadyEventProp = serializedObject.FindProperty("_imageReadyEvent");
        _preparedEventProp = serializedObject.FindProperty("_preparedEvent");
        _playingEventProp = serializedObject.FindProperty("_playingEvent");
        _pausedEventProp = serializedObject.FindProperty("_pausedEvent");
        _stoppedEventProp = serializedObject.FindProperty("_stoppedEvent");
        _endReachedEventProp = serializedObject.FindProperty("_endReachedEvent");
        _encounteredErrorEventProp = serializedObject.FindProperty("_encounteredErrorEvent");
        _timeChangedEventProp = serializedObject.FindProperty("_timeChangedEvent");
        _positionChangedEventProp = serializedObject.FindProperty("_positionChangedEvent");
        _snapshotTakenEventProp = serializedObject.FindProperty("_snapshotTakenEvent");

        #region Desktop Options
        _desktopAudioOutputsProp = serializedObject.FindProperty("_desktopAudioOutputs");
        _desktopHardwareDecodingProp = serializedObject.FindProperty("_desktopHardwareDecoding");
        _desktopFlipVerticallyProp = serializedObject.FindProperty("_desktopFlipVertically");
        _desktopVideoBufferSizeProp = serializedObject.FindProperty("_desktopVideoBufferSize");
        _desktopOutputToFileProp = serializedObject.FindProperty("_desktopOutputToFile");
        _desktopDisplayOutputProp = serializedObject.FindProperty("_desktopDisplayOutput");
        _desktopOutputFilePathProp = serializedObject.FindProperty("_desktopOutputFilePath");
        _desktopRtspOverTcpProp = serializedObject.FindProperty("_desktopRtspOverTcp");
        _desktopFileCachingProp = serializedObject.FindProperty("_desktopFileCaching");
        _desktopLiveCachingProp = serializedObject.FindProperty("_desktopLiveCaching");
        _desktopDiskCachingProp = serializedObject.FindProperty("_desktopDiskCaching");
        _desktopNetworkCachingProp = serializedObject.FindProperty("_desktopNetworkCaching");
        #endregion

        #region Android Options
        _androidPlayerTypeProp = serializedObject.FindProperty("_androidPlayerType");
        _androidHardwareAccelerationProp = serializedObject.FindProperty("_androidHardwareAcceleration");
        _androidOpenGLDecodingProp = serializedObject.FindProperty("_androidOpenGLDecoding");
        _androidVideoChromaProp = serializedObject.FindProperty("_androidVideoChroma");

        _androidPlayInBackgroundProb = serializedObject.FindProperty("_androidPlayInBackground");
        _androidRtspOverTcpProp = serializedObject.FindProperty("_androidRtspOverTcp");
        _androidNetworkCachingProp = serializedObject.FindProperty("_androidNetworkCaching");
        #endregion

        #region IPhone Options
        _iphonePlayerTypeProp = serializedObject.FindProperty("_iphonePlayerType");
        _iphoneFlipVerticallyProp = serializedObject.FindProperty("_iphoneFlipVertically");
        _iphoneVideoToolboxProp = serializedObject.FindProperty("_iphoneVideoToolbox");
        _iphoneVideoToolboxMaxFrameWidthProp = serializedObject.FindProperty("_iphoneVideoToolboxMaxFrameWidth");
        _iphoneVideoToolboxAsyncProp = serializedObject.FindProperty("_iphoneVideoToolboxAsync");
        _iphoneVideoToolboxWaitAsyncProp = serializedObject.FindProperty("_iphoneVideoToolboxWaitAsync");
        _iphonePlayInBackgroundProb = serializedObject.FindProperty("_iphonePlayInBackground");
        _iphoneRtspOverTcpProp = serializedObject.FindProperty("_iphoneRtspOverTcp");
        _iphonePacketBufferingProp = serializedObject.FindProperty("_iphonePacketBuffering");
        _iphoneMaxBufferSizeProp = serializedObject.FindProperty("_iphoneMaxBufferSize");
        _iphoneMinFramesProp = serializedObject.FindProperty("_iphoneMinFrames");
        _iphoneInfbufProp = serializedObject.FindProperty("_iphoneInfbuf");
        _iphoneFramedropProp = serializedObject.FindProperty("_iphoneFramedrop");
        _iphoneMaxFpsProp = serializedObject.FindProperty("_iphoneMaxFps");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var umpEditor = (UniversalMediaPlayer)target;
        var settings = UMPSettings.Instance;

        EditorGUI.BeginChangeCheck();

        _cachedFontStyle = EditorStyles.label.fontStyle;
        _cachedTextColor = EditorStyles.textField.normal.textColor;
		_cachedLabelWordWrap = EditorStyles.label.wordWrap;

        #region Rendering Field
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_renderingObjectsProp, new GUIContent("Rendering GameObjects:"), true);
        #endregion

        #region Path Field
        EditorGUILayout.Space();

        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Path to video file:");
        EditorStyles.label.fontStyle = _cachedFontStyle;
        EditorStyles.textField.wordWrap = true;
        _pathProp.stringValue = EditorGUILayout.TextField(_pathProp.stringValue, GUILayout.Height(30));
        EditorStyles.textField.wordWrap = false;
        #endregion

        #region Additional Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Additional properties:");
        EditorStyles.label.fontStyle = _cachedFontStyle;

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AutoPlay:", GUILayout.MinWidth(60));
        _autoPlayProp.boolValue = EditorGUILayout.Toggle(_autoPlayProp.boolValue, EditorStyles.radioButton);

        if (!_loopSmoothProp.boolValue)
            EditorGUILayout.LabelField("Loop:", GUILayout.MinWidth(36));
        else
            EditorGUILayout.LabelField("Loop(smooth):", GUILayout.MinWidth(90));

        _loopProp.boolValue = EditorGUILayout.Toggle(_loopProp.boolValue, EditorStyles.radioButton, GUILayout.MaxWidth(20));
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        _loopSmoothProp.boolValue = EditorGUILayout.Toggle(_loopSmoothProp.boolValue, EditorStyles.radioButton);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("Mute:", GUILayout.MinWidth(36));
        _muteProp.boolValue = EditorGUILayout.Toggle(_muteProp.boolValue, EditorStyles.radioButton);
        GUILayout.EndHorizontal();

        if (_toggleButton == null)
        {
            _toggleButton = new GUIStyle(EditorStyles.miniButtonMid);
            _toggleButton.normal.background = EditorStyles.miniButton.active.background;
        }

        if (GUILayout.Button("Advanced options", _useAdvancedProp.boolValue ? _toggleButton : EditorStyles.miniButtonMid))
        {
            _useAdvancedProp.boolValue = !_useAdvancedProp.boolValue;
            _availablePlatforms = settings.GetInstalledPlatforms(UMPSettings.Desktop | UMPSettings.Mobile);
        }

        if (_useAdvancedProp.boolValue)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            _cachedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 170;

            _useFixedSizeProp.boolValue = EditorGUILayout.Toggle("Use fixed video size:", _useFixedSizeProp.boolValue);

            if (_useFixedSizeProp.boolValue)
            {
                _fixedVideoWidthProp.intValue = EditorGUILayout.IntField(new GUIContent("Width: ", "Fixed video width."), _fixedVideoWidthProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Fixed video width."));
                _fixedVideoWidthProp.intValue = Mathf.Clamp(_fixedVideoWidthProp.intValue, 1, 7680);

                _fixedVideoHeightProp.intValue = EditorGUILayout.IntField(new GUIContent("Height: ", "Fixed video height."), _fixedVideoHeightProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Fixed video height."));
                _fixedVideoHeightProp.intValue = Mathf.Clamp(_fixedVideoHeightProp.intValue, 1, 7680);
            }
            else
            {
                _fixedVideoWidthProp.intValue = 0;
                _fixedVideoHeightProp.intValue = 0;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            if (_availablePlatforms == null || _availablePlatforms.Length <= 0)
                _availablePlatforms = settings.GetInstalledPlatforms(UMPSettings.Desktop | UMPSettings.Mobile);

            if (_availablePlatforms.Length <= 0)
            {
                var warningLabel = new GUIStyle(EditorStyles.textArea);
                warningLabel.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("Can't find 'UniversalMediaPlayer' asset folder, please check your Unity UMP preferences.", warningLabel);

                EditorStyles.label.normal.textColor = _cachedTextColor;
                EditorStyles.label.fontStyle = _cachedFontStyle;

                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();

                return;
            }

            _chosenPlatformProp.intValue = GUILayout.SelectionGrid(_chosenPlatformProp.intValue, _availablePlatforms, _availablePlatforms.Length, EditorStyles.miniButton);
            _chosenPlatformProp.intValue = Mathf.Clamp(_chosenPlatformProp.intValue, 0, _availablePlatforms.Length - 1);

            #region Desktop Options
            if (_availablePlatforms[_chosenPlatformProp.intValue] == UMPSettings.DESKTOP_CATEGORY_NAME)
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                #region Audio Option
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(_desktopAudioOutputsProp, new GUIContent("Audio Outputs:"), true);
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
                #endregion

                #region Decoding Options
                EditorGUILayout.PropertyField(_desktopHardwareDecodingProp, new GUIContent("Hardware decoding: ", "This allows hardware decoding when available."), false);

                if (_desktopHardwareDecodingProp.intValue == (int)PlayerOptions.States.Default)
                {
                    var hardwareDecodingName = "DirectX Video Acceleration (DXVA) 2.0";
                    if (UMPSettings.RuntimePlatform == UMPSettings.Platforms.Mac)
                        hardwareDecodingName = "Video Decode Acceleration Framework (VDA)";
                    if (UMPSettings.RuntimePlatform == UMPSettings.Platforms.Linux)
                        hardwareDecodingName = "VA-API video decoder via DRM";

                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", hardwareDecodingName));
                }

                if (_desktopHardwareDecodingProp.intValue == (int)PlayerOptions.States.Enable)
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Automatic"));
                EditorGUILayout.Space();
                #endregion

                #region Flip Options
                _desktopFlipVerticallyProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Flip vertically: ", "Flip video frame vertically when we get it from native library (CPU usage cost)."), _desktopFlipVerticallyProp.boolValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Flip video frame vertically when we get it from native library (CPU usage cost)."));
                #endregion

                #region Buffer Options
                _desktopVideoBufferSizeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Video Buffer Size: ", "To gain video resolution will be used special video buffer instead of size of video gotted directly from library."), _desktopVideoBufferSizeProp.boolValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "To gain video resolution will be used special video buffer instead of size of video gotted directly from library."));
                #endregion

                #region Dublicate Options
                _desktopOutputToFileProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Data to a file: ", "Duplicate the output stream and redirect it to a file (output file must have '.mp4' video file format)."), _desktopOutputToFileProp.boolValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Duplicate the output stream and redirect it to a file (output file must have '.mp4' video file format)."));

                if (_desktopOutputToFileProp.boolValue)
                {
                    _desktopDisplayOutputProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Display source video: ", "Display source video when duplicate data to a file."), _desktopDisplayOutputProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Display source video when duplicate data to a file."));

                    _desktopOutputFilePathProp.stringValue = EditorGUILayout.TextField(new GUIContent("Path to file: ", "Full path to a file where output data will be stored (example: 'C:\\Path\\To\\File\\Name.mp4')."), _desktopOutputFilePathProp.stringValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Full path to a file where output data will be stored (example: 'C:\\Path\\To\\File\\Name.mp4')."));
                }
                EditorGUILayout.Space();
                #endregion

                #region RTP/RTSP/SDP Options
                _desktopRtspOverTcpProp.boolValue = EditorGUILayout.Toggle(new GUIContent("RTP over RTSP (TCP): ", "Use RTP over RTSP (TCP) (HTTP default)."), _desktopRtspOverTcpProp.boolValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Use RTP over RTSP (TCP) (HTTP default)."));
                #endregion
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(umpEditor.IsReady);
                #region Caching Options
                _desktopFileCachingProp.intValue = EditorGUILayout.IntField(new GUIContent("File caching (ms): ", "Caching value for local files, in milliseconds."), _desktopFileCachingProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Caching value for local files, in milliseconds."));
                _desktopFileCachingProp.intValue = Mathf.Clamp(_desktopFileCachingProp.intValue, 0, 60000);

                _desktopLiveCachingProp.intValue = EditorGUILayout.IntField(new GUIContent("Live capture caching (ms): ", "Caching value for cameras and microphones, in milliseconds."), _desktopLiveCachingProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Caching value for cameras and microphones, in milliseconds."));
                _desktopLiveCachingProp.intValue = Mathf.Clamp(_desktopLiveCachingProp.intValue, 0, 60000);

                _desktopDiskCachingProp.intValue = EditorGUILayout.IntField(new GUIContent("Disc caching (ms): ", "Caching value for optical media, in milliseconds."), _desktopDiskCachingProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Caching value for optical media, in milliseconds."));
                _desktopDiskCachingProp.intValue = Mathf.Clamp(_desktopDiskCachingProp.intValue, 0, 60000);

                _desktopNetworkCachingProp.intValue = EditorGUILayout.IntField(new GUIContent("Network caching (ms): ", "Caching value for network resources, in milliseconds."), _desktopNetworkCachingProp.intValue);
                GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Caching value for network resources, in milliseconds."));
                _desktopNetworkCachingProp.intValue = Mathf.Clamp(_desktopNetworkCachingProp.intValue, 0, 60000);
                #endregion
                EditorGUI.EndDisabledGroup();
            }
            #endregion

            #region WebGL Options
            if (_availablePlatforms[_chosenPlatformProp.intValue] == UMPSettings.Platforms.WebGL.ToString())
            {
                var warningLabel = new GUIStyle(EditorStyles.textArea);
                warningLabel.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("Doesn't support any additional options for current platform in this version.", warningLabel);
            }
            #endregion

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            #region Android Options
            if (_availablePlatforms[_chosenPlatformProp.intValue] == UMPSettings.Platforms.Android.ToString())
            {
                #region Player Type Options
                EditorGUILayout.Space();

                var playersNames = new List<string>();
                var playerValues = (int[])Enum.GetValues(typeof(PlayerOptionsAndroid.PlayerTypes));
                var playerEnum = (PlayerOptionsAndroid.PlayerTypes)_androidPlayerTypeProp.intValue;
                var choosedPlayer = -1;

                for (int i = 0; i < playerValues.Length; i++)
                {
                    var playerType = (PlayerOptionsAndroid.PlayerTypes)playerValues[i];
                    if ((settings.PlayersAndroid & playerType) == playerType)
                    {
                        playersNames.Add(playerType.ToString());

                        if (playerEnum == playerType)
                            choosedPlayer = playersNames.Count - 1;
                    }
                }

                if (choosedPlayer < 0)
                    choosedPlayer = 0;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Player Type:", "Choose player type for current instance"), GUILayout.Width(80));
                EditorGUILayout.Space();
                choosedPlayer = GUILayout.SelectionGrid(choosedPlayer, playersNames.ToArray(), playersNames.Count, EditorStyles.miniButton, GUILayout.Width(playersNames.Count * 60));
                GUILayout.EndHorizontal();

                _androidPlayerTypeProp.intValue = (int)Enum.Parse(typeof(PlayerOptionsAndroid.PlayerTypes), playersNames[choosedPlayer]);
                _androidPlayerTypeProp.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                #endregion

                if (_androidPlayerTypeProp.intValue == (int)PlayerOptionsAndroid.PlayerTypes.LibVLC)
                {
                    #region Hardware Acceleration Options
                    EditorGUILayout.PropertyField(_androidHardwareAccelerationProp, new GUIContent("Hardware Acceleration: ", "This allows hardware acceleration when available:\n* Disabled: better stability.\n* Decoding: may improve performance.\n* Full: may improve performance further."), false);
                    if (_androidHardwareAccelerationProp.intValue == (int)PlayerOptionsAndroid.DecodingStates.Disabled)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Disabled: better stability."));

                    if (_androidHardwareAccelerationProp.intValue == (int)PlayerOptionsAndroid.DecodingStates.DecodingAcceleration)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Decoding: may improve performance."));

                    if (_androidHardwareAccelerationProp.intValue == (int)PlayerOptionsAndroid.DecodingStates.FullAcceleration)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Full: may improve performance further."));
                    EditorGUILayout.Space();
                    #endregion

                    #region OpenGL Decoding Options
                    EditorGUILayout.PropertyField(_androidOpenGLDecodingProp, new GUIContent("OpenGL Decoding: ", "OpenGL ES2 is used for software decoding and hardware decoding when needed (360° videos), but can affect on correct video rendering."), false);
                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptions.States.Default)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Automatic."));

                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptions.States.Disable)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Force Off."));

                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptions.States.Enable)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Force On."));
                    EditorGUILayout.Space();
                    #endregion

                    #region Video Chroma Options
                    EditorGUILayout.PropertyField(_androidVideoChromaProp, new GUIContent("Force video chroma: ", "* RGB 32-bit: default chroma\n* RGB 16-bit: better performance but lower quality\n* YUV: best performance but does not work on all devices. Android 2.3 and later only."), false);
                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptionsAndroid.ChromaTypes.RGB32Bit)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "RGB 32-bit: default chroma."));

                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptionsAndroid.ChromaTypes.RGB16Bit)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "RGB 16-bit: better performance but lower quality."));

                    if (_androidOpenGLDecodingProp.intValue == (int)PlayerOptionsAndroid.ChromaTypes.YUV)
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "YUV: best performance but does not work on all devices. Android 2.3 and later only."));
                    EditorGUILayout.Space();
                    #endregion

                    #region Background Options
                    _androidPlayInBackgroundProb.boolValue = EditorGUILayout.Toggle(new GUIContent("Play in background: ", "Continue play video when application in background."), _androidPlayInBackgroundProb.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Continue play video when application in background."));
                    #endregion

                    #region RTP/RTSP/SDP Options
                    _androidRtspOverTcpProp.boolValue = EditorGUILayout.Toggle(new GUIContent("RTP over RTSP (TCP): ", "Use RTP over RTSP (TCP) (HTTP default)."), _androidRtspOverTcpProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Use RTP over RTSP (TCP) (HTTP default)."));
                    #endregion

                    #region Caching Options
                    _androidNetworkCachingProp.intValue = EditorGUILayout.IntField(new GUIContent("Network caching (ms): ", "The amount of time to buffer network media (in ms). Does not work with hardware decoding. Leave '0' to reset."), _androidNetworkCachingProp.intValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "The amount of time to buffer network media (in ms). Does not work with hardware decoding. Leave '0' to reset."));
                    _androidNetworkCachingProp.intValue = Mathf.Clamp(_androidNetworkCachingProp.intValue, 0, 60000);
                    #endregion
                }
                else
                {
                    var warningLabel = new GUIStyle(EditorStyles.textArea);
                    warningLabel.fontStyle = FontStyle.Bold;
                    EditorGUILayout.LabelField("Doesn't support any additional options for '" + ((PlayerOptionsAndroid.PlayerTypes)_androidPlayerTypeProp.intValue).ToString() + "' player in this version.", warningLabel);
                }
            }
            #endregion
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            #region IPhone Options
            if (_availablePlatforms[_chosenPlatformProp.intValue] == UMPSettings.Platforms.iOS.ToString())
            {
                #region Player Type Options
                EditorGUILayout.Space();

                var playersNames = new List<string>();
                var playerValues = (int[])Enum.GetValues(typeof(PlayerOptionsIPhone.PlayerTypes));
                var playerEnum = (PlayerOptionsIPhone.PlayerTypes)_iphonePlayerTypeProp.intValue;
                var choosedPlayer = -1;

                for (int i = 0; i < playerValues.Length; i++)
                {
                    var playerType = (PlayerOptionsIPhone.PlayerTypes)playerValues[i];
                    if ((settings.PlayersIPhone & playerType) == playerType)
                    {
                        playersNames.Add(playerType.ToString());

                        if (playerEnum == playerType)
                            choosedPlayer = playersNames.Count - 1;
                    }
                }

                if (choosedPlayer < 0)
                    choosedPlayer = 0;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Player Type:", "Choose player type for current instance"), GUILayout.Width(80));
                EditorGUILayout.Space();
                choosedPlayer = GUILayout.SelectionGrid(choosedPlayer, playersNames.ToArray(), playersNames.Count, EditorStyles.miniButton, GUILayout.Width(playersNames.Count * 60));
                GUILayout.EndHorizontal();

                _iphonePlayerTypeProp.intValue = (int)Enum.Parse(typeof(PlayerOptionsIPhone.PlayerTypes), playersNames[choosedPlayer]);
                _iphonePlayerTypeProp.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                #endregion

                if (_iphonePlayerTypeProp.intValue == (int)PlayerOptionsIPhone.PlayerTypes.FFmpeg)
                {
                    #region VideoToolbox Options
                    _iphoneVideoToolboxProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Hardware decoding: ", "This allows hardware decoding when available (enable VideoToolbox decoding)."), _iphoneVideoToolboxProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "This allows hardware decoding when available (enable VideoToolbox decoding)."));

                    if (_iphoneVideoToolboxProp.boolValue)
                    {
                        _iphoneVideoToolboxMaxFrameWidthProp.intValue = EditorGUILayout.IntField(new GUIContent("Max width of output frame: ", "Max possible video resolution for hardware decoding."), _iphoneVideoToolboxMaxFrameWidthProp.intValue);
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Max possible video resolution for hardware decoding."));
                        _iphoneVideoToolboxMaxFrameWidthProp.intValue = Mathf.Clamp(_iphoneVideoToolboxMaxFrameWidthProp.intValue, 0, 32768);

                        _iphoneVideoToolboxAsyncProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Async decompression: ", "Use asynchronous decompression for hardware frame decoding."), _iphoneVideoToolboxAsyncProp.boolValue);
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Use asynchronous decompression for hardware frame decoding."));

                        if (_iphoneVideoToolboxAsyncProp.boolValue)
                        {
                            _iphoneVideoToolboxWaitAsyncProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Wait for asynchronous frames: ", "Wait when frames is ready."), _iphoneVideoToolboxWaitAsyncProp.boolValue);
                            GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Wait when frames is ready."));
                        }
                    }
                    EditorGUILayout.Space();
                    #endregion

                    #region Background Options
                    _iphonePlayInBackgroundProb.boolValue = EditorGUILayout.Toggle(new GUIContent("Play in background: ", "Continue play video when application in background."), _iphonePlayInBackgroundProb.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Continue play video when application in background."));
                    #endregion

                    #region RTP/RTSP/SDP Options
                    _iphoneRtspOverTcpProp.boolValue = EditorGUILayout.Toggle(new GUIContent("RTP over RTSP (TCP): ", "Use RTP over RTSP (TCP) (HTTP default)."), _iphoneRtspOverTcpProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Use RTP over RTSP (TCP) (HTTP default)."));
                    #endregion

                    #region Buffer Options
                    _iphonePacketBufferingProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Packet buffering: ", "Pause output until enough packets have been read after stalling."), _iphonePacketBufferingProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Pause output until enough packets have been read after stalling."));

                    _iphoneMaxBufferSizeProp.intValue = EditorGUILayout.IntField(new GUIContent("Max buffer size: ", "Max buffer size should be pre-read (in bytes)."), _iphoneMaxBufferSizeProp.intValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Max buffer size should be pre-read."));
                    _iphoneMaxBufferSizeProp.intValue = Mathf.Clamp(_iphoneMaxBufferSizeProp.intValue, 0, 15 * 1024 * 1024);

                    _iphoneMinFramesProp.intValue = EditorGUILayout.IntField(new GUIContent("Min frames: ", "Minimal frames to stop pre-reading."), _iphoneMinFramesProp.intValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Minimal frames to stop pre-reading."));
                    _iphoneMinFramesProp.intValue = Mathf.Clamp(_iphoneMinFramesProp.intValue, 5, 50000);

                    _iphoneInfbufProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Infbuf: ", "Don't limit the input buffer size (useful with realtime streams)."), _iphoneInfbufProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Don't limit the input buffer size (useful with realtime streams)."));

                    EditorGUILayout.Space();
                    #endregion

                    #region Frame Options
                    _iphoneFramedropProp.intValue = EditorGUILayout.IntField(new GUIContent("Framedrop: ", "Drop frames when cpu is too slow."), _iphoneFramedropProp.intValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Drop frames when cpu is too slow."));
                    _iphoneFramedropProp.intValue = Mathf.Clamp(_iphoneFramedropProp.intValue, -1, 120);

                    _iphoneMaxFpsProp.intValue = EditorGUILayout.IntField(new GUIContent("Max fps: ", "Drop frames in video whose fps is greater than max-fps."), _iphoneMaxFpsProp.intValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Drop frames in video whose fps is greater than max-fps."));
                    _iphoneMaxFpsProp.intValue = Mathf.Clamp(_iphoneMaxFpsProp.intValue, -1, 120);
                    #endregion
                }
                else if (_iphonePlayerTypeProp.intValue == (int)PlayerOptionsIPhone.PlayerTypes.Native)
                {
                    #region Flip Options
                    _iphoneFlipVerticallyProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Flip vertically: ", "Flip video frame vertically when we get it from native library (CPU usage cost)."), _iphoneFlipVerticallyProp.boolValue);
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Flip video frame vertically when we get it from native library (CPU usage cost)."));
                    #endregion
                }
                else
                {
                    var warningLabel = new GUIStyle(EditorStyles.textArea);
                    warningLabel.fontStyle = FontStyle.Bold;
                    EditorGUILayout.LabelField("Doesn't support any additional options for '" + ((PlayerOptionsAndroid.PlayerTypes)_iphonePlayerTypeProp.intValue).ToString() + "' player in this version.", warningLabel);
                }
            }
            #endregion
            EditorGUI.EndDisabledGroup();

            EditorGUIUtility.labelWidth = _cachedLabelWidth;
        }
        else
        {
            _desktopFileCachingProp.intValue = PlayerOptions.DEFAULT_CACHING_VALUE;
            _desktopLiveCachingProp.intValue = PlayerOptions.DEFAULT_CACHING_VALUE;
            _desktopDiskCachingProp.intValue = PlayerOptions.DEFAULT_CACHING_VALUE;
            _desktopNetworkCachingProp.intValue = PlayerOptions.DEFAULT_CACHING_VALUE;
            _fixedVideoWidthProp.intValue = -1;
            _fixedVideoHeightProp.intValue = -1;
        }

        if (settings.UseExternalLibraries)
        {
            if (_externalPath.Equals(string.Empty))
                _externalPath = settings.GetLibrariesPath(UMPSettings.RuntimePlatform, true);

            if (_externalPath != string.Empty)
            {
                var wrapTextStyle = EditorStyles.textArea;
                wrapTextStyle.wordWrap = true;
                EditorGUILayout.LabelField("Path to external/installed libraries: '" + _externalPath + "'", wrapTextStyle);
            }
        }
        else
        {
            _externalPath = string.Empty;
        }

        GUILayout.EndVertical();
        #endregion

        #region Player Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Player properties:");
        EditorStyles.label.fontStyle = _cachedFontStyle;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
		EditorGUILayout.LabelField("Volume", centeredStyle, GUILayout.MinWidth(80));
        if (GUILayout.Button("x", EditorStyles.miniButton))
        {
            _volumeProp.intValue = 50;
        }
        GUILayout.EndHorizontal();

        _volumeProp.intValue = EditorGUILayout.IntSlider(_volumeProp.intValue, 0, 100);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Play rate", centeredStyle, GUILayout.MinWidth(80));
        if (GUILayout.Button("x", EditorStyles.miniButton))
        {
            _playRateProp.floatValue = 1f;
        }
        GUILayout.EndHorizontal();

        _playRateProp.floatValue = EditorGUILayout.Slider(_playRateProp.floatValue, 0.5f, 5f);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!umpEditor.IsReady);
        EditorGUILayout.Space();
        GUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Position", centeredStyle, GUILayout.MinWidth(100));
        _positionProp.floatValue = EditorGUILayout.Slider(_positionProp.floatValue, 0f, 1f);
        GUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!Application.isPlaying || !umpEditor.isActiveAndEnabled || umpEditor.IsParsing);
        GUILayout.BeginHorizontal("Box");
		if (GUILayout.Button("LOAD", GUILayout.MinWidth(40)))
        {
            umpEditor.Prepare();
        }
		if (GUILayout.Button("PLAY", GUILayout.MinWidth(40)))
        {
            umpEditor.Play();
        }
		if (GUILayout.Button("PAUSE", GUILayout.MinWidth(40)))
        {
            umpEditor.Pause();
        }
		if (GUILayout.Button("STOP", GUILayout.MinWidth(40)))
        {
            umpEditor.Stop();
        }
		if (GUILayout.Button("SHOT", GUILayout.MinWidth(40)))
        {
            umpEditor.Snapshot(Application.persistentDataPath);
        }
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        #endregion

        #region Events & Logging Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
		EditorGUILayout.LabelField("Events & Logging:");
        EditorStyles.label.fontStyle = _cachedFontStyle;

        GUILayout.BeginVertical("Box");
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUILayout.PropertyField(_logDetailProp, GUILayout.MinWidth(50));
        EditorGUI.EndDisabledGroup();

        GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Last msg: ", GUILayout.MinWidth(70));
        EditorStyles.label.normal.textColor = Color.black;
        EditorStyles.label.fontStyle = FontStyle.Italic;
		EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField(_lastEventMsgProp.stringValue, GUILayout.MaxWidth(100));
        EditorStyles.label.normal.textColor = _cachedTextColor;
        EditorStyles.label.fontStyle = _cachedFontStyle;
		EditorStyles.label.wordWrap = _cachedLabelWordWrap;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        _showEventsListeners = EditorGUILayout.Foldout(_showEventsListeners, "Event Listeners");

        if (_showEventsListeners)
        {
			EditorGUILayout.PropertyField(_pathPreparedEventProp, new GUIContent("Path Prepared"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_openingEventProp, new GUIContent("Opening"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_bufferingEventProp, new GUIContent("Buffering"), true, GUILayout.MinWidth(50));
            EditorGUILayout.PropertyField(_imageReadyEventProp, new GUIContent("ImageReady"), true, GUILayout.MinWidth(50));
            EditorGUILayout.PropertyField(_preparedEventProp, new GUIContent("Prepared"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_playingEventProp, new GUIContent("Playing"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_pausedEventProp, new GUIContent("Paused"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_stoppedEventProp, new GUIContent("Stopped"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_endReachedEventProp, new GUIContent("End Reached"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_encounteredErrorEventProp, new GUIContent("Encountered Error"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_timeChangedEventProp, new GUIContent("Time Changed"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_positionChangedEventProp, new GUIContent("Position Changed"), true, GUILayout.MinWidth(50));
			EditorGUILayout.PropertyField(_snapshotTakenEventProp, new GUIContent("Snapshot"), true, GUILayout.MinWidth(50));
        }
        #endregion

        EditorStyles.label.normal.textColor = _cachedTextColor;
        EditorStyles.label.fontStyle = _cachedFontStyle;

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
