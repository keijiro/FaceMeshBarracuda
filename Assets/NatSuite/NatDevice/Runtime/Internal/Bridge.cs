/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Bridge {

        public const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatDevice";
        #endif


        #region --IMediaDevice--
        [DllImport(Assembly, EntryPoint = @"NDReleaseDevice")]
        public static extern void Release (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDDeviceUniqueID")]
        public static extern void UniqueID (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest
        );
        [DllImport(Assembly, EntryPoint = @"NDDeviceFlags")]
        public static extern DeviceFlags Flags (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDDeviceRunning")]
        public static extern bool Running (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDDeviceStopRunning")]
        public static extern void StopRunning (this IntPtr device);
        #endregion


        #region --AudioDevice--
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceCount")]
        public static extern int AudioDeviceCount ();
        [DllImport(Assembly, EntryPoint = @"NDAudioDevices")]
        public static extern void AudioDevices (
            [Out] IntPtr[] devices,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceName")]
        public static extern void Name (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest
        );
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceEchoCancellation")]
        public static extern bool EchoCancellation (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceSetEchoCancellation")]
        public static extern void EchoCancellation (this IntPtr device, bool echoCancellation);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceSampleRate")]
        public static extern int SampleRate (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceSetSampleRate")]
        public static extern void SampleRate (this IntPtr device, int sampleRate);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceChannelCount")]
        public static extern int ChannelCount (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceSetChannelCount")]
        public static extern void ChannelCount (this IntPtr device, int sampleRate);
        [DllImport(Assembly, EntryPoint = @"NDAudioDeviceStartRunning")]
        public static extern unsafe bool StartRunning (
            this IntPtr device,
            SampleBufferHandler callback,
            void* context
        );
        #endregion


        #region --CameraDevice--
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceCount")]
        public static extern int CameraDeviceCount ();
        [DllImport(Assembly, EntryPoint = @"NDCameraDevices")]
        public static extern void CameraDevices (
            [Out] IntPtr[] devices,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceFieldOfView")]
        public static extern void FieldOfView (this IntPtr device, out float x, out float y);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceExposureRange")]
        public static extern void ExposureRange (this IntPtr device, out float min, out float max);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceZoomRange")]
        public static extern void ZoomRange (this IntPtr device, out float min, out float max);
        [DllImport(Assembly, EntryPoint = @"NDCameraDevicePreviewResolution")]
        public static extern void PreviewResolution (this IntPtr device, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetPreviewResolution")]
        public static extern void PreviewResolution (this IntPtr device, int width, int height);
        [DllImport(Assembly, EntryPoint = @"NDCameraDevicePhotoResolution")]
        public static extern void PhotoResolution (this IntPtr device, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetPhotoResolution")]
        public static extern void PhotoResolution (this IntPtr device, int width, int height);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceFrameRate")]
        public static extern int FrameRate (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetFrameRate")]
        public static extern void FrameRate (this IntPtr device, int framerate);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceExposureBias")]
        public static extern float ExposureBias (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetExposureBias")]
        public static extern void ExposureBias (this IntPtr device, float bias);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetExposurePoint")]
        public static extern void ExposurePoint (this IntPtr device, float x, float y);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceExposureLock")]
        public static extern bool ExposureLock (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetExposureLock")]
        public static extern void ExposureLock (this IntPtr device, bool locked);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceFlashMode")]
        public static extern FlashMode FlashMode (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetFlashMode")]
        public static extern void FlashMode (this IntPtr device, FlashMode state);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceFocusLock")]
        public static extern bool FocusLock (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetFocusLock")]
        public static extern void FocusLock (this IntPtr device, bool locked);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetFocusPoint")]
        public static extern void FocusPoint (this IntPtr device, float x, float y);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceTorchEnabled")]
        public static extern bool TorchEnabled (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetTorchEnabled")]
        public static extern void TorchEnabled (this IntPtr device, bool enabled);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceWhiteBalanceLock")]
        public static extern bool WhiteBalanceLock (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetWhiteBalanceLock")]
        public static extern void WhiteBalanceLock (this IntPtr device, bool locked);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceZoomRatio")]
        public static extern float ZoomRatio (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetZoomRatio")]
        public static extern void ZoomRatio (this IntPtr device, float ratio);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceSetOrientation")]
        public static extern void Orientation (this IntPtr device, int orentation);
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceStartRunning")]
        public static extern unsafe void StartRunning (
            this IntPtr device,
            PixelBufferHandler handler,
            void* context
        );
        [DllImport(Assembly, EntryPoint = @"NDCameraDeviceCapturePhoto")]
        public static extern unsafe void CapturePhoto (
            this IntPtr device,
            PixelBufferHandler handler,
            void* context
        );
        #endregion
    }
}