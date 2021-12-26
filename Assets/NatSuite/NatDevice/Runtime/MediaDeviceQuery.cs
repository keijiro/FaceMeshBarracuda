/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;

    /// <summary>
    /// Query that can be used to access available media devices.
    /// </summary>
    public sealed partial class MediaDeviceQuery : IReadOnlyList<IMediaDevice> {

        #region --Client API--
        /// <summary>
        /// Number of devices discovered by the query.
        /// </summary>
        public int count => devices.Length;

        /// <summary>
        /// Get the device at a given index.
        /// </summary>
        public IMediaDevice this [int index] => devices[index];

        /// <summary>
        /// Current device that meets the provided criteria.
        /// </summary>
        public IMediaDevice current => index < devices.Length ? devices[index] : null;

        /// <summary>
        /// Create a media device query.
        /// </summary>
        /// <param name="criterion">Criterion that devices should meet.</param>
        public MediaDeviceQuery (Predicate<IMediaDevice> criterion = null) {
            // Get media devices
            var devices = new List<IMediaDevice>();
            switch (Application.platform) {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    devices.AddRange(AudioDevices());
                    devices.AddRange(CameraDevices());
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    devices.AddRange(AudioDevices());
                    devices.AddRange(WebCamDevices());
                    break;
                default:
                    devices.AddRange(WebCamDevices());
                    break;
            }
            // Filter by provided criterion
            this.devices = criterion != null ? devices.Where(d => criterion(d)).ToArray() : devices.ToArray();
        }

        /// <summary>
        /// Advance the next available device that meets the provided criteria.
        /// </summary>
        public void Advance () => index = (index + 1) % devices.Length;
        #endregion


        #region --Operations--

        private readonly IMediaDevice[] devices;
        private int index;

        private static IEnumerable<AudioDevice> AudioDevices () {
            var count = Bridge.AudioDeviceCount();
            var devices = new IntPtr[count];
            Bridge.AudioDevices(devices, devices.Length);
            foreach (var device in devices)
                yield return new NativeAudioDevice(device);
        }

        private static IEnumerable<CameraDevice> CameraDevices () {
            var count = Bridge.CameraDeviceCount();
            var devices = new IntPtr[count];
            Bridge.CameraDevices(devices, devices.Length);
            foreach (var device in devices)
                yield return new NativeCameraDevice(device);
        }

        private static IEnumerable<CameraDevice> WebCamDevices () {
            foreach (var device in WebCamTexture.devices)
                yield return new WebCameraDevice(device);
        }

        int IReadOnlyCollection<IMediaDevice>.Count => count;

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<IMediaDevice>).GetEnumerator();

        IEnumerator<IMediaDevice> IEnumerable<IMediaDevice>.GetEnumerator () {
            foreach (var device in devices)
                yield return device;
        }
        #endregion
    }
}