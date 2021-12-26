//
//  NatDeviceBase.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 3/02/2021.
//  Copyright © 2021 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include "NatDeviceTypes.h"

#pragma region --IMediaDevice--
/*!
 @function NDReleaseDevice

 @abstract Dispose a device and release resources.

 @discussion Dispose a device and release resources.

 @param device
 Opaque handle to a media device.
*/
BRIDGE EXPORT void APIENTRY NDReleaseDevice (NDDevice* device);

/*!
 @function NDDeviceUniqueID

 @abstract Get the media device unique ID.

 @discussion Get the media device unique ID.

 @param device
 Opaque handle to a media device.

 @param dstString
 Destination UTF-8 string.
*/
BRIDGE EXPORT void APIENTRY NDDeviceUniqueID (NDDevice* device, char* dstString);

/*!
 @function NDDeviceFlags
 
 @abstract Get the media device flags.
 
 @discussion Get the media device flags.
 
 @param device
 Opaque handle to a media device.
 
 @returns Device flags.
*/
BRIDGE EXPORT NDFlags APIENTRY NDDeviceFlags (NDDevice* device);

/*!
 @function NDDeviceRunning
 
 @abstract Is the device running?
 
 @discussion Is the device running?
 
 @param device
 Opaque handle to a media device.
 
 @returns True if device is running.
*/
BRIDGE EXPORT bool APIENTRY NDDeviceRunning (NDDevice* device);

/*!
 @function NDDeviceStopRunning
 
 @abstract Stop running device.
 
 @discussion Stop running device.
 
 @param device
 Opaque handle to a media device.
 */
BRIDGE EXPORT void APIENTRY NDDeviceStopRunning (NDDevice* device);
#pragma endregion
