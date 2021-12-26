using UnityEngine;
using UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class MyWebcamInput : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] RenderTexture _targetRenderTexture = null;

        [SerializeField] Texture2D _dummyImage = null;
        [SerializeField] uint defaultDeviceIndex;

        #endregion

        #region Internal objects

        WebCamTexture _webcam;
        Vector2Int _resolution = new Vector2Int(1920, 1080);
        WebCamDevice[] devices;
        uint currentDevice;


        #endregion

        #region Public properties

        public Texture Texture
      => _dummyImage != null ? (Texture)_dummyImage : (Texture)_targetRenderTexture;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            if (_dummyImage != null) return;

            devices = WebCamTexture.devices;

            currentDevice = defaultDeviceIndex;

            _resolution.x = _targetRenderTexture.width;
            _resolution.y = _targetRenderTexture.height;

            _webcam = new WebCamTexture(devices[currentDevice].name, _resolution.x, _resolution.y);

            _webcam.Play();
        }

        void OnDestroy()
        {
            if (_webcam != null)
            {
                _webcam.Stop();
                Destroy(_webcam);
            }
        }

        void Update()
        {
            if (_dummyImage != null) return;
            if (!_webcam.didUpdateThisFrame) return;

            var aspect1 = (float)_webcam.width / _webcam.height;
            var aspect2 = (float)_resolution.x / _resolution.y;
            var gap = aspect2 / aspect1;

            var vflip = _webcam.videoVerticallyMirrored;
            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

            Graphics.Blit(_webcam, _targetRenderTexture, scale, offset);

        }

        //カメラ切り替え
        public void ChangeDevice()
        {
            //次のデバイスを選択。台数を超えたら最初に戻る
            currentDevice++;
            if (currentDevice >= devices.Length)
            {
                currentDevice = 0;
            }

            _webcam.Stop();
            _webcam = new WebCamTexture(devices[currentDevice].name, _resolution.x, _resolution.y);
            _webcam.Play();

            Debug.Log("ChangeCameraDevice" + currentDevice);
        }

        #endregion
    }

} // namespace MediaPipe.FaceMesh
