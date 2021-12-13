using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UMP
{
    public class ARVideoCanvasHelper : MonoBehaviour
    {
        // Name of properties that we will use in shader to set the left/right and top/bottom border size
        private const string SHADER_BORDER_U_WIDTH = "_BorderUWidth";
        private const string SHADER_BORDER_V_WIDTH = "_BorderVWidth";

        // Main camera property that we will use to find the size of gameobject that have "MeshRenderer " component
        [SerializeField]
        private Camera _mainCamera = null;
        // Media player property that we will use to get current video size
        [SerializeField]
        private UniversalMediaPlayer _mediaPlayer = null;
        // Default size of video canvas (used if native library can't return correct video size)
        [SerializeField]
        public int _defaultWidth = 640;
        [SerializeField]
        public int _defaultHeight = 640;

        private MeshRenderer _meshRenderer;
        private RawImage _rawImageRenderer;
        private Material _objectMaterial;
        private Vector2 _objectSize;
        private Vector2 _videoSize;
        private Vector2 _calcSize;
        private Vector2 _borderUVSize;

        private IEnumerator _updateCanvasRatioEnum;

        private void Start()
        {
            _rawImageRenderer = gameObject.GetComponent<RawImage>();
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (_rawImageRenderer == null && _meshRenderer == null)
            {
                Debug.LogError("Object need have MeshRenderer or RawImage component!");
                return;
            }
            _objectMaterial = _rawImageRenderer != null ? _rawImageRenderer.material : _meshRenderer.material;

            if (_mediaPlayer != null)
            {
                _mediaPlayer.AddImageReadyEvent(OnPlayerImageReady);
                _mediaPlayer.AddStoppedEvent(OnPlayerStopped);
            }

            ShowVideoCanvasBorder(true);
        }

        private void OnDestroy()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.RemoveImageReadyEvent(OnPlayerImageReady);
                _mediaPlayer.RemoveStoppedEvent(OnPlayerStopped);
            }

            if (_updateCanvasRatioEnum != null)
                StopCoroutine(_updateCanvasRatioEnum);

            ShowVideoCanvasBorder(false);
        }

        /// <summary>
        /// Calculate the size of video canvas borders and send results to custom shader
        /// </summary>
        private IEnumerator UpdateVideoCanvasRatio()
        {
            while (true)
            {
                _objectSize = GetPixelSizeOfMeshRenderer(_meshRenderer, _mainCamera);
                if (_objectSize == Vector2.zero)
                    _objectSize = GetPixelSizeOfRawImage(_rawImageRenderer);

                _calcSize = Vector2.zero;
                _calcSize.x = (_objectSize.y / _videoSize.y) * _videoSize.x;
                if (_calcSize.x < _objectSize.x)
                    _calcSize.y = _objectSize.y;
                else
                    _calcSize = new Vector2(_objectSize.x, (_objectSize.x / _videoSize.x) * _videoSize.y);

                _borderUVSize = new Vector2((1 - (_calcSize.x / _objectSize.x)) * 0.5f, (1 - (_calcSize.y / _objectSize.y)) * 0.5f);
                _objectMaterial.SetFloat(SHADER_BORDER_U_WIDTH, _borderUVSize.x);
                _objectMaterial.SetFloat(SHADER_BORDER_V_WIDTH, _borderUVSize.y);
                yield return null;
            }
        }

        /// <summary>
        /// Show/Hide special video canvas borders in output texture
        /// </summary>
        private void ShowVideoCanvasBorder(bool state)
        {
            _objectMaterial.SetFloat(SHADER_BORDER_U_WIDTH, state ? 1 : 0);
            _objectMaterial.SetFloat(SHADER_BORDER_V_WIDTH, state ? 1 : 0);
        }

        private void OnPlayerImageReady(Texture2D image)
        {
            _videoSize = new Vector2(image.width, image.height);

            if (_updateCanvasRatioEnum == null)
            {
                _updateCanvasRatioEnum = UpdateVideoCanvasRatio();
                StartCoroutine(_updateCanvasRatioEnum);
            }
        }

        private void OnPlayerStopped()
        {
            if (!_mediaPlayer.Loop)
            {
                if (_updateCanvasRatioEnum != null)
                {
                    StopCoroutine(_updateCanvasRatioEnum);
                    _updateCanvasRatioEnum = null;
                }

                ShowVideoCanvasBorder(true);
            }
        }

        /// <summary>
        /// Get size in pixels of gameobject that have "MeshRenderer" component
        /// </summary>
        /// <param name="meshRenderer">Gameobject "MeshRenderer" component</param>
        /// <param name="camera">Main camera of current scene</param>
        /// <returns></returns>
        public static Vector2 GetPixelSizeOfMeshRenderer(MeshRenderer meshRenderer, Camera camera)
        {
            if (meshRenderer == null)
                return Vector2.zero;

            Vector3 posStart = camera.WorldToScreenPoint(new Vector3(meshRenderer.bounds.min.x, meshRenderer.bounds.min.y, meshRenderer.bounds.min.z));
            Vector3 posEnd = camera.WorldToScreenPoint(new Vector3(meshRenderer.bounds.max.x, meshRenderer.bounds.max.y, meshRenderer.bounds.min.z));

            float widthX = Mathf.Abs((posEnd.x - posStart.x));
            float widthY = Mathf.Abs((posEnd.y - posStart.y));

            return new Vector2((int)widthX, (int)widthY);
        }

        /// <summary>
        /// Get size in pixels of gameobject that have "RawImage" component
        /// </summary>
        /// <param name="rawImage">Gameobject "RawImage" component</param>
        /// <returns></returns>
        public static Vector2 GetPixelSizeOfRawImage(RawImage rawImage)
        {
            if (rawImage == null)
                return Vector2.zero;

            return new Vector2(rawImage.rectTransform.rect.width, rawImage.rectTransform.rect.height);
        }
    }
}
