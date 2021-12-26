/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using System;

    /// <summary>
    /// Media device which provides media buffers.
    /// </summary>
    public interface IMediaDevice : IEquatable<IMediaDevice> {

        /// <summary>
        /// Device unique ID.
        /// </summary>
        string uniqueID { get; }

        /// <summary>
        /// Device type.
        /// </summary>
        DeviceType type { get; }

        /// <summary>
        /// Is the device running?
        /// </summary>
        bool running { get; }

        /// <summary>
        /// Stop running.
        /// </summary>
        void StopRunning ();
    }
}