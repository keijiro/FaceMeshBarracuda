/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Examples.Components {

    using UnityEngine;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// AudioClip recorder.
    /// </summary>
    public sealed class ClipRecorder {

        #region --Client API--
        /// <summary>
        /// Create an AudioClip recorder.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        public ClipRecorder (int sampleRate, int channelCount) {
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.audioBuffer = new MemoryStream();
        }

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// </summary>
        /// <param name="sampleBuffer">Raw PCM audio sample buffer, interleaved by channel.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CommitSamples (float[] sampleBuffer, long timestamp = 0L) { // NatCorder proto
            var byteSamples = new byte[Buffer.ByteLength(sampleBuffer)];
            Buffer.BlockCopy(sampleBuffer, 0, byteSamples, 0, byteSamples.Length);
            audioBuffer.Write(byteSamples, 0, byteSamples.Length);
        }

        /// <summary>
        /// Finish writing and return the AudioClip.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public AudioClip FinishWriting () {
            // Get the full sample buffer
            var byteSamples = audioBuffer.ToArray();
            var totalSampleCount = byteSamples.Length / sizeof(float); 
            var sampleBuffer = new float[totalSampleCount];  
            var recordingName = string.Format("recording_{0}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            Buffer.BlockCopy(byteSamples, 0, sampleBuffer, 0, byteSamples.Length);
            audioBuffer.Dispose();
            // Create audio clip
            var audioClip = AudioClip.Create(recordingName, totalSampleCount / channelCount, channelCount, sampleRate, false);
            audioClip.SetData(sampleBuffer, 0);
            return audioClip;
        }
        #endregion


        #region --Operations--
        private readonly int sampleRate, channelCount;
        private readonly MemoryStream audioBuffer;
        #endregion
    }
}