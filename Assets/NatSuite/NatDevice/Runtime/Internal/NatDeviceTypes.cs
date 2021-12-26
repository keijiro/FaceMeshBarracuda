/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices.Internal { // CHECK // Must match `NatDeviceTypes.h`

    using System;

    #region --Enumerations--
    [Flags]
    public enum DeviceFlags : int {
        // Type
        Internal = 1 << 0,
        External = 1 << 1,
        // Microphone
        EchoCancellationSupported = 1 << 2,
        // Camera
        FrontFacing = 1 << 6,
        FlashSupported = 1 << 7,
        TorchSupported = 1 << 8,
        ExposurePointSupported = 1 << 9,
        FocusPointSupported = 1 << 10,
        ExposureLockSupported = 1 << 11,
        FocusLockSupported = 1 << 12,
        WhiteBalanceLockSupported = 1 << 13
    }
    #endregion


    #region --Delegates--
    public unsafe delegate void PixelBufferHandler (void* context, byte* pixelBuffer, int width, int height, long timestamp);
    public unsafe delegate void SampleBufferHandler (void* context, float* sampleBuffer, int sampleCount, long timestamp);
    #endregion
}