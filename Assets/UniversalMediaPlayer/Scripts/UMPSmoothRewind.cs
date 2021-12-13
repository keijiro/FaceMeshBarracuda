using UnityEngine;
using UnityEngine.UI;

namespace UMP
{
    public class UMPSmoothRewind : MonoBehaviour
    {
        [SerializeField]
        private UniversalMediaPlayer _mediaPlayer = null;

        [SerializeField]
        private Slider _rewindSlider = null;

        private long _framesCounterCahce;

        private void Update()
        {
            if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
            {
                if (_mediaPlayer.IsPlaying && _framesCounterCahce != _mediaPlayer.FramesCounter)
                {
                    _framesCounterCahce = _mediaPlayer.FramesCounter;
                    var frameAmount = (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).FramesAmount;

                    if (frameAmount > 0)
                        _rewindSlider.value = (float)_framesCounterCahce / frameAmount;
                }
            }
        }

        public void OnPositionChanged()
        {
            _mediaPlayer.Position = _rewindSlider.value;
        }
    }
}
