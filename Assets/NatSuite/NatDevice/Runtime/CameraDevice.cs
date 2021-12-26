/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using UnityEngine;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Abstraction for a hardware camera device.
    /// </summary>
    public abstract class CameraDevice : IMediaDevice {

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
        /// Is this camera front facing?
        /// </summary>
        public abstract bool frontFacing { get; }

        /// <summary>
        /// Is flash supported for photo capture?
        /// </summary>
        public abstract bool flashSupported { get; }

        /// <summary>
        /// Is torch supported?
        /// </summary>
        public abstract bool torchSupported { get; }
        
        /// <summary>
        /// Is setting the exposure point supported?
        /// </summary>
        public abstract bool exposurePointSupported { get; }

        /// <summary>
        /// Is setting the focus point supported?
        /// </summary>
        public abstract bool focusPointSupported { get; }

        /// <summary>
        /// Is exposure lock supported?
        /// </summary>
        public abstract bool exposureLockSupported { get; }

        /// <summary>
        /// Is focus lock supported?
        /// </summary>
        public abstract bool focusLockSupported { get; }

        /// <summary>
        /// Is white balance lock supported?
        /// </summary>
        public abstract bool whiteBalanceLockSupported { get; }

        /// <summary>
        /// Field of view in degrees.
        /// </summary>
        public abstract (float width, float height) fieldOfView { get; }

        /// <summary>
        /// Exposure bias range.
        /// </summary>
        public abstract (float min, float max) exposureRange { get; }

        /// <summary>
        /// Zoom ratio range.
        /// </summary>
        public abstract (float min, float max) zoomRange { get; }

        /// <summary>
        /// Get or set the preview resolution.
        /// </summary>
        public abstract (int width, int height) previewResolution { get; set; }

        /// <summary>
        /// Get or set the photo resolution.
        /// </summary>
        public abstract (int width, int height) photoResolution { get; set; }

        /// <summary>
        /// Get or set the preview framerate.
        /// </summary>
        public abstract int frameRate { get; set; }

        /// <summary>
        /// Get or set the exposure bias.
        /// This value must be in the range returned by `exposureRange`.
        /// </summary>
        public abstract float exposureBias { get; set; }

        /// <summary>
        /// Get or set the exposure lock.
        /// </summary>
        public abstract bool exposureLock { get; set; }

        /// <summary>
        /// Set the exposure point of interest.
        /// </summary>
        public abstract (float x, float y) exposurePoint { set; }

        /// <summary>
        /// Get or set the photo flash mode.
        /// </summary>
        public abstract FlashMode flashMode { get; set; }

        /// <summary>
        /// Get or set the focus lock.
        /// </summary>
        public abstract bool focusLock { get; set; }

        /// <summary>
        /// Set the focus point of interest.
        /// </summary>
        public abstract (float x, float y) focusPoint { set; }

        /// <summary>
        /// Get or set the torch mode.
        /// </summary>
        public abstract bool torchEnabled { get; set; }

        /// <summary>
        /// Get or set the white balance lock.
        /// </summary>
        public abstract bool whiteBalanceLock { get; set; }

        /// <summary>
        /// Get or set the zoom ratio.
        /// This value must be in the range returned by `zoomRange`.
        /// </summary>
        public abstract float zoomRatio { get; set; }

        /// <summary>
        /// Set the preview orientation.
        /// Defaults to the screen orientation.
        /// </summary>
        public abstract ScreenOrientation orientation { set; }
        #endregion


        #region --Operations--
        /// <summary>
        /// Is the device running?
        /// </summary>
        public abstract bool running { get; }

        /// <summary>
        /// Start running.
        /// </summary>
        /// <returns>Camera preview texture.</returns>
        public abstract Task<Texture2D> StartRunning ();

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive pixel buffers.</param>
        public abstract void StartRunning (PixelBufferDelegate handler);

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive pixel buffers.</param>
        public abstract void StartRunning (NativePixelBufferDelegate handler);

        /// <summary>
        /// Stop running.
        /// </summary>
        public abstract void StopRunning ();

        /// <summary>
        /// Capture a photo.
        /// </summary>
        /// <returns>High resolution photo texture.</returns>
        public abstract Task<Texture2D> CapturePhoto ();
        #endregion


        #region --Operations--

        public bool Equals (IMediaDevice other) => other != null && other is CameraDevice && other.uniqueID == uniqueID;

        public override string ToString () => $"camera:{uniqueID}";
        #endregion
    }
}