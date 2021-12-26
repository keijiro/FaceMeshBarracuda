//
//  NatDeviceTypes.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 1/14/2021.
//  Copyright © 2021 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include "stdint.h"
#include "stdbool.h"

// Platform defines
#ifdef __cplusplus
    #define BRIDGE extern "C"
#else
    #define BRIDGE
#endif

#ifdef _WIN64
    #define EXPORT __declspec(dllexport)
#else
    #define EXPORT
    #define APIENTRY
#endif


#pragma region --Types--
/*!
 @struct NDDevice
 
 @abstract Media device.

 @discussion Media device.
*/
struct NDDevice;
typedef struct NDDevice NDDevice;
#pragma endregion


#pragma region --Enumerations--
/*!
@enum NDFlashMode

@abstract Camera device photo flash modes.

@constant ND_FLASH_MODE_OFF
The flash will never be fired.

@constant ND_FLASH_MODE_ON
The flash will always be fired.

@constant ND_FLASH_MODE_AUTO
The sensor will determine whether to fire the flash.
*/
enum NDFlashMode {
    ND_FLASH_MODE_OFF = 0,
    ND_FLASH_MODE_ON = 1,
    ND_FLASH_MODE_AUTO = 2
};
typedef enum NDFlashMode NDFlashMode;

/*!
 @enum NDFrameOrientation
 
 @abstract Camera device frame orientation.
 
 @constant ND_FRAME_ORIENTATION_LANDSCAPE_LEFT
 Landscape left.
 
 @constant ND_FRAME_ORIENTATION_PORTRAIT
 Portrait.
 
 @constant ND_FRAME_ORIENTATION_LANDSCAPE_RIGHT
 Landscape right.
 
 @constant ND_FRAME_ORIENTATION_PORTRAIT_UPSIDE_DOWN
 Portrait upside down.
*/
enum NDFrameOrientation {
    ND_FRAME_ORIENTATION_LANDSCAPE_LEFT = 3,
    ND_FRAME_ORIENTATION_PORTRAIT = 1,
    ND_FRAME_ORIENTATION_LANDSCAPE_RIGHT = 4,
    ND_FRAME_ORIENTATION_PORTRAIT_UPSIDE_DOWN = 2
};
typedef enum NDFrameOrientation NDFrameOrientation;

/*!
 @enum NDFlags
 
 @abstract Immutable properties of media devices.
 
 @constant ND_FLAG_INTERNAL
 Device is internal.

 @constant ND_FLAG_EXTERNAL
 Device is external.

 @constant ND_FLAG_ECHO_CANCELLATION_SUPPORTED
 Audio device supports echo cancellation.

 @constant ND_FLAG_FRONT_FACING
 Camera device is front-facing.

 @constant ND_FLAG_FLASH_SUPPORTED
 Camera device supports flash when capturing photos.

 @constant ND_FLAG_TORCH_SUPPORTED
 Camera device supports torch.

 @constant ND_FLAG_EXPOSURE_POINT_SUPPORTED
 Camera device supports setting exposure point.

 @constant ND_FLAG_FOCUS_POINT_SUPPORTED
 Camera device supports setting focus point.

 @constant ND_FLAG_EXPOSURE_LOCK_SUPPORTED
 Camera device supports locking exposure.

 @constant ND_FLAG_FOCUS_LOCK_SUPPORTED
 Camera device supports locking focus.

 @constant ND_FLAG_WHITE_BALANCE_LOCK_SUPPORTED
 Camera device supports locking white balance.
*/
enum NDFlags {
    // Type
    ND_FLAG_INTERNAL = 1 << 0,
    ND_FLAG_EXTERNAL = 1 << 1,
    // Microphone flags
    ND_FLAG_ECHO_CANCELLATION_SUPPORTED = 1 << 2,
    // Camera flags
    ND_FLAG_FRONT_FACING = 1 << 6,
    ND_FLAG_FLASH_SUPPORTED = 1 << 7,
    ND_FLAG_TORCH_SUPPORTED = 1 << 8,
    ND_FLAG_EXPOSURE_POINT_SUPPORTED = 1 << 9,
    ND_FLAG_FOCUS_POINT_SUPPORTED = 1 << 10,
    ND_FLAG_EXPOSURE_LOCK_SUPPORTED = 1 << 11,
    ND_FLAG_FOCUS_LOCK_SUPPORTED = 1 << 12,
    ND_FLAG_WHITE_BALANCE_LOCK_SUPPORTED = 1 << 13
};
typedef enum NDFlags NDFlags;
#pragma endregion


#pragma region --Delegates--
/*!
 @abstract Callback invoked with new pixel buffer from a camera device.
 
 @param context
 User-provided context.
 
 @param pixelBuffer
 Pixel buffer in RGBA8888 format. This pixel buffer will never have padding bytes.
 
 @param width
 Pixel buffer width.
 
 @param height
 Pixel buffer height.
 
 @param timestamp
 Pixel buffer timestamp in nanoseconds.
 */
typedef void (*NDPixelBufferHandler) (void* context, uint8_t* pixelBuffer, int32_t width, int32_t height, int64_t timestamp);

/*!
 @abstract Callback invoked with new sample buffer from an audio device.
 
 @param context
 User-provided context.
 
 @param sampleBuffer
 Linear PCM sample buffer, interleaved by channel.
 
 @param sampleCount
 Number of samples in sample buffer.
 
 @param timestamp
 Sample buffer timestamp in nanoseconds.
 */
typedef void (*NDSampleBufferHandler) (void* context, float* sampleBuffer, int32_t sampleCount, int64_t timestamp);
#pragma endregion
