/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using AOT;

    public sealed class NativeAudioDevice : AudioDevice {

        #region --Properties--

        public override string uniqueID {
            get {
                var result = new StringBuilder(1024);
                device.UniqueID(result);
                return result.ToString();
            }
        }

        public override Devices.DeviceType type => (Devices.DeviceType)((int)device.Flags() & 0x3);

        public override string name {
            get {
                var result = new StringBuilder(1024);
                device.Name(result);
                return result.ToString();
            }
        }

        public override bool echoCancellationSupported => device.Flags().HasFlag(DeviceFlags.EchoCancellationSupported);
        
        public override bool echoCancellation {
            get => device.EchoCancellation();
            set => device.EchoCancellation(value);
        }

        public override int sampleRate {
            get => device.SampleRate();
            set => device.SampleRate(value);
        }

        public override int channelCount {
            get => device.ChannelCount();
            set => device.ChannelCount(value);
        }
        #endregion


        #region --Recording--

        public override bool running => device.Running();

        public override unsafe void StartRunning (SampleBufferDelegate handler) => StartRunning((nativeBuffer, sampleCount, timestamp) => {
            var sampleBuffer = new float[sampleCount]; // Allocating on every frame is inefficient
            Marshal.Copy((IntPtr)nativeBuffer, sampleBuffer, 0, sampleCount);
            try { // Surround with try-catch because uncaught exception will cause hard crash
                handler(sampleBuffer, timestamp);
            } catch (Exception ex) {
                Debug.LogError($"NatDevice Error: Sample buffer delegate raised exception");
                Debug.LogException(ex);
            }
        });

        public override unsafe void StartRunning (NativeSampleBufferDelegate handler) {
            handle = GCHandle.Alloc(handler, GCHandleType.Normal);
            device.StartRunning(OnSampleBuffer, (void*)(IntPtr)handle);
        }

        public override void StopRunning () {
            device.StopRunning();
            handle.Free();
            handle = default;
        }
        #endregion


        #region --Operations--

        private readonly IntPtr device;
        private GCHandle handle;

        internal NativeAudioDevice (IntPtr device) => this.device = device;

        ~NativeAudioDevice () => device.Release();

        [MonoPInvokeCallback(typeof(SampleBufferHandler))]
        private static unsafe void OnSampleBuffer (void* context, float* sampleBuffer, int sampleCount, long timestamp) {
            var handle = (GCHandle)(IntPtr)context;
            var handler = handle.Target as NativeSampleBufferDelegate;
            handler(sampleBuffer, sampleCount, timestamp);
        }
        #endregion
    }
}