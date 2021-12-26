/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Devices {

    using AOT;
    using UnityEngine;
    using UnityEngine.Android;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Internal;

    public sealed partial class MediaDeviceQuery {
        
        #region --Client API--
        /// <summary>
        /// Request permissions to use media devices from the user.
        /// </summary>
        /// <returns>Whether the app has permission to use the requested media device type.</returns>
        public static Task<bool> RequestPermissions<T> () where T : IMediaDevice {
            // Get type
            var permissionType = (PermissionType)0;
            if (typeof(CameraDevice).IsAssignableFrom(typeof(T)))
                permissionType = PermissionType.Camera;
            else if (typeof(AudioDevice).IsAssignableFrom(typeof(T)))
                permissionType = PermissionType.Audio;
            // Check
            if (permissionType == 0)
                return Task.FromResult(true);
            // Request
            var permissionTask = new TaskCompletionSource<bool>();
            switch (Application.platform) {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    permissionTask.SetResult(true); // Windows does not use permissions
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    var handle = GCHandle.Alloc(permissionTask, GCHandleType.Normal);
                    RequestPermissions(permissionType, OnPermissionResult, (IntPtr)handle);
                    break;
                case RuntimePlatform.Android:
                    var androidHelper = new GameObject("MediaDeviceQuery Android Permissions Helper").AddComponent<MediaDeviceQueryPermissionsHelper>();
                    androidHelper.StartCoroutine(RequestAndroid(permissionType, permissionTask, androidHelper));
                    break;
                default:
                    var helper = new GameObject("MediaDeviceQuery Permissions Helper").AddComponent<MediaDeviceQueryPermissionsHelper>();
                    helper.StartCoroutine(RequestUnity(permissionType, permissionTask, helper));
                    break;
            }
            return permissionTask.Task;
        }
        #endregion


        #region --Operations--

        private enum PermissionType { Audio = 1, Camera = 2 } // CHECK // Must match definition in `NatDeviceExt.h`

        private static IEnumerator RequestAndroid (PermissionType permissionType, TaskCompletionSource<bool> permissionTask, MediaDeviceQueryPermissionsHelper requester) {
            var permission = permissionType == PermissionType.Camera ? Permission.Camera : Permission.Microphone;
            if (!Permission.HasUserAuthorizedPermission(permission))
                Permission.RequestUserPermission(permission);
            /**
             * Unity dooesn't provide a callback for completion, and doesn't provide an indeterminate state.
             * so instead we're gonna have to wait for an arbitrary amount of time then check again.
             */
            yield return new WaitForSeconds(3f); // This should be enough for user to decide
            var result = Permission.HasUserAuthorizedPermission(permission);
            permissionTask.SetResult(result);
            MonoBehaviour.Destroy(requester.gameObject);
        }

        private static IEnumerator RequestUnity (PermissionType permissionType, TaskCompletionSource<bool> permissionTask, MediaDeviceQueryPermissionsHelper requester) {
            var permission = permissionType == PermissionType.Camera ? UserAuthorization.WebCam : UserAuthorization.Microphone;
            yield return Application.RequestUserAuthorization(permission);
            permissionTask.SetResult(Application.HasUserAuthorization(permission));
            MonoBehaviour.Destroy(requester.gameObject);
        }

        private delegate void PermissionResultHandler (IntPtr context, bool granted);

        #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport(Bridge.Assembly, EntryPoint = @"NDRequestPermissions")]
        private static extern void RequestPermissions (PermissionType permissionType, PermissionResultHandler handler, IntPtr context);
        #else
        private static void RequestPermissions (PermissionType permissionType, PermissionResultHandler handler, IntPtr context) { }
        #endif

        [MonoPInvokeCallback(typeof(PermissionResultHandler))]
        private static void OnPermissionResult (IntPtr context, bool granted) {
            var handle = (GCHandle)context;
            var permissionTask = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            permissionTask.SetResult(granted);
        }

        private sealed class MediaDeviceQueryPermissionsHelper : MonoBehaviour {
            void Awake () => DontDestroyOnLoad(this.gameObject);
        }
        #endregion
    }
}