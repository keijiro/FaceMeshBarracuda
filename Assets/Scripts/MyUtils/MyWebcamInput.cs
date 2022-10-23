using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using NatSuite.Devices;

namespace MediaPipe.FaceMesh
{

    public sealed class MyWebcamInput : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] RenderTexture _targetRenderTexture = null;

        [SerializeField] Texture2D _dummyImage = null;

        [SerializeField] bool _isMirror = true;

        [SerializeField] JsonSettings _jsonSettings;

        #endregion

        #region Internal objects

        #endregion

        #region Public properties

        public Texture Texture
      => _dummyImage != null ? (Texture)_dummyImage : (Texture)_targetRenderTexture;

        #endregion

        MediaDeviceQuery query;

        Texture2D previewTexture;

        CameraDevice device;

        IMediaDevice defaultDevice;

        #region MonoBehaviour implementation

        async void Start()
        {
            if (_dummyImage != null) return;

            // Request camera permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>())
            {
                Debug.LogError("User did not grant camera permissions");
                return;
            }

            //load from Json
            _isMirror = _jsonSettings._settings.isMirrored;

            defaultDevice = _jsonSettings._settings.device;

            // Create a device query for device cameras
            query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);

            //search for default device.
            if(defaultDevice != null)
            {
                for(int i=0; i<query.count; i++)
                {
                    if(defaultDevice == query.current)
                    {
                        break;
                    }
                    query.Advance();
                }
            }


            device = query.current as CameraDevice;
            
            // Start camera preview           
            previewTexture = await device.StartRunning();
            Debug.Log($"Started camera preview with resolution {previewTexture.width}x{previewTexture.height}");
            
        }

  
        void Update()
        {
            if (_dummyImage != null) return;
            if (previewTexture == null) return;

            //1:1にクロップする
            var aspect1 = (float)previewTexture.width / previewTexture.height;
            var aspect2 = (float)_targetRenderTexture.width / _targetRenderTexture.height;
            var gap = aspect2 / aspect1;

            if (_isMirror) gap = -gap;

            var scale = new Vector2(gap, 1);
            var offset = new Vector2((1 - gap) / 2, 0);

            

            Graphics.Blit(previewTexture, _targetRenderTexture, scale, offset);

        }

        //カメラ切り替え
        public async void ChangeDevice()
        {
            // Check that there is another camera to switch to
            if (query.count < 2)
                return;
            // Stop current camera
            device.StopRunning();
            // Advance to next available camera
            query.Advance();
            // Start new camera
            device = query.current as CameraDevice;
            previewTexture = await device.StartRunning();

            //Update Json
            _jsonSettings.UpdateSettings<IMediaDevice>("device", query.current);

        }

        public void SetMirrored(bool isMirror)
        {
            _isMirror = isMirror;
            _jsonSettings.UpdateSettings<bool>("isMirrored", _isMirror);
        }

#endregion
    }

} // namespace MediaPipe.FaceMesh
