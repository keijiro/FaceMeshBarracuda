using System;
using System.Runtime.InteropServices;

namespace FullscreenEditor.Windows {

    internal enum MonitorDpiType {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
    }

    internal static class ShCore {

        [DllImport("shcore.dll")]
        internal static extern uint GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    }
}
