using UnityEngine;

namespace UMP
{
    public class UMPAudioOutput : AudioOutput
    {
        /* 
         * Example how to get audio output data on desktop platforms (Win, Mac, Linux),
         * to use it uncomment code below
         */

        private void Awake()
        {
            //OutputDataListener += OnOutputData;
        }

        private void OnDestroy()
        {
            //OutputDataListener -= OnOutputData;
        }

        private void OnOutputData(float[] data, AudioChannels channels)
        {
            Debug.Log("Handle audio output data");
        }
    }
}
