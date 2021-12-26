/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Examples {
    
    using UnityEngine;
    using Devices;
    using Components;

    public class HotMic : MonoBehaviour {
        
        AudioDevice device;
        ClipRecorder recorder;
        
        async void Start () {
            // Request mic permissions
            if (!await MediaDeviceQuery.RequestPermissions<AudioDevice>()) {
                Debug.LogError("User did not grant microphone permissions");
                return;
            }
            // Create a media device query for audio devices
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            // Get the device
            device = query.current as AudioDevice;
            Debug.Log($"{device}");
        }

        public void StartRecording () {
            // Create a recorder
            Debug.Log($"Starting recording with format: {device.channelCount} channel @ {device.sampleRate}Hz");
            recorder = new ClipRecorder(device.sampleRate, device.channelCount);
            // Start recording
            device.StartRunning(recorder.CommitSamples);
        }

        public void StopRecording () {
            // Stop recording
            device.StopRunning();
            var audioClip = recorder.FinishWriting();
            // Playback the recording
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }
    }
}