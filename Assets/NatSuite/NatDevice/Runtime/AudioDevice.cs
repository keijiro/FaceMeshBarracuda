/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    /// <summary>
    /// Abstraction for a hardware audio input device.
    /// </summary>
    public abstract class AudioDevice : IMediaDevice {
        
        #region --Properties--
        /// <summary>
        /// Device unique ID.
        /// </summary>
        public abstract string uniqueID { get; }

        /// <summary>
        /// Device type.
        /// </summary>
        public abstract DeviceType type { get; }

        /// <summary>
        /// Display friendly device name.
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// Is echo cancellation supported?
        /// </summary>
        public abstract bool echoCancellationSupported { get; }

        /// <summary>
        /// Enable or disable Adaptive Echo Cancellation (AEC).
        /// </summary>
        public abstract bool echoCancellation { get; set; }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public abstract int sampleRate { get; set; }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public abstract int channelCount { get; set; }
        #endregion


        #region --Operations--
        /// <summary>
        /// Is the device running?
        /// </summary>
        public abstract bool running { get; }

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive sample buffers.</param>
        public abstract void StartRunning (SampleBufferDelegate handler);

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive sample buffers.</param>
        public abstract void StartRunning (NativeSampleBufferDelegate handler);
        
        /// <summary>
        /// Stop running.
        /// </summary>
        public abstract void StopRunning ();
        #endregion


        #region --Operations--

        public bool Equals (IMediaDevice other) => other != null && other is AudioDevice && other.uniqueID == uniqueID;

        public override string ToString () => $"microphone:{uniqueID}";
        #endregion
    }
}