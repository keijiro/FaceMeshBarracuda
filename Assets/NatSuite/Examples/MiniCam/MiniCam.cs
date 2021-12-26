/* 
*   NatDevice
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Examples {
    
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Threading.Tasks;
    using NatSuite.Devices;

    public class MiniCam : MonoBehaviour {
        
        #region --Inspector--
        [Header("Preview")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        [Header("Buttons")]
        public CanvasGroup buttons;
        public Image flashIcon;
        public Image switchIcon;
        #endregion
        

        #region --Setup--

        MediaDeviceQuery query;
        Texture2D previewTexture;

        async void Start () {
            // Request camera permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>()) {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
            // Create a device query for device cameras
            query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            // Start camera preview
            var device = query.current as CameraDevice;
            previewTexture = await device.StartRunning();
            Debug.Log($"Started camera preview with resolution {previewTexture.width}x{previewTexture.height}");
            // Display preview texture
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
            // Set UI state
            switchIcon.color = query.count > 1 ? Color.white : Color.gray;
            flashIcon.color = device.flashSupported ? Color.white : Color.gray;
        }
        #endregion


        #region --UI Handlers--

        public async void CapturePhoto () {
            // Capture photo
            var device = query.current as CameraDevice;
            var photoTexture = await device.CapturePhoto();
            Debug.Log($"Captured photo with resolution {photoTexture.width}x{photoTexture.height}");
            // Hide buttons
            buttons.alpha = 0f;
            buttons.interactable = false;
            // Display photo texture
            rawImage.texture = photoTexture;
            aspectFitter.aspectRatio = (float)photoTexture.width / photoTexture.height;
            // Wait a few seconds
            await Task.Delay(3_000);
            // Restore preview
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
            buttons.alpha = 1f;
            buttons.interactable = true;
            // Destroy the photo texture
            Texture2D.Destroy(photoTexture);
        }

        public async void SwitchCamera () {
            // Check that there is another camera to switch to
            if (query.count < 2)
                return;
            // Stop current camera
            var device = query.current as CameraDevice;
            device.StopRunning();
            // Advance to next available camera
            query.Advance();
            // Start new camera
            device = query.current as CameraDevice;
            previewTexture = await device.StartRunning();
            // Display preview texture
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }

        public void FocusCamera (BaseEventData e) {
            // Check if focus is supported
            var device = query.current as CameraDevice;
            if (!device.focusPointSupported)
                return;
            // Get the touch position in viewport coordinates
            var eventData = e as PointerEventData;
            var transform = eventData.pointerPress.GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform,
                eventData.pressPosition,
                eventData.pressEventCamera,
                out var worldPoint
            ))
                return;
            var corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            var point = worldPoint - corners[0];
            var size = new Vector2(corners[3].x, corners[1].y) - (Vector2)corners[0];
            // Focus camera at point
            device.focusPoint = (point.x / size.x, point.y / size.y);
        }

        public void ToggleFlashMode () {
            // Check if flash is supported
            var device = query.current as CameraDevice;
            if (!device.flashSupported)
                return;
            // Toggle
            if (device.flashMode == FlashMode.On) {
                device.flashMode = FlashMode.Off;
                flashIcon.color = Color.gray;
            } else {
                device.flashMode = FlashMode.On;
                flashIcon.color = Color.white;
            }
        }
        #endregion


        #region --Operations--

        void OnDisable () {
            if (query.current.running)
                query.current.StopRunning();
        }
        #endregion
    }
}