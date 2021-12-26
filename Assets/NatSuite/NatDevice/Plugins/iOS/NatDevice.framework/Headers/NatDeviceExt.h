//
//  NatDeviceExt.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 8/14/2020.
//  Copyright © 2021 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include "NatDeviceTypes.h"

#pragma region --Permissions--
/*!
 @enum NDPermissionType
 
 @abstract Permission type to request.
 
 @constant ND_PERMISSION_TYPE_MICROPHONE
 Request microphone permissions.
 
 @constant ND_PERMISSION_TYPE_CAMERA
 Request camera permissions.
 */
enum NDPermissionType {
    ND_PERMISSION_TYPE_MICROPHONE = 1,
    ND_PERMISSION_TYPE_CAMERA = 2
};
typedef enum NDPermissionType NDPermissionType;

/*!
 @abstract Callback invoked with result of permission request.
 
 @param context
 User-provided context.
 
 @param result
 Whether user granted permissions.
 */
typedef void (*NDPermissionResultHandler) (void* context, bool result);

/*!
 @function NDRequestPermissions
 
 @abstract Request permissions for a given media type.
 
 @discussion Request permissions for a given media type.
 
 @param type
 Permission type.
 
 @param handler
 Permission delegate to receive result of permission request.

 @param context
 User-provided context to be passed to the permission delegate.
 */
BRIDGE EXPORT void APIENTRY NDRequestPermissions (NDPermissionType type, NDPermissionResultHandler handler, void* context);
#pragma endregion
