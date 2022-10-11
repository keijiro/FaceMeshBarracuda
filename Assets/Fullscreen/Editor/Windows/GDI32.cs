using System;
using System.Runtime.InteropServices;

namespace FullscreenEditor.Windows {
    internal static class GDI32 {

        // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        public enum DeviceCap {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

    }
}
