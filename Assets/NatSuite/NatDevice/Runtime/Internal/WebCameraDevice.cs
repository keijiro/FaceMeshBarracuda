/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class WebCameraDevice : CameraDevice {

        #region --Properties--

        public override string uniqueID => device.name;

        public override Devices.DeviceType type => Devices.DeviceType.Unknown;

        public override bool frontFacing => device.isFrontFacing;

        public override bool flashSupported => default;

        public override bool torchSupported => default;

        public override bool exposurePointSupported => default;

        public override bool focusPointSupported => default;
        
        public override bool exposureLockSupported => default;

        public override bool focusLockSupported => default;

        public override bool whiteBalanceLockSupported => default;

        public override (float width, float height) fieldOfView => default;

        public override (float min, float max) exposureRange => default;

        public override (float min, float max) zoomRange => (1f, 1f);

        public override (int width, int height) previewResolution { get; set; }

        public override (int width, int height) photoResolution {
            get => previewResolution;
            set { }
        }

        public override int frameRate { get; set; }

        public override float exposureBias {
            get => 0f;
            set { }
        }

        public override bool exposureLock {
            get => false;
            set { }
        }

        public override (float x, float y) exposurePoint {
            set { }
        }

        public override FlashMode flashMode {
            get => FlashMode.Off;
            set { }
        }

        public override bool focusLock {
            get => false;
            set { }
        }

        public override (float x, float y) focusPoint {
            set { }
        }

        public override bool torchEnabled {
            get => false;
            set { }
        }

        public override bool whiteBalanceLock {
            get => false;
            set { }
        }

        public override float zoomRatio {
            get => 1f;
            set { }
        }

        public override ScreenOrientation orientation {
            set { }
        }

        public override bool running => webCamTexture;

        public override unsafe Task<Texture2D> StartRunning () {
            var startTask = new TaskCompletionSource<Texture2D>();
            StartRunning(delegate (byte* nativeBuffer, int width, int height, long timestamp) {
                var firstFrame = !previewTexture;
                previewTexture = previewTexture ?? new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false, false);
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
            webCamTexture = new WebCamTexture(device.name, previewResolution.width, previewResolution.height, frameRate);
            webCamTexture.Play();
            Color32[] pixelBuffer = default;
            attachment = new GameObject("NatDevice WebCameraDevice Helper").AddComponent<WebCameraDeviceAttachment>();
            attachment.handler = delegate () {
                if (webCamTexture.width == 16 || webCamTexture.height == 16)
                    return;
                if (!webCamTexture.didUpdateThisFrame)
                    return;
                pixelBuffer = pixelBuffer ?? webCamTexture.GetPixels32();
                webCamTexture.GetPixels32(pixelBuffer);
                var timestamp = DateTime.Now.Ticks * 100L;
                fixed (void* baseAddress = pixelBuffer)
                    handler((byte*)baseAddress, webCamTexture.width, webCamTexture.height, timestamp);
            };
        }

        public override void StopRunning () {
            attachment.handler = default;
            WebCameraDeviceAttachment.Destroy(attachment.gameObject);
            webCamTexture.Stop();
            WebCamTexture.Destroy(webCamTexture);
            Texture2D.Destroy(previewTexture);
            webCamTexture = default;
            previewTexture = default;
            attachment = default;
        }

        public override unsafe Task<Texture2D> CapturePhoto () {
            var photoTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
            photoTexture.SetPixels32(webCamTexture.GetPixels32());
            photoTexture.Apply();
            return Task.FromResult(photoTexture);
        }
        #endregion


        #region --Operations--

        private readonly WebCamDevice device;
        private WebCamTexture webCamTexture;
        private Texture2D previewTexture;
        private WebCameraDeviceAttachment attachment;

        internal WebCameraDevice (WebCamDevice device) {
            this.device = device;
            this.previewResolution = (1280, 720);
            this.frameRate = 30;
        }

        public override string ToString () => $"webcam:{uniqueID}";

        private class WebCameraDeviceAttachment : MonoBehaviour {
            public Action handler;
            void Awake () => DontDestroyOnLoad(this.gameObject);
            void Update () => handler?.Invoke();
        }
        #endregion
    }
}