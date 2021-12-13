using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UMP.Services;
using UMP.Services.Youtube;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMP
{
    public class UniversalMediaPlayer : MonoBehaviour, IMediaListener, IPathPreparedListener, IPlayerPreparedListener, IPlayerTimeChangedListener, IPlayerPositionChangedListener, IPlayerSnapshotTakenListener
    {
        private const float DEFAULT_POSITION_CHANGED_OFFSET = 0.05f;

        #region Editor Visible Properties
        [SerializeField]
        private GameObject[] _renderingObjects;

        [SerializeField]
        private string _path = string.Empty;

        [SerializeField]
        private bool _autoPlay = false;

        [SerializeField]
        private bool _loop = false;

        [SerializeField]
        private bool _loopSmooth = false;

        [SerializeField]
        private bool _mute = false;

#pragma warning disable 0414
        [SerializeField]
        private bool _useAdvanced = false;

        [SerializeField]
        private bool _useFixedSize = false;

        [SerializeField]
        private int _fixedVideoWidth = -1;

        [SerializeField]
        private int _fixedVideoHeight = -1;

        [SerializeField]
        private int _chosenPlatform = 0;

        [SerializeField]
        private int _volume = 50;

        [SerializeField]
        private float _playRate = 1;

        [SerializeField]
        private float _position = 0;

        [SerializeField]
        private LogLevels _logDetail = LogLevels.Disable;

        [SerializeField]
        private string _lastEventMsg = string.Empty;
#pragma warning restore 0414

        #region Desktop Options
        [SerializeField]
        private AudioOutput[] _desktopAudioOutputs;

        [SerializeField]
        private PlayerOptions.States _desktopHardwareDecoding = PlayerOptions.States.Default;

        [SerializeField]
        private bool _desktopFlipVertically = true;

        [SerializeField]
        private bool _desktopVideoBufferSize = false;

        [SerializeField]
        private bool _desktopOutputToFile = false;

        [SerializeField]
        private bool _desktopDisplayOutput = false;

        [SerializeField]
        private string _desktopOutputFilePath = string.Empty;

        [SerializeField]
        private bool _desktopRtspOverTcp = false;

        [SerializeField]
        private int _desktopFileCaching = 300;

        [SerializeField]
        private int _desktopLiveCaching = 300;

        [SerializeField]
        private int _desktopDiskCaching = 300;

        [SerializeField]
        private int _desktopNetworkCaching = 300;
        #endregion

        #region Android Options
        [SerializeField]
        private PlayerOptionsAndroid.PlayerTypes _androidPlayerType = PlayerOptionsAndroid.PlayerTypes.LibVLC;

        [SerializeField]
        private PlayerOptionsAndroid.DecodingStates _androidHardwareAcceleration = PlayerOptionsAndroid.DecodingStates.Automatic;

        [SerializeField]
        private PlayerOptions.States _androidOpenGLDecoding = PlayerOptions.States.Disable;

        [SerializeField]
        private PlayerOptionsAndroid.ChromaTypes _androidVideoChroma = PlayerOptionsAndroid.ChromaTypes.RGB16Bit;

        [SerializeField]
        private bool _androidPlayInBackground = false;

        [SerializeField]
        private bool _androidRtspOverTcp = false;

        [SerializeField]
        private int _androidNetworkCaching = 300;
        #endregion

        #region IPhone Options
        [SerializeField]
        private PlayerOptionsIPhone.PlayerTypes _iphonePlayerType = PlayerOptionsIPhone.PlayerTypes.FFmpeg;

        [SerializeField]
        private bool _iphoneFlipVertically = true;

        [SerializeField]
        private bool _iphoneVideoToolbox = true;

        [SerializeField]
        private int _iphoneVideoToolboxMaxFrameWidth = 4096;

        [SerializeField]
        private bool _iphoneVideoToolboxAsync = false;

        [SerializeField]
        private bool _iphoneVideoToolboxWaitAsync = true;

        [SerializeField]
        private bool _iphonePlayInBackground = false;

        [SerializeField]
        private bool _iphoneRtspOverTcp = false;

        [SerializeField]
        private bool _iphonePacketBuffering = true;

        [SerializeField]
        private int _iphoneMaxBufferSize = 15 * 1024 * 1024;

        [SerializeField]
        private int _iphoneMinFrames = 50000;

        [SerializeField]
        private bool _iphoneInfbuf = false;

        [SerializeField]
        private int _iphoneFramedrop = 0;

        [SerializeField]
        private int _iphoneMaxFps = 31;
        #endregion

        [Serializable]
        private class EventTextType : UnityEvent<string> { }

        [Serializable]
        private class EventFloatType : UnityEvent<float> { }

        [Serializable]
        private class EventLongType : UnityEvent<long> { }

        [Serializable]
        private class EventSizeType : UnityEvent<int, int> { }

        [Serializable]
        private class EventTextureType : UnityEvent<Texture2D> { }

        [SerializeField]
        private EventTextType _pathPreparedEvent = new EventTextType();

        [SerializeField]
        private UnityEvent _openingEvent = new UnityEvent();

        [SerializeField]
        private EventFloatType _bufferingEvent = new EventFloatType();

        [SerializeField]
        private EventTextureType _imageReadyEvent = new EventTextureType();

        [SerializeField]
        private EventSizeType _preparedEvent = new EventSizeType();

        [SerializeField]
        private UnityEvent _playingEvent = new UnityEvent();

        [SerializeField]
        private UnityEvent _pausedEvent = new UnityEvent();

        [SerializeField]
        private UnityEvent _stoppedEvent = new UnityEvent();

        [SerializeField]
        private UnityEvent _endReachedEvent = new UnityEvent();

        [SerializeField]
        private UnityEvent _encounteredErrorEvent = new UnityEvent();

        [SerializeField]
        private EventLongType _timeChangedEvent = new EventLongType();

        [SerializeField]
        private EventFloatType _positionChangedEvent = new EventFloatType();

        [SerializeField]
        private EventTextType _snapshotTakenEvent = new EventTextType();
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set simple array that consist with Unity 'GameObject' that have 'Mesh Renderer' (with some material)
        /// or 'Raw Image' component
        /// </summary>
        public GameObject[] RenderingObjects
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.VideoOutputObjects;
                return null;
            }
            set
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.VideoOutputObjects = value;

                _renderingObjects = value;
            }
        }

        /// <summary>
        /// Get simple array that consist with Unity 'AudioSource' components
        /// (will be used for audio data output instead default one)
        /// * Warning: in current stage correctly working only with audio that has 2 channels
        /// </summary>
        public AudioOutput[] AudioOutputs
        {
            get
            {
                return _desktopAudioOutputs;
            }
        }

        /// <summary>
        /// Get media player object for current running platform 
        /// (supported: Standalone, WebGL, Android and iOS platforms)
        /// for get more additional possibilities that exists only for this platform.
        /// Example of using:
        /// if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
        ///     return (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).GetLastError();
        /// </summary>
        public object PlatformPlayer
        {
            get
            {
                return _mediaPlayer != null ? _mediaPlayer.PlatformPlayer : null;
            }
        }

        /// <summary>
        /// Get/Set local path or url link to your video/audio file/stream
        /// Example of using:
        /// Local storage space - 'file:///C:\MyFolder\Videos\video1.mp4' or 'C:\MyFolder\Videos\video1.mp4' or 
        /// 'file:///DCIM/100ANDRO/MyVideo.mp4' (example for Android platform);
        /// Remote space (streams) - 'rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov';
        /// 'StreamingAssets' folder - 'file:///myVideoFile.mp4';
        /// </summary>
        public string Path
        {
            set { _path = value; }
            get { return _path; }
        }

        /// <summary>
        /// Get/Set start playback automatically after video is buffered
        /// </summary>
        public bool AutoPlay
        {
            set { _autoPlay = value; }
            get { return _autoPlay; }
        }

        /// <summary>
        /// Get/Set jumps to the start and plays again if playback reaches the end position
        /// </summary>
        public bool Loop
        {
            set { _loop = value; }
            get { return _loop; }
        }

        /// <summary>
        /// Get/Set mute status for current video playback
        /// </summary>
        public bool Mute
        {
            set
            {
                _mediaPlayer.Mute = value;
                _mute = value;
            }
            get { return _mediaPlayer.Mute; }
        }

        /// <summary>
        /// Get/Set current software audio volume 
        /// (by default you can change this value from '0' to '100')
        /// </summary>
        public float Volume
        {
            set
            {
                _mediaPlayer.Volume = (int)value;
                _volume = (int)value;
            }
            get { return _mediaPlayer.Volume; }
        }

        /// <summary>
        /// Get/Set video position.
        /// This has no effect if playback is not enabled.
        /// This might not work depending on the underlying input format and protocol
        /// </summary>
        public float Position
        {
            set { _mediaPlayer.Position = value; }
            get { return _mediaPlayer.Position; }
        }

        /// <summary>
        /// Get/Set the current video time (in milliseconds). 
        /// This has no effect if no media is being played. 
        /// Not all formats and protocols support this
        /// </summary>
        public long Time
        {
            set
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.Time = value;
            }
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.Time;
                return -1;
            }
        }

        /// <summary>
        /// Get/Set the requested video play rate
        /// </summary>
        public float PlayRate
        {
            set
            {
                _mediaPlayer.PlaybackRate = value;
                _playRate = value;
            }
            get { return _mediaPlayer.PlaybackRate; }
        }

        /// <summary>
        /// Is the player able to play
        /// </summary>
        public bool AbleToPlay
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.AbleToPlay;
                return false;
            }
        }

        /// <summary>
        /// Is media is currently playing
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.IsPlaying;
                return false;
            }
        }

        /// <summary>
        /// Is media is ready to play (first frame available)
        /// </summary>
        public bool IsReady
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.IsReady;
                return false;
            }
        }

        /// <summary>
        /// Is current media is parsing from video hosting service (in our case Youtube)
        /// </summary>
        public bool IsParsing
        {
            get
            {
                return _isParsing;
            }
        }

        /// <summary>
        /// Get frames per second (fps) for current video playback.
        /// * Warning: it's not a predefined value from video file/stream - calculated in video playback process
        /// </summary>
        public float FrameRate
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.FrameRate;
                return 0;
            }
        }

        /// <summary>
        /// Get video frames counter
        /// </summary>
        public long FramesCounter
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.FramesCounter;
                return 0;
            }
        }

        /// <summary>
        /// Get pixels of current video frame
        /// Example of using:
        /// texture.LoadRawTextureData(_player.FramePixels);
        /// texture.Apply();
        /// </summary>
        public byte[] FramePixels
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.FramePixels;
                return null;
            }
        }

        /// <summary>
        /// Get the current video length (in milliseconds)
        /// </summary>
        public long Length
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.Length;
                return 0;
            }
        }

        /// <summary>
        /// Get current video width in pixels
        /// </summary>
        public int VideoWidth
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.VideoWidth;
                return 0;
            }
        }

        /// <summary>
        /// Get current video height in pixels
        /// </summary>
        public int VideoHeight
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.VideoHeight;
                return 0;
            }
        }

        /// <summary>
        /// Get the pixel dimensions of current video
        /// </summary>
        public Vector2 VideoSize
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.VideoSize;
                return new Vector2(0, 0);
            }
        }

        /// <summary>
        /// Get/Set the current audio track
        /// </summary>
        public MediaTrackInfo AudioTrack
        {
            set
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.AudioTrack = value;
            }
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.AudioTrack;
                return null;
            }
        }

        /// <summary>
        /// Get the available audio tracks
        /// </summary>
        public MediaTrackInfo[] AudioTracks
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.AudioTracks;
                return null;
            }
        }

        /// <summary>
        /// Get/Set the current spu (subtitle) track (supported only on Standalone platform)
        /// </summary>
        public MediaTrackInfo SpuTrack
        {
            set
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.SpuTrack = value;
            }
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.SpuTrack;
                return null;
            }
        }

        /// <summary>
        /// Gets the available spu (subtitle) tracks (supported only on Standalone platform)
        /// </summary>
        public MediaTrackInfo[] SpuTracks
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.SpuTracks;
                return null;
            }
        }

        /// <summary>
        /// Get event manager for current media player to add possibility to attach/detach special playback listeners
        /// </summary>
        public PlayerManagerEvents EventManager
        {
            get
            {
                if (_mediaPlayer != null)
                    return _mediaPlayer.EventManager;
                return null;
            }
        }
        #endregion

        private MediaPlayer _mediaPlayer;
        private MediaPlayer _mediaPlayerLoop;
        private VideoServices _videoServices;

        private string _tmpPath = string.Empty;
        private bool _isParsing;
        private static bool _isExportCompleted;
        private static Dictionary<string, string> _cachedVideoPaths = new Dictionary<string, string>();

        private static IEnumerator _exportedHandlerEnum;
        private IEnumerator _videoPathPreparingEnum;

#pragma warning disable 0414
        private bool _isFirstEditorStateChange = true;
#pragma warning restore 0414

        private void Awake()
        {
#if UNITY_EDITOR
#if UNITY_4 || UNITY_5 || UNITY_2017_1
            EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
#else
        EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
#endif
#endif

            if (UMPSettings.Instance.UseAudioSource && (_desktopAudioOutputs == null || _desktopAudioOutputs.Length <= 0))
            {
                var audioOutput = gameObject.AddComponent<UMPAudioOutput>();
                _desktopAudioOutputs = new UMPAudioOutput[] { audioOutput };
            }

            PlayerOptions options = new PlayerOptions(null);

            switch (UMPSettings.RuntimePlatform)
            {
                case UMPSettings.Platforms.Win:
                case UMPSettings.Platforms.Mac:
                case UMPSettings.Platforms.Linux:
                    var standaloneOptions = new PlayerOptionsStandalone(null)
                    {
                        FixedVideoSize = _useFixedSize ? new Vector2(_fixedVideoWidth, _fixedVideoHeight) : Vector2.zero,
                        AudioOutputs = _desktopAudioOutputs,
                        //DirectAudioDevice = "Digital Audio",
                        HardwareDecoding = _desktopHardwareDecoding,
                        FlipVertically = _desktopFlipVertically,
                        VideoBufferSize = _desktopVideoBufferSize,
                        UseTCP = _desktopRtspOverTcp,
                        FileCaching = _desktopFileCaching,
                        LiveCaching = _desktopLiveCaching,
                        DiskCaching = _desktopDiskCaching,
                        NetworkCaching = _desktopNetworkCaching
                    };

                    if (_desktopOutputToFile)
                        standaloneOptions.RedirectToFile(_desktopDisplayOutput, _desktopOutputFilePath);

                    standaloneOptions.SetLogDetail(_logDetail, UnityConsoleLogging);
                    options = standaloneOptions;
                    break;

                case UMPSettings.Platforms.Android:
                    var androidOptions = new PlayerOptionsAndroid(null)
                    {
                        FixedVideoSize = _useFixedSize ? new Vector2(_fixedVideoWidth, _fixedVideoHeight) : Vector2.zero,
                        PlayerType = _androidPlayerType,
                        HardwareAcceleration = _androidHardwareAcceleration,
                        OpenGLDecoding = _androidOpenGLDecoding,
                        VideoChroma = _androidVideoChroma,
                        PlayInBackground = _androidPlayInBackground,
                        UseTCP = _androidRtspOverTcp,
                        NetworkCaching = _androidNetworkCaching
                    };

                    options = androidOptions;

                    if (_exportedHandlerEnum == null)
                    {
                        _exportedHandlerEnum = AndroidExpoterdHandler();
                        StartCoroutine(_exportedHandlerEnum);
                    }

                    break;

                case UMPSettings.Platforms.iOS:
                    var iphoneOptions = new PlayerOptionsIPhone(null)
                    {
                        FixedVideoSize = _useFixedSize ? new Vector2(_fixedVideoWidth, _fixedVideoHeight) : Vector2.zero,
                        PlayerType = _iphonePlayerType,
                        FlipVertically = _iphoneFlipVertically,
                        VideoToolbox = _iphoneVideoToolbox,
                        VideoToolboxFrameWidth = _iphoneVideoToolboxMaxFrameWidth,
                        VideoToolboxAsync = _iphoneVideoToolboxAsync,
                        VideoToolboxWaitAsync = _iphoneVideoToolboxWaitAsync,
                        PlayInBackground = _iphonePlayInBackground,
                        UseTCP = _iphoneRtspOverTcp,
                        PacketBuffering = _iphonePacketBuffering,
                        MaxBufferSize = _iphoneMaxBufferSize,
                        MinFrames = _iphoneMinFrames,
                        Infbuf = _iphoneInfbuf,
                        Framedrop = _iphoneFramedrop,
                        MaxFps = _iphoneMaxFps
                    };

                    options = iphoneOptions;
                    break;
            }

            _mediaPlayer = new MediaPlayer(this, _renderingObjects, options);

            // Create scpecial parser to add possibiity of get video link from different video hosting servies (like youtube)
            _videoServices = new VideoServices(this);

            // Attach scecial listeners to MediaPlayer instance
            AddListeners();
            // Create additional media player for add smooth loop possibility
            if (_loopSmooth)
            {
                _mediaPlayerLoop = new MediaPlayer(this, _mediaPlayer);
                _mediaPlayerLoop.VideoOutputObjects = null;
                _mediaPlayerLoop.EventManager.RemoveAllEvents();
            }
        }

        private IEnumerator AndroidExpoterdHandler()
        {
            var settings = UMPSettings.Instance;

            foreach (var exportedPath in settings.AndroidExportedPaths)
            {
                var tempFilePath = System.IO.Path.Combine(Application.temporaryCachePath, exportedPath);
                var saPath = "Assets" + System.IO.Path.AltDirectorySeparatorChar + "StreamingAssets" + System.IO.Path.AltDirectorySeparatorChar;
                var localFilePath = exportedPath.Replace(saPath, "");

                if (File.Exists(tempFilePath))
                {
                    _cachedVideoPaths.Add(localFilePath, tempFilePath);
                    continue;
                }

                var data = new byte[0];

#if UNITY_2017_2_OR_NEWER
                var www = UnityWebRequest.Get(System.IO.Path.Combine(Application.streamingAssetsPath, localFilePath));
                yield return www.SendWebRequest();
                data = www.downloadHandler.data;
#else
                var www = new WWW(System.IO.Path.Combine(Application.streamingAssetsPath, localFilePath));
                yield return www;
                data = www.bytes;
#endif

                if (string.IsNullOrEmpty(www.error))
                {
                    var tempFile = new FileInfo(tempFilePath);
                    tempFile.Directory.Create();
                    File.WriteAllBytes(tempFile.FullName, data);
                    _cachedVideoPaths.Add(localFilePath, tempFilePath);
                }
                else
                {
                    Debug.LogError("Can't create temp file from asset folder: " + www.error);
                }

                www.Dispose();
            }

            _isExportCompleted = true;
        }

        #region Editor Additional Possibility
#if UNITY_EDITOR
        private bool _cachedPlayState;

#if UNITY_4 || UNITY_5 || UNITY_2017_1
        //Used for defined in new Unity version 'playmodeStateChanged' method
        private void HandleOnPlayModeChanged()
#else
    //Used for new 'playModeStateChanged' method that available in new Unity 2017.2+ version
    private void HandleOnPlayModeChanged(PlayModeStateChange modeState)
#endif
        {
            if (_isFirstEditorStateChange)
            {
                _isFirstEditorStateChange = false;
                return;
            }

            if (_mediaPlayer == null)
                return;

            if (EditorApplication.isPaused)
            {
                _cachedPlayState = _mediaPlayer.IsPlaying;
                Pause();
            }
            else
            {
                if (!isActiveAndEnabled)
                {
                    Stop();
                    return;
                }

                if (_cachedPlayState)
                {
                    _mediaPlayer.Play();
                }
                else
                {
                    Pause();
                }
            }
        }

        private void OnValidate()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsReady)
            {
                if (_mediaPlayer.Mute != _mute)
                    _mediaPlayer.Mute = _mute;

                if (_mediaPlayer.Volume != _volume)
                    _mediaPlayer.Volume = _volume;

                if (_mediaPlayer.PlaybackRate != _playRate)
                    _mediaPlayer.PlaybackRate = _playRate;

                if (_position > _mediaPlayer.Position + DEFAULT_POSITION_CHANGED_OFFSET ||
                    _position < _mediaPlayer.Position - DEFAULT_POSITION_CHANGED_OFFSET)
                {
                    _mediaPlayer.Position = _position;
                }
            }
        }
#endif
        #endregion

        private void Start()
        {
            if (!_autoPlay)
                return;

            Play();
        }

        private void OnDisable()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                Stop();
            }
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
#if UNITY_4 || UNITY_5 || UNITY_2017_1
            EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged;
#else
        EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
#endif
#endif

            if (_mediaPlayer != null)
            {
                // Release MediaPlayer
                Release();
            }
        }

        private void AddListeners()
        {
            if (_mediaPlayer == null || _mediaPlayer.EventManager == null)
                return;

            // Add to MediaPlayer new main group of listeners
            _mediaPlayer.AddMediaListener(this);
            // Add to MediaPlayer new "OnPlayerTimeChanged" listener
            _mediaPlayer.EventManager.PlayerTimeChangedListener += OnPlayerTimeChanged;
            // Add to MediaPlayer new "OnPlayerPositionChanged" listener
            _mediaPlayer.EventManager.PlayerPositionChangedListener += OnPlayerPositionChanged;
            // Add to MediaPlayer new "OnPlayerSnapshotTaken" listener
            _mediaPlayer.EventManager.PlayerSnapshotTakenListener += OnPlayerSnapshotTaken;
        }

        private void RemoveListeners()
        {
            if (_mediaPlayer == null)
                return;

            // Remove from MediaPlayer the main group of listeners
            _mediaPlayer.RemoveMediaListener(this);
            // Remove from MediaPlayer "OnPlayerTimeChanged" listener
            _mediaPlayer.EventManager.PlayerTimeChangedListener -= OnPlayerTimeChanged;
            // Remove from MediaPlayer "OnPlayerPositionChanged" listener
            _mediaPlayer.EventManager.PlayerPositionChangedListener -= OnPlayerPositionChanged;
            // Remove from MediaPlayer new "OnPlayerSnapshotTaken" listener
            _mediaPlayer.EventManager.PlayerSnapshotTakenListener -= OnPlayerSnapshotTaken;
        }

        private IEnumerator VideoPathPreparing(string path, bool playImmediately, IPathPreparedListener listener)
        {
            if (_cachedVideoPaths.ContainsKey(path))
            {
                listener.OnPathPrepared(_cachedVideoPaths[path], playImmediately);
                yield break;
            }

#if UNITY_EDITOR
            _lastEventMsg = "Path Preparing";
#endif

            if (UMPSettings.RuntimePlatform == UMPSettings.Platforms.Android)
            {
                /// Check if we try to play exported videos and wait when export process will be completed
                var exptPaths = UMPSettings.Instance.AndroidExportedPaths;
                var filePath = path.Replace("file:///", "");

                foreach (var exptPath in exptPaths)
                {
                    if (exptPath.Contains(filePath))
                    {
                        while (!_isExportCompleted)
                            yield return null;

                        if (_cachedVideoPaths.ContainsKey(filePath))
                        {
                            listener.OnPathPrepared(_cachedVideoPaths[filePath], playImmediately);
                            yield break;
                        }

                        break;
                    }
                }

                if ((_mediaPlayer.Options as PlayerOptionsAndroid).PlayerType ==
                    PlayerOptionsAndroid.PlayerTypes.LibVLC
                    && MediaPlayerHelper.IsAssetsFile(path))
                {
                    var tempFilePath = System.IO.Path.Combine(Application.temporaryCachePath, filePath);
                    if (File.Exists(tempFilePath))
                    {
                        _cachedVideoPaths.Add(path, tempFilePath);
                        listener.OnPathPrepared(tempFilePath, playImmediately);
                        yield break;
                    }

                    var data = new byte[0];

#if UNITY_2017_2_OR_NEWER
                    var www = UnityWebRequest.Get(System.IO.Path.Combine(Application.streamingAssetsPath, filePath));
                    yield return www.SendWebRequest();
                    data = www.downloadHandler.data;
#else
                    var www = new WWW(System.IO.Path.Combine(Application.streamingAssetsPath, filePath));
                    yield return www;
                    data = www.bytes;
#endif
                    if (string.IsNullOrEmpty(www.error))
                    {
                        var tempFile = new FileInfo(tempFilePath);
                        tempFile.Directory.Create();
                        File.WriteAllBytes(tempFile.FullName, data);
                        _cachedVideoPaths.Add(path, tempFilePath);
                        path = tempFilePath;
                    }
                    else
                    {
                        Debug.LogError("Can't create temp file from asset folder: " + www.error);
                    }

                    www.Dispose();
                }
            }

            if (_videoServices.ValidUrl(path))
            {
                Video serviceVideo = null;
                _isParsing = true;

                yield return _videoServices.GetVideos(path, (videos) =>
                {
                    _isParsing = false;
                    serviceVideo = VideoServices.FindVideo(videos, int.MaxValue, int.MaxValue);
                }, (error) =>
                {
                    _isParsing = false;
                    Debug.LogError(string.Format("[UniversalMediaPlayer.GetVideos] {0}", error));
                    OnPlayerEncounteredError();
                });

                if (serviceVideo == null)
                {
                    Debug.LogError("[UniversalMediaPlayer.VideoPathPreparing] Can't get service video information");
                    OnPlayerEncounteredError();
                }
                else
                {
                    if (serviceVideo is YoutubeVideo)
                    {
                        yield return (serviceVideo as YoutubeVideo).Decrypt(UMPSettings.Instance.YoutubeDecryptFunction, (error) =>
                        {
                            Debug.LogError(string.Format("[UniversalMediaPlayer.Decrypt] {0}", error));
                            OnPlayerEncounteredError();
                        });
                    }

                    path = serviceVideo.Url;
                }
            }

            listener.OnPathPrepared(path, playImmediately);
            yield return null;
        }

        public void Prepare()
        {
            if (_videoPathPreparingEnum != null)
                StopCoroutine(_videoPathPreparingEnum);

            _videoPathPreparingEnum = VideoPathPreparing(_path, false, this);
            StartCoroutine(_videoPathPreparingEnum);
        }

        public void Play()
        {
            if (_videoPathPreparingEnum != null)
                StopCoroutine(_videoPathPreparingEnum);

            _videoPathPreparingEnum = VideoPathPreparing(_path, true, this);
            StartCoroutine(_videoPathPreparingEnum);
        }

        public void Pause()
        {
            if (_mediaPlayer == null)
                return;

            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Pause();
        }

        public void Stop()
        {
            Stop(true);
        }

        public void Stop(bool clearVideoTexture)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPaused)
                return;
#endif
            if (_videoPathPreparingEnum != null)
                StopCoroutine(_videoPathPreparingEnum);

            _position = 0;

            if (_mediaPlayer == null)
                return;

            _mediaPlayer.Stop(clearVideoTexture);

            if (_mediaPlayerLoop != null)
                _mediaPlayerLoop.Stop(clearVideoTexture);
        }

        public void Release()
        {
            Stop();

            if (_mediaPlayer != null)
            {
                // Release MediaPlayer
                _mediaPlayer.Release();
                _mediaPlayer = null;

                if (_mediaPlayerLoop != null)
                    _mediaPlayerLoop.Release();

                RemoveListeners();

                _openingEvent.RemoveAllListeners();
                _bufferingEvent.RemoveAllListeners();
                _imageReadyEvent.RemoveAllListeners();
                _preparedEvent.RemoveAllListeners();
                _playingEvent.RemoveAllListeners();
                _pausedEvent.RemoveAllListeners();
                _stoppedEvent.RemoveAllListeners();
                _endReachedEvent.RemoveAllListeners();
                _encounteredErrorEvent.RemoveAllListeners();
                _timeChangedEvent.RemoveAllListeners();
                _positionChangedEvent.RemoveAllListeners();
                _snapshotTakenEvent.RemoveAllListeners();
            }
        }

        public string GetFormattedLength(bool detail)
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.GetFormattedLength(detail);
            return string.Empty;
        }

        public void Snapshot(string path)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPaused)
                return;
#endif

            if (_mediaPlayer == null)
                return;

            if (_mediaPlayer.AbleToPlay)
            {
                if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
                    (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).TakeSnapShot(path);
#if UNITY_EDITOR
                Debug.Log("Snapshot path: " + path);
#endif
            }
        }

        private void UnityConsoleLogging(PlayerManagerLogs.PlayerLog args)
        {
            if (args.Level != _logDetail)
                return;

            Debug.Log(args.Level.ToString() + ": " + args.Message);
        }

        public void OnPathPrepared(string path, bool playImmediately)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPaused)
                return;
#endif

            _mediaPlayer.Mute = _mute;
            _mediaPlayer.Volume = _volume;
            _mediaPlayer.PlaybackRate = _playRate;

            if (!_path.Equals(_tmpPath))
            {
                if (IsPlaying)
                    Stop();

                _tmpPath = _path;
                _mediaPlayer.DataSource = path;
            }

            if (!playImmediately)
                _mediaPlayer.Prepare();
            else
                _mediaPlayer.Play();

            if (_mediaPlayerLoop != null && !_mediaPlayerLoop.IsReady)
            {
                _mediaPlayerLoop.DataSource = _mediaPlayer.DataSource;
                _mediaPlayerLoop.Prepare();
            }

            if (_pathPreparedEvent != null)
                _pathPreparedEvent.Invoke(path);
        }

        public void AddPathPreparedEvent(UnityAction<string> action)
        {
            _pathPreparedEvent.AddListener(action);
        }

        public void RemovePathPreparedEvent(UnityAction<string> action)
        {
            _pathPreparedEvent.RemoveListener(action);
        }

        public void OnPlayerOpening()
        {
#if UNITY_EDITOR
            _lastEventMsg = "Opening";
#endif
            if (_openingEvent != null)
                _openingEvent.Invoke();
        }

        public void AddOpeningEvent(UnityAction action)
        {
            _openingEvent.AddListener(action);
        }

        public void RemoveOpeningEvent(UnityAction action)
        {
            _openingEvent.RemoveListener(action);
        }

        public void OnPlayerBuffering(float percentage)
        {
#if UNITY_EDITOR
            _lastEventMsg = "Buffering: " + percentage;
#endif
            if (_bufferingEvent != null)
                _bufferingEvent.Invoke(percentage);
        }

        public void AddBufferingEvent(UnityAction<float> action)
        {
            _bufferingEvent.AddListener(action);
        }

        public void RemoveBufferingEvent(UnityAction<float> action)
        {
            _bufferingEvent.RemoveListener(action);
        }

        public void OnPlayerImageReady(Texture2D image)
        {
#if UNITY_EDITOR
            _lastEventMsg = "ImageReady";
#endif

            if (_imageReadyEvent != null)
                _imageReadyEvent.Invoke(image);
        }

        public void AddImageReadyEvent(UnityAction<Texture2D> action)
        {
            _imageReadyEvent.AddListener(action);
        }

        public void RemoveImageReadyEvent(UnityAction<Texture2D> action)
        {
            _imageReadyEvent.RemoveListener(action);
        }

        public void OnPlayerPrepared(int videoWidth, int videoHeight)
        {
#if UNITY_EDITOR
            _lastEventMsg = "Prepared";
#endif

            _mediaPlayer.Mute = _mute;
            _mediaPlayer.Volume = _volume;
            _mediaPlayer.PlaybackRate = _playRate;

            if (_preparedEvent != null)
                _preparedEvent.Invoke(videoWidth, videoHeight);
        }

        public void AddPreparedEvent(UnityAction<int, int> action)
        {
            _preparedEvent.AddListener(action);
        }

        public void RemovePreparedEvent(UnityAction<int, int> action)
        {
            _preparedEvent.RemoveListener(action);
        }

        public void OnPlayerPlaying()
        {
#if UNITY_EDITOR
            _lastEventMsg = "Playing";
#endif
            if (_playingEvent != null)
                _playingEvent.Invoke();
        }

        public void AddPlayingEvent(UnityAction action)
        {
            _playingEvent.AddListener(action);
        }

        public void RemovePlayingEvent(UnityAction action)
        {
            _playingEvent.RemoveListener(action);
        }

        public void OnPlayerPaused()
        {
#if UNITY_EDITOR
            _lastEventMsg = "Paused";
#endif
            if (_pausedEvent != null)
                _pausedEvent.Invoke();
        }

        public void AddPausedEvent(UnityAction action)
        {
            _pausedEvent.AddListener(action);
        }

        public void RemovePausedEvent(UnityAction action)
        {
            _pausedEvent.RemoveListener(action);
        }

        public void OnPlayerStopped()
        {
#if UNITY_EDITOR
            if (!_lastEventMsg.Contains("Error"))
                _lastEventMsg = "Stopped";
#endif
            if (_stoppedEvent != null)
                _stoppedEvent.Invoke();
        }

        public void AddStoppedEvent(UnityAction action)
        {
            _stoppedEvent.AddListener(action);
        }

        public void RemoveStoppedEvent(UnityAction action)
        {
            _stoppedEvent.RemoveListener(action);
        }

        public void OnPlayerEndReached()
        {
#if UNITY_EDITOR
            _lastEventMsg = "End";
#endif

            _position = 0;
            _mediaPlayer.Stop(!_loop);

            if (_loop)
            {
                if (_mediaPlayerLoop != null)
                {
                    _mediaPlayerLoop.EventManager.CopyPlayerEvents(_mediaPlayer.EventManager);
                    _mediaPlayerLoop.VideoOutputObjects = _mediaPlayer.VideoOutputObjects;
                    _mediaPlayer.VideoOutputObjects = null;
                    _mediaPlayer.EventManager.RemoveAllEvents();

                    var tempPlayer = _mediaPlayer;
                    _mediaPlayer = _mediaPlayerLoop;
                    _mediaPlayerLoop = tempPlayer;
                }

                if (!string.IsNullOrEmpty(_path))
                    Play();
            }

            if (_endReachedEvent != null)
                _endReachedEvent.Invoke();
        }

        public void AddEndReachedEvent(UnityAction action)
        {
            _endReachedEvent.AddListener(action);
        }

        public void RemoveEndReachedEvent(UnityAction action)
        {
            _endReachedEvent.RemoveListener(action);
        }

        public void OnPlayerEncounteredError()
        {
#if UNITY_EDITOR
            _lastEventMsg = "Error (" + (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).GetLastError() + ")";
#endif
            Stop();

            if (_encounteredErrorEvent != null)
                _encounteredErrorEvent.Invoke();
        }

        public void AddEncounteredErrorEvent(UnityAction action)
        {
            _encounteredErrorEvent.AddListener(action);
        }

        public void RemoveEncounteredErrorEvent(UnityAction action)
        {
            _encounteredErrorEvent.RemoveListener(action);
        }

        public void OnPlayerTimeChanged(long time)
        {
#if UNITY_EDITOR
            _lastEventMsg = "TimeChanged";
#endif

            if (_timeChangedEvent != null)
                _timeChangedEvent.Invoke(time);
        }

        public void AddTimeChangedEvent(UnityAction<long> action)
        {
            _timeChangedEvent.AddListener(action);
        }

        public void RemoveTimeChangedEvent(UnityAction<long> action)
        {
            _timeChangedEvent.RemoveListener(action);
        }

        public void OnPlayerPositionChanged(float position)
        {
#if UNITY_EDITOR
            _lastEventMsg = "PositionChanged";
#endif
            _position = _mediaPlayer.Position;

            if (_positionChangedEvent != null)
                _positionChangedEvent.Invoke(position);
        }

        public void AddPositionChangedEvent(UnityAction<float> action)
        {
            _positionChangedEvent.AddListener(action);
        }

        public void RemovePositionChangedEvent(UnityAction<float> action)
        {
            _positionChangedEvent.RemoveListener(action);
        }

        public void OnPlayerSnapshotTaken(string path)
        {
            if (_snapshotTakenEvent != null)
                _snapshotTakenEvent.Invoke(path);
        }

        public void AddSnapshotTakenEvent(UnityAction<string> action)
        {
            _snapshotTakenEvent.AddListener(action);
        }

        public void RemoveSnapshotTakenEvent(UnityAction<string> action)
        {
            _snapshotTakenEvent.RemoveListener(action);
        }
    }
}