/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using AOT;

    public sealed class NativeCameraDevice : CameraDevice {

        #region --Properties--
        
        public override string uniqueID {
            get {
                var result = new StringBuilder(1024);
                device.UniqueID(result);
                return result.ToString();
            }
        }

        public override Devices.DeviceType type => (Devices.DeviceType)((int)device.Flags() & 0x3);

        public override bool frontFacing => device.Flags().HasFlag(DeviceFlags.FrontFacing);

        public override bool flashSupported => device.Flags().HasFlag(DeviceFlags.FlashSupported);

        public override bool torchSupported => device.Flags().HasFlag(DeviceFlags.TorchSupported);

        public override bool exposurePointSupported => device.Flags().HasFlag(DeviceFlags.ExposurePointSupported);

        public override bool focusPointSupported => device.Flags().HasFlag(DeviceFlags.FocusPointSupported);

        public override bool exposureLockSupported => device.Flags().HasFlag(DeviceFlags.ExposureLockSupported);

        public override bool focusLockSupported => device.Flags().HasFlag(DeviceFlags.FocusLockSupported);

        public override bool whiteBalanceLockSupported => device.Flags().HasFlag(DeviceFlags.WhiteBalanceLockSupported);

        public override (float width, float height) fieldOfView {
            get {
                device.FieldOfView(out var width, out var height);
                return (width, height);
            }
        }

        public override (float min, float max) exposureRange {
            get {
                device.ExposureRange(out var min, out var max);
                return (min, max);
            }
        }

        public override (float min, float max) zoomRange {
            get {
                device.ZoomRange(out var min, out var max);
                return (min, max);
            }
        }

        public override (int width, int height) previewResolution {
            get { device.PreviewResolution(out var width, out var height); return (width, height); }
            set => device.PreviewResolution(value.width, value.height);
        }

        public override (int width, int height) photoResolution {
            get { device.PhotoResolution(out var width, out var height); return (width, height); }
            set => device.PhotoResolution(value.width, value.height);
        }
        
        public override int frameRate {
            get => device.FrameRate();
            set => device.FrameRate(value);
        }

        public override float exposureBias {
            get => device.ExposureBias();
            set => device.ExposureBias(value);
        }

        public override bool exposureLock {
            get => device.ExposureLock();
            set => device.ExposureLock(value);
        }

        public override (float x, float y) exposurePoint {
            set => device.ExposurePoint(value.x, value.y);
        }

        public override FlashMode flashMode {
            get => device.FlashMode();
            set => device.FlashMode(value);
        }

        public override bool focusLock {
            get => device.FocusLock();
            set => device.FocusLock(value);
        }

        public override (float x, float y) focusPoint {
            set => device.FocusPoint(value.x, value.y);
        }

        public override bool torchEnabled {
            get => device.TorchEnabled();
            set => device.TorchEnabled(value);
        }

        public override bool whiteBalanceLock {
            get => device.WhiteBalanceLock();
            set => device.WhiteBalanceLock(value);
        }

        public override float zoomRatio {
            get => device.ZoomRatio();
            set => device.ZoomRatio(value);
        }

        public override ScreenOrientation orientation {
            set => device.Orientation((int)value);
        }
        #endregion


        #region --Preview--

        public override bool running => device.Running();

        public override unsafe Task<Texture2D> StartRunning () {
            var startTask = new TaskCompletionSource<Texture2D>();
            StartRunning(delegate (byte* nativeBuffer, int width, int height, long timestamp) {
                var firstFrame = !previewTexture;
                previewTexture = previewTexture ?? new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                previewTexture.LoadRawTextureData((IntPtr)nativeBuffer, width * height * 4);
                previewTexture.Apply();
                if (firstFrame)
                    startTask.SetResult(previewTexture);
            });
            return startTask.Task;
        }
        
        public override unsafe void StartRunning (PixelBufferDelegate handler) {
            byte[] pixelBuffer = default;
            StartRunning(delegate (byte* nativeBuffer, int width, int height, long timestamp) {
                pixelBuffer = pixelBuffer ?? new byte[width * height * 4];
                Marshal.Copy((IntPtr)nativeBuffer, pixelBuffer, 0, pixelBuffer.Length);
                handler(pixelBuffer, width, height, timestamp);
            });
        }

        public override unsafe void StartRunning (NativePixelBufferDelegate handler) { 
            handle = GCHandle.Alloc(handler, GCHandleType.Normal);
            device.StartRunning(OnPixelBuffer, (void*)(IntPtr)handle);
        }

        public override void StopRunning () {
            device.StopRunning();
            handle.Free();
            Texture2D.Destroy(previewTexture);
            previewTexture = default;
            handle = default;
        }

        public override unsafe Task<Texture2D> CapturePhoto () {
            var captureTask = new TaskCompletionSource<Texture2D>();
            GCHandle handle = new();
            NativePixelBufferDelegate handler = (pixelBuffer, width, height, timestamp) => {
                handle.Free();
                var photoTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                photoTexture.LoadRawTextureData((IntPtr)pixelBuffer, width * height * 4);
                photoTexture.Apply();
                captureTask.SetResult(photoTexture);
            };
            handle = GCHandle.Alloc(handler, GCHandleType.Normal);
            device.CapturePhoto(OnPixelBuffer, (void*)(IntPtr)handle);
            return captureTask.Task;
        }
        #endregion


        #region --Operations--

        private readonly IntPtr device;
        private GCHandle handle;
        private Texture2D previewTexture;

        internal NativeCameraDevice (IntPtr device) {
            this.device = device;
            this.orientation = Screen.orientation;
        }

        ~NativeCameraDevice () => device.Release();

        [MonoPInvokeCallback(typeof(PixelBufferHandler))]
        private static unsafe void OnPixelBuffer (void* context, byte* pixelBuffer, int width, int height, long timestamp) {
            var handle = (GCHandle)(IntPtr)context;
            var handler = handle.Target as NativePixelBufferDelegate;
            handler(pixelBuffer, width, height, timestamp);
        }
        #endregion
    }
}