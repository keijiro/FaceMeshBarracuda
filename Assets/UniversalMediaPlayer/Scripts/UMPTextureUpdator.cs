using UnityEngine;
using UnityEngine.UI;

namespace UMP
{
    public class UMPTextureUpdator : MonoBehaviour
    {
        [SerializeField]
        private RawImage _image = null;

        [SerializeField]
        private UniversalMediaPlayer _player = null;

        private Texture2D _texture;
        private long _framesCounter;

        void Start()
        {
            _player.AddImageReadyEvent(OnImageReady);
            _player.AddStoppedEvent(OnStop);
        }

        void Update()
        {
            if (_texture != null && _framesCounter != _player.FramesCounter)
            {
                _texture.LoadRawTextureData(_player.FramePixels);
                _texture.Apply();

                _framesCounter = _player.FramesCounter;
            }
        }

        void OnDestroy()
        {
            _player.RemoveStoppedEvent(OnStop);
        }

        void OnImageReady(Texture image)
        {
            if (_texture != null)
                Destroy(_texture);

            //Video size != Video buffer size (FramePixels has video buffer size), so we will use
            //previously created playback texture size that based on video buffer size
            _texture = MediaPlayerHelper.GenVideoTexture(image.width, image.height);
            _texture.Apply();

            _image.texture = _texture;
        }

        void OnStop()
        {
            if (_texture != null)
                Destroy(_texture);
            _texture = null;
        }
    }
}
