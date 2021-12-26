/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    #region --Enumerations--
    /// <summary>
    /// Device type.
    /// </summary>
    public enum DeviceType : int { // CHECK // Must match `NatDeviceTypes.h`
        /// <summary>
        /// Device type is unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Device is internal.
        /// </summary>
        Internal = 1 << 0,
        /// <summary>
        /// Device is external.
        /// </summary>
        External = 1 << 1
    }

    /// <summary>
    /// Photo flash mode.
    /// </summary>
    public enum FlashMode : int {
        /// <summary>
        /// Never use flash.
        /// </summary>
        Off = 0,
        /// <summary>
        /// Always use flash.
        /// </summary>
        On = 1,
        /// <summary>
        /// Let the sensor detect if it needs flash.
        /// </summary>
        Auto = 2
    }
    #endregion


    #region --Delegates--
    /// <summary>
    /// Delegate invoked when camera device reports a new pixel buffer.
    /// </summary>
    /// <param name="pixelBuffer">Pixel buffer in RGBA8888 format. This pixel buffer will never have padding bytes.</param>
    /// <param name="width">Pixel buffer width.</param>
    /// <param name="height">Pixel buffer width.</param>
    /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
    public delegate void PixelBufferDelegate (byte[] pixelBuffer, int width, int height, long timestamp);

    /// <summary>
    /// Delegate invoked when camera device reports a new pixel buffer.
    /// </summary>
    /// <param name="nativeBuffer">Native pixel buffer in RGBA8888 format. This pixel buffer will never have padding bytes.</param>
    /// <param name="width">Pixel buffer width.</param>
    /// <param name="height">Pixel buffer width.</param>
    /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
    public unsafe delegate void NativePixelBufferDelegate (byte* nativeBuffer, int width, int height, long timestamp);

    /// <summary>
    /// Delegate invoked when audio device reports a new sample buffer.
    /// </summary>
    /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
    /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
    public delegate void SampleBufferDelegate (float[] sampleBuffer, long timestamp);

    /// <summary>
    /// Delegate invoked when audio device reports a new sample buffer.
    /// </summary>
    /// <param name="sampleBuffer">Native linear PCM sample buffer interleaved by channel.</param>
    /// <param name="sampleCount">Total number of samples in the buffer.</param>
    /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
    public unsafe delegate void NativeSampleBufferDelegate (float* sampleBuffer, int sampleCount, long timestamp);
    #endregion
}