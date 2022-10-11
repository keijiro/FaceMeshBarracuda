using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FullscreenEditor.Windows;
using UnityEditorInternal;
using UnityEngine;

namespace FullscreenEditor {

    [System.Serializable]
    public class DisplayInfo {
        public bool PrimaryDisplay;

        public float ScreenHeight {
            get { return MonitorArea.yMax - MonitorArea.yMin; }
        }
        public float ScreenWidth {
            get { return MonitorArea.xMax - MonitorArea.xMin; }
        }

        public float scaleFactor2;

        public int LogicalScreenHeight;
        public int PhysicalScreenHeight;

        public string DeviceName {
            get { return displayDevice.DeviceName; }
        }

        public string FriendlyName {
            get { return displayDevice.DeviceString; }
        }

        public Rect MonitorArea;
        public Rect WorkArea;

        internal DevMode devMode;
        internal DisplayDevice displayDevice;

        public Rect DpiCorrectedArea {
            get {
                var firstDisplayInfo = DisplayInfo.GetDisplay(0);
                var monitorArea = MonitorArea;

                var origin = monitorArea.min;
                var size = monitorArea.size;

                return new Rect(
                    Mathf.Round(origin.x / firstDisplayInfo.scaleFactor2),
                    Mathf.Round(origin.y / firstDisplayInfo.scaleFactor2),
                    Mathf.Round(size.x / scaleFactor2),
                    Mathf.Round(size.y / scaleFactor2)
                );
            }
        }

        public Rect UnityCorrectedArea {
            get {
                var rect = DpiCorrectedArea;
                return InternalEditorUtility.GetBoundsOfDesktopAtPoint(rect.center);
            }
        }

        public Rect PhysicalArea {
            get {
                return new Rect(
                    devMode.dmPositionX,
                    devMode.dmPositionY,
                    devMode.dmPelsWidth,
                    devMode.dmPelsHeight
                );
            }
        }

        public float scaleFactor {
            get { return devMode.dmPelsWidth / (MonitorArea.xMax - MonitorArea.xMin); }
        }

        public static List<DisplayInfo> GetDisplays() {
            var list = new List<DisplayInfo>();

            try {
                User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeRect lprcMonitor, IntPtr dwData) => {
                        var mi = new MonitorInfoEx();
                        mi.Init();
                        mi.size = Marshal.SizeOf(mi);
                        mi.size = 72;
                        var success = User32.GetMonitorInfo(hMonitor, ref mi);
                        if (success) {
                            var di = new DisplayInfo();
                            di.MonitorArea = mi.monitor;
                            di.WorkArea = mi.work;
                            di.PrimaryDisplay = (mi.flags & 1) != 0;

                            di.LogicalScreenHeight = GDI32.GetDeviceCaps(hMonitor, (int)GDI32.DeviceCap.VERTRES);
                            di.PhysicalScreenHeight = GDI32.GetDeviceCaps(hMonitor, (int)GDI32.DeviceCap.DESKTOPVERTRES);

                            // TransformToPixels(0, 0, out var x, out var y);

                            uint dpiX;
                            uint dpiY;

                            try {
                                ShCore.GetDpiForMonitor(
                                    hMonitor,
                                    MonitorDpiType.MDT_EFFECTIVE_DPI,
                                    out dpiX,
                                    out dpiY
                                );
                            } catch {
                                dpiX = 96;
                                dpiY = 96;
                            }

                            di.scaleFactor2 = dpiX / 96f;
                            list.Add(di);
                        } else {
                            Logger.Debug("Getting monitor info failed");
                        }

                        return true;
                    }, IntPtr.Zero);

                AddAdditionalInfos(list);
            } catch (Exception e) {
                Logger.Exception(e);
            }

            return list;
        }

        public static DisplayInfo GetDisplay(int index) {
            var displays = GetDisplays();

            if (displays != null && index >= 0 && index < displays.Count) {
                return displays[index];
            }

            return null;
        }

        private static void AddAdditionalInfos(List<DisplayInfo> displayInfo) {
            for (int id = 0; id < displayInfo.Count; id++) {
                var vDevMode = new DevMode();
                var d = new DisplayDevice();

                d.cb = Marshal.SizeOf(d);

                try {
                    User32.EnumDisplayDevices(displayInfo[id].DeviceName, 0, ref d, 0);

                    d.cb = Marshal.SizeOf(d);

                    if ((d.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == DisplayDeviceStateFlags.AttachedToDesktop) {
                        User32.EnumDisplaySettings(displayInfo[id].DeviceName, -1, ref vDevMode);
                        displayInfo[id].devMode = vDevMode;
                    }

                    displayInfo[id].displayDevice = d;
                } catch (Exception e) {
                    Logger.Exception(e);
                }
            }
        }

        private static void TransformToPixels(double unitX, double unitY, out int pixelX, out int pixelY) {
            var hDc = User32.GetDC(IntPtr.Zero);

            if (hDc != IntPtr.Zero) {
                var dpiX = GDI32.GetDeviceCaps(hDc, (int)GDI32.DeviceCap.VERTRES);
                var dpiY = GDI32.GetDeviceCaps(hDc, (int)GDI32.DeviceCap.DESKTOPVERTRES);

                User32.ReleaseDC(IntPtr.Zero, hDc);

                pixelX = (int)(((double)dpiX / 96) * unitX);
                pixelY = (int)(((double)dpiY / 96) * unitY);
            } else {
                throw new ArgumentNullException("Failed to get DC.");
            }
        }

    }
}
