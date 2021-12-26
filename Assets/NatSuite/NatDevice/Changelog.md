## 1.1.0
+ Greatly reduced camera preview memory consumption on Android.
+ Greatly reduced camera preview latency on Android.
+ Greatly reduced microphone streaming latency on iOS.
+ Added `IMediaDevice.type` enumeration for checking if device is internal or external.
+ Added `AudioDevice.StartRunning` overload with callback that is given the native sample buffer handle with no memory copy.
+ Added `AudioDevice.echoCancellationSupported` boolean for checking if audio device supports echo cancellation.
+ Added `CameraDevice.StartRunning` overload with callback that is given the raw pixel buffer in managed memory.
+ Added `CameraDevice.StartRunning` overload with callback that is given the native pixel buffer handle with no memory copy.
+ Added `CameraDevice.exposurePointSupported` for checking if camera supports setting exposure point.
+ Added `CameraDevice.focusPointSupported` for checking if camera supports setting focus point.
+ Added `MediaDeviceQuery.count` property.
+ Added `MediaDeviceCriteria.Internal` criterion for finding internal media devices.
+ Added `MediaDeviceCriteria.External` criterion for finding external media devices.
+ Added `MediaDeviceCriteria.EchoCancellation` criterion for finding microphones that perform echo cancellation.
+ Added `MediaDeviceCriteria.Any` function for creating a criterion that requires any of the provided sub-criteria.
+ Added `MediaDeviceCriteria.All` function for creating a criterion that requires all of the provided sub-criteria.
+ Added support for Apple Silicon on macOS.
+ Changed `CameraDevice` behaviour to no longer pause and resume when app is suspended. **You must handle this yourself**.
+ Fixed flash not firing on some Android devices.
+ Fixed preview resolution setting being ignored on iOS.
+ Fixed hardware microphone format being ignored on macOS.
+ Fixed memory leak in photo capture on iOS.
+ Fixed memory leak when `CameraDevice` is stopped on macOS and Windows.
+ Fixed crash when focus point is set on Sony Xperia devices.
+ Fixed non-ASCII characters in `AudioDevice` name not being captured on Windows.
+ Fixed media device query permission task never completing on Android.
+ Fixed media device query permission task never completing on Windows.
+ Fixed permissions dialog not being presented on UWP.
+ Refactored `MediaDeviceQuery.Criteria` to `MediaDeviceCriteria`.
+ Refactored `MediaDeviceCriteria.RearFacing` to `MediaDeviceCriteria.RearCamera`.
+ Refactored `MediaDeviceCriteria.FrontFacing` to `MediaDeviceCriteria.FrontCamera`.
+ Refactored `MediaDeviceQuery.currentDevice` to `MediaDeviceQuery.current`.
+ Deprecated `MediaDeviceQuery.devices` property. The query now acts as a collection itself.
+ Removed `IAudioDevice` interface. All microphones are now exposed as `AudioDevice` instances.
+ Removed `ICameraDevice` interface. All cameras are now exposed as `CameraDevice` instances.
+ Removed `WebCameraDevice` class.
+ Removed `MixerDevice` class.
+ Removed `MediaDeviceCriteria.GenericCameraDevice`.
+ Removed `FrameOrientation` enumeration. Use Unity's `ScreenOrientation` enumeration instead.
+ NatDevice now requires Android API level 24+.
+ NatDevice now requires iOS 13+.
+ NatDevice now requires macOS 10.15+.

## 1.0.2
+ Moved documentation [online](https://docs.natsuite.io/natdevice/).
+ Added native permissions requests on iOS and macOS.
+ Echo cancellation can now be enabled and disabled on `AudioDevice` instances that support it.
+ Changed `MediaDeviceQuery` to only accept a single criterion, instead of multiple.
+ Fixed hard crash on iPhone 6 when `MediaDeviceQuery` is created.
+ Fixed `AudioDevice` causing NatCorder to crash when recording is stopped on iOS.
+ Fixed `AudioDevice` reporting incorrect format before the device starts running on iOS.
+ Deprecated `MediaDeviceQuery.count` property. Use `MediaDeviceQuery.devices.Length` instead.
+ Deprecated `MediaDeviceQuery.Criterion` delegate type. Use `System.Predicate` delegate from .NET BCL instead.
+ Deprecated `MediaDeviceQuery.Criteria.EchoCancellation` criterion as it is no longer useful.

## 1.0.1
+ Updated top-level namespace to `NatSuite.Devices` for parity with other NatSuite API's.
+ Fixed camera device query crash on Galaxy S10 and S10+.
+ Fixed sporadic crashes on some Android devices when the camera preview is started.
+ Fixed crash due to JNI local reference table overflow on Android.
+ Fixed `MediaDeviceQuery.Criteria.FrontFacing` not finding any cameras on iOS.
+ Fixed iOS archive generating error due to NatDevice not being built with full bitcode generation.

## 1.0.0
+ First release.