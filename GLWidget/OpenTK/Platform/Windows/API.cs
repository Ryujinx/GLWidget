//
// WinRawJoystick.cs
//
// Author:
//       Stefanos A. <stapostol@gmail.com>
//
// Copyright (c) 2006 Stefanos Apostolopoulos
// Copyright (c) 2007 Erik Ylvisaker
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Security;

/* TODO: Update the description of TimeBeginPeriod and other native methods. Update Timer. */

#pragma warning disable 3019    // CLS-compliance checking
#pragma warning disable 0649    // struct members not explicitly initialized
#pragma warning disable 0169    // field / method is never used.
#pragma warning disable 0414    // field assigned but never used.

namespace OpenTK.Platform.Windows
{
    using HWND = System.IntPtr;
    using HINSTANCE = System.IntPtr;
    using HMENU = System.IntPtr;
    using HICON = System.IntPtr;
    using HBRUSH = System.IntPtr;
    using HCURSOR = System.IntPtr;
    using HKEY = System.IntPtr;
    using PHKEY = System.IntPtr;

    using HDROP = System.IntPtr;

    using LRESULT = System.IntPtr;
    using LPVOID = System.IntPtr;
    using LPCTSTR = System.String;

    using WPARAM = System.IntPtr;
    using LPARAM = System.IntPtr;
    using HANDLE = System.IntPtr;
    using HRAWINPUT = System.IntPtr;

    using BYTE = System.Byte;
    using SHORT = System.Int16;
    using USHORT = System.UInt16;
    using LONG = System.Int32;
    using ULONG = System.UInt32;
    using WORD = System.Int16;
    using DWORD = System.Int32;
    using BOOL = System.Boolean;
    using INT = System.Int32;
    using UINT = System.UInt32;
    using LONG_PTR = System.IntPtr;
    using ATOM = System.Int32;

    using COLORREF = System.Int32;
    using RECT = OpenTK.Platform.Windows.Win32Rectangle;
    using WNDPROC = System.IntPtr;
    using LPDEVMODE = DeviceMode;
    using HDEVNOTIFY = System.IntPtr;

    using HRESULT = System.IntPtr;
    using HMONITOR = System.IntPtr;

    using DWORD_PTR = System.IntPtr;
    using UINT_PTR = System.UIntPtr;

    using TIMERPROC = Functions.TimerProc;

    using REGSAM = System.UInt32;
    using System.Diagnostics;

    /// \internal
    /// <summary>
    /// For internal use by OpenTK only!
    /// Exposes useful native WINAPI methods and structures.
    /// </summary>
    internal static class API
    {
        // Prevent BeforeFieldInit optimization, and initialize 'size' fields.
        static API()
        {
            PixelFormatDescriptorVersion = 1;
            PixelFormatDescriptorSize = (short)Marshal.SizeOf(typeof(PixelFormatDescriptor));
            WindowInfoSize = Marshal.SizeOf(typeof(WindowInfo));
        }

        internal static readonly short PixelFormatDescriptorSize;
        internal static readonly short PixelFormatDescriptorVersion;
        internal static readonly int WindowInfoSize;
    }


    internal static class Functions
    {


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern ushort RegisterClass(ref WindowClass window_class);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern ushort RegisterClassEx(ref ExtendedWindowClass window_class);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern short UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR className, IntPtr instance);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern short UnregisterClass(IntPtr className, IntPtr instance);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public extern static IntPtr DefWindowProc(HWND hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="DC"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr hwnd, IntPtr DC);

        [DllImport("gdi32.dll")]
        internal static extern int ChoosePixelFormat(IntPtr dc, ref PixelFormatDescriptor pfd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(
            ExtendedWindowStyle ExStyle,
            [MarshalAs(UnmanagedType.LPTStr)] string className,
            [MarshalAs(UnmanagedType.LPTStr)] string windowName,
            WindowStyle Style,
            int X, int Y,
            int Width, int Height,
            IntPtr HandleToParentWindow,
            IntPtr Menu,
            IntPtr Instance,
            IntPtr Param);
        /*
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int CreateWindowEx(
            [In]ExtendedWindowStyle ExStyle,
            [In]IntPtr ClassName,
            [In]IntPtr WindowName,
            [In]WindowStyle Style,
            [In]int X, [In]int Y,
            [In]int Width, [In]int Height,
            [In]IntPtr HandleToParentWindow,
            [In]IntPtr Menu,
            [In]IntPtr Instance,
            [In]IntPtr Param);
        */
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(
            ExtendedWindowStyle ExStyle,
            IntPtr ClassAtom,
            IntPtr WindowName,
            WindowStyle Style,
            int X, int Y,
            int Width, int Height,
            IntPtr HandleToParentWindow,
            IntPtr Menu,
            IntPtr Instance,
            IntPtr Param);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(IntPtr windowHandle);

        [DllImport("gdi32.dll")]
        internal static extern int DescribePixelFormat(IntPtr deviceContext, int pixel, int pfdSize, ref PixelFormatDescriptor pixelFormat);

        /// <summary>
        ///
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="format"></param>
        /// <param name="pfd"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetPixelFormat(IntPtr dc, int format, ref PixelFormatDescriptor pfd);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SwapBuffers(IntPtr dc);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr handle, string funcname);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr handle, IntPtr funcname);

        /// <summary>
        ///
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern BOOL EnumDisplayDevices([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpDevice,
            DWORD iDevNum, [In, Out] WindowsDisplayDevice lpDisplayDevice, DWORD dwFlags);

        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern BOOL EnumDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpszDeviceName, DisplayModeSettingsEnum iModeNum,
            [In, Out] DeviceMode lpDevMode, DWORD dwFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern BOOL EnumDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpszDeviceName, DWORD iModeNum,
            [In, Out] DeviceMode lpDevMode, DWORD dwFlags);

        /// <summary>
        /// Sets the current process as dots per inch (dpi) aware.
        /// Note: SetProcessDPIAware is subject to a possible race condition
        /// if a DLL caches dpi settings during initialization.
        /// For this reason, it is recommended that dpi-aware be set through
        /// the application (.exe) manifest rather than by calling SetProcessDPIAware.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is true.
        /// Otherwise, the return value is false.
        /// </returns>
        /// <remarks>
        /// DLLs should accept the dpi setting of the host process
        /// rather than call SetProcessDPIAware themselves.
        /// To be set properly, dpiAware should be specified as part
        /// of the application (.exe) manifest.
        /// </remarks>
        [DllImport("user32.dll")]
        internal static extern BOOL SetProcessDPIAware();

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void TimerProc(HWND hwnd, WindowMessage uMsg, UINT_PTR idEvent, DWORD dwTime);
    }


    internal static class Constants
    {
        // Found in winuser.h
        internal const int KEYBOARD_OVERRUN_MAKE_CODE = 0xFF;

        // WM_ACTIVATE state values (found in winuser.h)
        internal const int WA_INACTIVE = 0;
        internal const int WA_ACTIVE = 1;
        internal const int WA_CLICKACTIVE = 2;

        // Window Messages (found in winuser.h)
        internal const int WM_NULL = 0x0000;
        internal const int WM_CREATE = 0x0001;
        internal const int WM_DESTROY = 0x0002;
        internal const int WM_MOVE = 0x0003;
        internal const int WM_SIZE = 0x0005;
        internal const int WM_ACTIVATE = 0x0006;
        internal const int WM_SETFOCUS = 0x0007;
        internal const int WM_KILLFOCUS = 0x0008;
        internal const int WM_ENABLE = 0x000A;
        internal const int WM_SETREDRAW = 0x000B;
        internal const int WM_SETTEXT = 0x000C;
        internal const int WM_GETTEXT = 0x000D;
        internal const int WM_GETTEXTLENGTH = 0x000E;
        internal const int WM_PAINT = 0x000F;
        internal const int WM_CLOSE = 0x0010;
        // _WIN32_WCE
        internal const int WM_QUERYENDSESSION = 0x0011;
        internal const int WM_QUERYOPEN = 0x0013;
        internal const int WM_ENDSESSION = 0x0016;
        internal const int WM_QUIT = 0x0012;
        internal const int WM_ERASEBKGND = 0x0014;
        internal const int WM_SYSCOLORCHANGE = 0x0015;
        internal const int WM_SHOWWINDOW = 0x0018;
        internal const int WM_WININICHANGE = 0x001A;
        // WINVER >= 0x400
        internal const int WM_SETTINGCHANGE = WM_WININICHANGE;

        internal const int WM_DEVMODECHANGE = 0x001B;
        internal const int WM_ACTIVATEAPP = 0x001C;
        internal const int WM_FONTCHANGE = 0x001D;
        internal const int WM_TIMECHANGE = 0x001E;
        internal const int WM_CANCELMODE = 0x001F;
        internal const int WM_SETCURSOR = 0x0020;
        internal const int WM_MOUSEACTIVATE = 0x0021;
        internal const int WM_CHILDACTIVATE = 0x0022;
        internal const int WM_QUEUESYNC = 0x0023;

        internal const int WM_GETMINMAXINFO = 0x0024;

        internal const int WM_WINDOWPOSCHANGING = 0x0046;
        internal const int WM_WINDOWPOSCHANGED = 0x0047;

        // Keyboard events (found in winuser.h)
        internal const int WM_INPUT = 0x00FF;       // Raw input. XP and higher only.
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_SYSKEYDOWN = 0x0104;
        internal const int WM_SYSKEYUP = 0x0105;
        internal const int WM_COMMAND = 0x0111;
        internal const int WM_SYSCOMMAND = 0x0112;
        internal const int WM_ENTERIDLE = 0x121;

        // Pixel types (found in wingdi.h)
        internal const byte PFD_TYPE_RGBA = 0;
        internal const byte PFD_TYPE_COLORINDEX = 1;

        // Layer types (found in wingdi.h)
        internal const byte PFD_MAIN_PLANE = 0;
        internal const byte PFD_OVERLAY_PLANE = 1;
        internal const byte PFD_UNDERLAY_PLANE = unchecked((byte)-1);

        // Device mode types (found in wingdi.h)
        internal const int DM_LOGPIXELS = 0x00020000;
        internal const int DM_BITSPERPEL = 0x00040000;
        internal const int DM_PELSWIDTH = 0x00080000;
        internal const int DM_PELSHEIGHT = 0x00100000;
        internal const int DM_DISPLAYFLAGS = 0x00200000;
        internal const int DM_DISPLAYFREQUENCY = 0x00400000;

        // ChangeDisplaySettings results (found in winuser.h)
        internal const int DISP_CHANGE_SUCCESSFUL = 0;
        internal const int DISP_CHANGE_RESTART = 1;
        internal const int DISP_CHANGE_FAILED = -1;

        // (found in winuser.h)
        internal const int ENUM_REGISTRY_SETTINGS = -2;
        internal const int ENUM_CURRENT_SETTINGS = -1;

        internal static readonly IntPtr MESSAGE_ONLY = new IntPtr(-3);

        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

        // System Error Codes
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms681381(v=vs.85).aspx

        /// <summary>
        /// The point passed to GetMouseMovePoints is not in the buffer.
        /// </summary>
        internal const int ERROR_POINT_NOT_FOUND = 1171;

        /// <summary>
        /// Retrieves the points using the display resolution.
        /// </summary>
        internal const int GMMP_USE_DISPLAY_POINTS = 1;

        /// <summary>
        /// Retrieves high resolution points. Points can range from zero to
        /// 65,535 (0xFFFF) in both x and y coordinates. This is the resolution
        /// provided by absolute coordinate pointing devices such as drawing
        /// tablets.
        /// </summary>
        internal const int GMMP_USE_HIGH_RESOLUTION_POINTS = 2;
    }

    /// \internal
    /// <summary>
    /// Describes a pixel format. It is used when interfacing with the WINAPI to create a new Context.
    /// Found in WinGDI.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PixelFormatDescriptor
    {
        internal short Size;
        internal short Version;
        internal PixelFormatDescriptorFlags Flags;
        internal PixelType PixelType;
        internal byte ColorBits;
        internal byte RedBits;
        internal byte RedShift;
        internal byte GreenBits;
        internal byte GreenShift;
        internal byte BlueBits;
        internal byte BlueShift;
        internal byte AlphaBits;
        internal byte AlphaShift;
        internal byte AccumBits;
        internal byte AccumRedBits;
        internal byte AccumGreenBits;
        internal byte AccumBlueBits;
        internal byte AccumAlphaBits;
        internal byte DepthBits;
        internal byte StencilBits;
        internal byte AuxBuffers;
        internal byte LayerType;
        private byte Reserved;
        internal int LayerMask;
        internal int VisibleMask;
        internal int DamageMask;
    }



    /// \internal
    /// <summary>
    /// Describes the pixel format of a drawing surface.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LayerPlaneDescriptor
    {
        internal static readonly WORD Size = (WORD)Marshal.SizeOf(typeof(LayerPlaneDescriptor));
        internal WORD Version;
        internal DWORD Flags;
        internal BYTE PixelType;
        internal BYTE ColorBits;
        internal BYTE RedBits;
        internal BYTE RedShift;
        internal BYTE GreenBits;
        internal BYTE GreenShift;
        internal BYTE BlueBits;
        internal BYTE BlueShift;
        internal BYTE AlphaBits;
        internal BYTE AlphaShift;
        internal BYTE AccumBits;
        internal BYTE AccumRedBits;
        internal BYTE AccumGreenBits;
        internal BYTE AccumBlueBits;
        internal BYTE AccumAlphaBits;
        internal BYTE DepthBits;
        internal BYTE StencilBits;
        internal BYTE AuxBuffers;
        internal BYTE LayerPlane;
        private BYTE Reserved;
        internal COLORREF crTransparent;
    }

    /// \internal
    /// <summary>
    /// The <b>GlyphMetricsFloat</b> structure contains information about the placement and orientation of a glyph in a
    /// character cell.
    /// </summary>
    /// <remarks>The values of <b>GlyphMetricsFloat</b> are specified as notional units.</remarks>
    /// <seealso cref="PointFloat" />
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphMetricsFloat
    {
        /// <summary>
        /// Specifies the width of the smallest rectangle (the glyph's black box) that completely encloses the glyph.
        /// </summary>
        internal float BlackBoxX;
        /// <summary>
        /// Specifies the height of the smallest rectangle (the glyph's black box) that completely encloses the glyph.
        /// </summary>
        internal float BlackBoxY;
        /// <summary>
        /// Specifies the x and y coordinates of the upper-left corner of the smallest rectangle that completely encloses the glyph.
        /// </summary>
        internal PointFloat GlyphOrigin;
        /// <summary>
        /// Specifies the horizontal distance from the origin of the current character cell to the origin of the next character cell.
        /// </summary>
        internal float CellIncX;
        /// <summary>
        /// Specifies the vertical distance from the origin of the current character cell to the origin of the next character cell.
        /// </summary>
        internal float CellIncY;
    }

    /// \internal
    /// <summary>
    /// The <b>PointFloat</b> structure contains the x and y coordinates of a point.
    /// </summary>
    /// <seealso cref="GlyphMetricsFloat" />
    [StructLayout(LayoutKind.Sequential)]
    internal struct PointFloat
    {
        /// <summary>
        /// Specifies the horizontal (x) coordinate of a point.
        /// </summary>
        internal float X;
        /// <summary>
        /// Specifies the vertical (y) coordinate of a point.
        /// </summary>
        internal float Y;
    };
    /*
    typedef struct _devicemode {
      BCHAR  dmDeviceName[CCHDEVICENAME];
      WORD   dmSpecVersion;
      WORD   dmDriverVersion;
      WORD   dmSize;
      WORD   dmDriverExtra;
      DWORD  dmFields;
      union {
        struct {
          short dmOrientation;
          short dmPaperSize;
          short dmPaperLength;
          short dmPaperWidth;
          short dmScale;
          short dmCopies;
          short dmDefaultSource;
          short dmPrintQuality;
        };
        POINTL dmPosition;
        DWORD  dmDisplayOrientation;
        DWORD  dmDisplayFixedOutput;
      };

      short  dmColor;
      short  dmDuplex;
      short  dmYResolution;
      short  dmTTOption;
      short  dmCollate;
      BYTE  dmFormName[CCHFORMNAME];
      WORD  dmLogPixels;
      DWORD  dmBitsPerPel;
      DWORD  dmPelsWidth;
      DWORD  dmPelsHeight;
      union {
        DWORD  dmDisplayFlags;
        DWORD  dmNup;
      }
      DWORD  dmDisplayFrequency;
    #if(WINVER >= 0x0400)
      DWORD  dmICMMethod;
      DWORD  dmICMIntent;
      DWORD  dmMediaType;
      DWORD  dmDitherType;
      DWORD  dmReserved1;
      DWORD  dmReserved2;
    #if (WINVER >= 0x0500) || (_WIN32_WINNT >= 0x0400)
      DWORD  dmPanningWidth;
      DWORD  dmPanningHeight;
    #endif
    #endif
    } DEVMODE;
    */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class DeviceMode
    {
        internal DeviceMode()
        {
            Size = (short)Marshal.SizeOf(this);
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string DeviceName;
        internal short SpecVersion;
        internal short DriverVersion;
        private short Size;
        internal short DriverExtra;
        internal int Fields;

        //internal short Orientation;
        //internal short PaperSize;
        //internal short PaperLength;
        //internal short PaperWidth;
        //internal short Scale;
        //internal short Copies;
        //internal short DefaultSource;
        //internal short PrintQuality;

        internal POINT Position;
        internal DWORD DisplayOrientation;
        internal DWORD DisplayFixedOutput;

        internal short Color;
        internal short Duplex;
        internal short YResolution;
        internal short TTOption;
        internal short Collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string FormName;
        internal short LogPixels;
        internal int BitsPerPel;
        internal int PelsWidth;
        internal int PelsHeight;
        internal int DisplayFlags;
        internal int DisplayFrequency;
        internal int ICMMethod;
        internal int ICMIntent;
        internal int MediaType;
        internal int DitherType;
        internal int Reserved1;
        internal int Reserved2;
        internal int PanningWidth;
        internal int PanningHeight;
    }

    /// \internal
    /// <summary>
    /// The DISPLAY_DEVICE structure receives information about the display device specified by the iDevNum parameter of the EnumDisplayDevices function.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class WindowsDisplayDevice
    {
        internal WindowsDisplayDevice()
        {
            size = (short)Marshal.SizeOf(this);
        }

        private readonly DWORD size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceString;
        internal DisplayDeviceStateFlags StateFlags;    // DWORD
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceKey;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowClass
    {
        internal ClassStyle Style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        internal WindowProcedure WindowProcedure;
        internal int ClassExtraBytes;
        internal int WindowExtraBytes;
        //[MarshalAs(UnmanagedType.
        internal IntPtr Instance;
        internal IntPtr Icon;
        internal IntPtr Cursor;
        internal IntPtr BackgroundBrush;
        //[MarshalAs(UnmanagedType.LPStr)]
        internal IntPtr MenuName;
        [MarshalAs(UnmanagedType.LPTStr)]
        internal string ClassName;
        //internal string ClassName;

        internal static int SizeInBytes = Marshal.SizeOf(default(WindowClass));
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct ExtendedWindowClass
    {
        public UINT Size;
        public ClassStyle Style;
        //public WNDPROC WndProc;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WindowProcedure WndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public HINSTANCE Instance;
        public HICON Icon;
        public HCURSOR Cursor;
        public HBRUSH Background;
        public IntPtr MenuName;
        public IntPtr ClassName;
        public HICON IconSm;

        public static uint SizeInBytes = (uint)Marshal.SizeOf(default(ExtendedWindowClass));
    }

    /// \internal
    /// <summary>
    /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive. In other words, the pixel whose coordinates are (right, bottom) lies immediately outside of the the rectangle. For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including, the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Rectangle
    {
        /// <summary>
        /// Specifies the x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal LONG left;
        /// <summary>
        /// Specifies the y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal LONG top;
        /// <summary>
        /// Specifies the x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal LONG right;
        /// <summary>
        /// Specifies the y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal LONG bottom;

        internal int Width { get { return right - left; } }
        internal int Height { get { return bottom - top; } }

        public override string ToString()
        {
            return String.Format("({0},{1})-({2},{3})", left, top, right, bottom);
        }
    }


    /// \internal
    /// <summary>
    /// Contains window information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowInfo
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public DWORD Size;
        /// <summary>
        /// Pointer to a RECT structure that specifies the coordinates of the window.
        /// </summary>
        public RECT Window;
        /// <summary>
        /// Pointer to a RECT structure that specifies the coordinates of the client area.
        /// </summary>
        public RECT Client;
        /// <summary>
        /// The window styles. For a table of window styles, see CreateWindowEx.
        /// </summary>
        public WindowStyle Style;
        /// <summary>
        /// The extended window styles. For a table of extended window styles, see CreateWindowEx.
        /// </summary>
        public ExtendedWindowStyle ExStyle;
        /// <summary>
        /// The window status. If this member is WS_ACTIVECAPTION, the window is active. Otherwise, this member is zero.
        /// </summary>
        public DWORD WindowStatus;
        /// <summary>
        /// The width of the window border, in pixels.
        /// </summary>
        public UINT WindowBordersX;
        /// <summary>
        /// The height of the window border, in pixels.
        /// </summary>
        public UINT WindowBordersY;
        /// <summary>
        /// The window class atom (see RegisterClass).
        /// </summary>
        public ATOM WindowType;
        /// <summary>
        /// The Microsoft Windows version of the application that created the window.
        /// </summary>
        public WORD CreatorVersion;
    }

    internal struct MonitorInfo
    {
        public DWORD Size;
        public RECT Monitor;
        public RECT Work;
        public DWORD Flags;

        public static readonly int SizeInBytes = Marshal.SizeOf(default(MonitorInfo));
    }

    internal struct TrackMouseEventStructure
    {
        public DWORD Size;
        public TrackMouseEventFlags Flags;
        public HWND TrackWindowHandle;
        public DWORD HoverTime;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(TrackMouseEventStructure));
    }

    internal enum DisplayModeSettingsEnum
    {
        CurrentSettings = -1,
        RegistrySettings = -2
    }

    [Flags]
    internal enum DisplayDeviceStateFlags
    {
        None = 0x00000000,
        AttachedToDesktop = 0x00000001,
        MultiDriver = 0x00000002,
        PrimaryDevice = 0x00000004,
        MirroringDriver = 0x00000008,
        VgaCompatible = 0x00000010,
        Removable = 0x00000020,
        ModesPruned = 0x08000000,
        Remote = 0x04000000,
        Disconnect = 0x02000000,

        // Child device state
        Active = 0x00000001,
        Attached = 0x00000002,
    }

    [Flags]
    internal enum WindowStyle : uint
    {
        Overlapped = 0x00000000,
        Popup = 0x80000000,
        Child = 0x40000000,
        Minimize = 0x20000000,
        Visible = 0x10000000,
        Disabled = 0x08000000,
        ClipSiblings = 0x04000000,
        ClipChildren = 0x02000000,
        Maximize = 0x01000000,
        Caption = 0x00C00000,    // Border | DialogFrame
        Border = 0x00800000,
        DialogFrame = 0x00400000,
        VScroll = 0x00200000,
        HScreen = 0x00100000,
        SystemMenu = 0x00080000,
        ThickFrame = 0x00040000,
        Group = 0x00020000,
        TabStop = 0x00010000,

        MinimizeBox = 0x00020000,
        MaximizeBox = 0x00010000,

        Tiled = Overlapped,
        Iconic = Minimize,
        SizeBox = ThickFrame,
        TiledWindow = OverlappedWindow,

        // Common window styles:
        OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
        PopupWindow = Popup | Border | SystemMenu,
        ChildWindow = Child
    }

    [Flags]
    internal enum ExtendedWindowStyle : uint
    {
        DialogModalFrame = 0x00000001,
        NoParentNotify = 0x00000004,
        Topmost = 0x00000008,
        AcceptFiles = 0x00000010,
        Transparent = 0x00000020,

        // #if(WINVER >= 0x0400)
        MdiChild = 0x00000040,
        ToolWindow = 0x00000080,
        WindowEdge = 0x00000100,
        ClientEdge = 0x00000200,
        ContextHelp = 0x00000400,
        // #endif

        // #if(WINVER >= 0x0400)
        Right = 0x00001000,
        Left = 0x00000000,
        RightToLeftReading = 0x00002000,
        LeftToRightReading = 0x00000000,
        LeftScrollbar = 0x00004000,
        RightScrollbar = 0x00000000,

        ControlParent = 0x00010000,
        StaticEdge = 0x00020000,
        ApplicationWindow = 0x00040000,

        OverlappedWindow = WindowEdge | ClientEdge,
        PaletteWindow = WindowEdge | ToolWindow | Topmost,
        // #endif

        // #if(_WIN32_WINNT >= 0x0500)
        Layered = 0x00080000,
        // #endif

        // #if(WINVER >= 0x0500)
        NoInheritLayout = 0x00100000, // Disable inheritence of mirroring by children
        RightToLeftLayout = 0x00400000, // Right to left mirroring
        // #endif /* WINVER >= 0x0500 */

        // #if(_WIN32_WINNT >= 0x0501)
        Composited = 0x02000000,
        // #endif /* _WIN32_WINNT >= 0x0501 */

        // #if(_WIN32_WINNT >= 0x0500)
        NoActivate = 0x08000000
        // #endif /* _WIN32_WINNT >= 0x0500 */
    }

    [Flags]
    internal enum PixelFormatDescriptorFlags : int
    {
        // PixelFormatDescriptor flags
        DOUBLEBUFFER = 0x01,
        STEREO = 0x02,
        DRAW_TO_WINDOW = 0x04,
        DRAW_TO_BITMAP = 0x08,
        SUPPORT_GDI = 0x10,
        SUPPORT_OPENGL = 0x20,
        GENERIC_FORMAT = 0x40,
        NEED_PALETTE = 0x80,
        NEED_SYSTEM_PALETTE = 0x100,
        SWAP_EXCHANGE = 0x200,
        SWAP_COPY = 0x400,
        SWAP_LAYER_BUFFERS = 0x800,
        GENERIC_ACCELERATED = 0x1000,
        SUPPORT_DIRECTDRAW = 0x2000,
        SUPPORT_COMPOSITION = 0x8000,

        // PixelFormatDescriptor flags for use in ChoosePixelFormat only
        DEPTH_DONTCARE = unchecked((int)0x20000000),
        DOUBLEBUFFER_DONTCARE = unchecked((int)0x40000000),
        STEREO_DONTCARE = unchecked((int)0x80000000)
    }
    internal enum PixelType : byte
    {
        RGBA = 0,
        INDEXED = 1
    }

    [Flags]
    internal enum ClassStyle
    {
        //None            = 0x0000,
        VRedraw = 0x0001,
        HRedraw = 0x0002,
        DoubleClicks = 0x0008,
        OwnDC = 0x0020,
        ClassDC = 0x0040,
        ParentDC = 0x0080,
        NoClose = 0x0200,
        SaveBits = 0x0800,
        ByteAlignClient = 0x1000,
        ByteAlignWindow = 0x2000,
        GlobalClass = 0x4000,

        Ime = 0x00010000,

        // #if(_WIN32_WINNT >= 0x0501)
        DropShadow = 0x00020000
        // #endif /* _WIN32_WINNT >= 0x0501 */
    }
 
    internal enum WindowMessage : int
    {
        NULL = 0x0000,
        CREATE = 0x0001,
        DESTROY = 0x0002,
        MOVE = 0x0003,
        SIZE = 0x0005,
        ACTIVATE = 0x0006,
        SETFOCUS = 0x0007,
        KILLFOCUS = 0x0008,
        //              internal const uint SETVISIBLE           = 0x0009;
        ENABLE = 0x000A,
        SETREDRAW = 0x000B,
        SETTEXT = 0x000C,
        GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E,
        PAINT = 0x000F,
        CLOSE = 0x0010,
        QUERYENDSESSION = 0x0011,
        QUIT = 0x0012,
        QUERYOPEN = 0x0013,
        ERASEBKGND = 0x0014,
        SYSCOLORCHANGE = 0x0015,
        ENDSESSION = 0x0016,
        //              internal const uint SYSTEMERROR          = 0x0017;
        SHOWWINDOW = 0x0018,
        CTLCOLOR = 0x0019,
        WININICHANGE = 0x001A,
        SETTINGCHANGE = 0x001A,
        DEVMODECHANGE = 0x001B,
        ACTIVATEAPP = 0x001C,
        FONTCHANGE = 0x001D,
        TIMECHANGE = 0x001E,
        CANCELMODE = 0x001F,
        SETCURSOR = 0x0020,
        MOUSEACTIVATE = 0x0021,
        CHILDACTIVATE = 0x0022,
        QUEUESYNC = 0x0023,
        GETMINMAXINFO = 0x0024,
        PAINTICON = 0x0026,
        ICONERASEBKGND = 0x0027,
        NEXTDLGCTL = 0x0028,
        //              internal const uint ALTTABACTIVE         = 0x0029;
        SPOOLERSTATUS = 0x002A,
        DRAWITEM = 0x002B,
        MEASUREITEM = 0x002C,
        DELETEITEM = 0x002D,
        VKEYTOITEM = 0x002E,
        CHARTOITEM = 0x002F,
        SETFONT = 0x0030,
        GETFONT = 0x0031,
        SETHOTKEY = 0x0032,
        GETHOTKEY = 0x0033,
        //              internal const uint FILESYSCHANGE        = 0x0034;
        //              internal const uint ISACTIVEICON         = 0x0035;
        //              internal const uint QUERYPARKICON        = 0x0036;
        QUERYDRAGICON = 0x0037,
        COMPAREITEM = 0x0039,
        //              internal const uint TESTING              = 0x003a;
        //              internal const uint OTHERWINDOWCREATED = 0x003c;
        GETOBJECT = 0x003D,
        //                      internal const uint ACTIVATESHELLWINDOW        = 0x003e;
        COMPACTING = 0x0041,
        COMMNOTIFY = 0x0044,
        WINDOWPOSCHANGING = 0x0046,
        WINDOWPOSCHANGED = 0x0047,
        POWER = 0x0048,
        COPYDATA = 0x004A,
        CANCELJOURNAL = 0x004B,
        NOTIFY = 0x004E,
        INPUTLANGCHANGEREQUEST = 0x0050,
        INPUTLANGCHANGE = 0x0051,
        TCARD = 0x0052,
        HELP = 0x0053,
        USERCHANGED = 0x0054,
        NOTIFYFORMAT = 0x0055,
        CONTEXTMENU = 0x007B,
        STYLECHANGING = 0x007C,
        STYLECHANGED = 0x007D,
        DISPLAYCHANGE = 0x007E,
        GETICON = 0x007F,

        // Non-Client messages
        SETICON = 0x0080,
        NCCREATE = 0x0081,
        NCDESTROY = 0x0082,
        NCCALCSIZE = 0x0083,
        NCHITTEST = 0x0084,
        NCPAINT = 0x0085,
        NCACTIVATE = 0x0086,
        GETDLGCODE = 0x0087,
        SYNCPAINT = 0x0088,
        //              internal const uint SYNCTASK       = 0x0089;
        NCMOUSEMOVE = 0x00A0,
        NCLBUTTONDOWN = 0x00A1,
        NCLBUTTONUP = 0x00A2,
        NCLBUTTONDBLCLK = 0x00A3,
        NCRBUTTONDOWN = 0x00A4,
        NCRBUTTONUP = 0x00A5,
        NCRBUTTONDBLCLK = 0x00A6,
        NCMBUTTONDOWN = 0x00A7,
        NCMBUTTONUP = 0x00A8,
        NCMBUTTONDBLCLK = 0x00A9,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONDOWN = 0x00ab,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONUP = 0x00ac,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONDBLCLK = 0x00ad,

        INPUT = 0x00FF,

        KEYDOWN = 0x0100,
        KEYFIRST = 0x0100,
        KEYUP = 0x0101,
        CHAR = 0x0102,
        DEADCHAR = 0x0103,
        SYSKEYDOWN = 0x0104,
        SYSKEYUP = 0x0105,
        SYSCHAR = 0x0106,
        SYSDEADCHAR = 0x0107,
        KEYLAST = 0x0108,
        UNICHAR = 0x0109,



        IME_STARTCOMPOSITION = 0x010D,
        IME_ENDCOMPOSITION = 0x010E,
        IME_COMPOSITION = 0x010F,
        IME_KEYLAST = 0x010F,
        INITDIALOG = 0x0110,
        COMMAND = 0x0111,
        SYSCOMMAND = 0x0112,
        TIMER = 0x0113,
        HSCROLL = 0x0114,
        VSCROLL = 0x0115,
        INITMENU = 0x0116,
        INITMENUPOPUP = 0x0117,
        //              internal const uint SYSTIMER       = 0x0118;
        MENUSELECT = 0x011F,
        MENUCHAR = 0x0120,
        ENTERIDLE = 0x0121,
        MENURBUTTONUP = 0x0122,
        MENUDRAG = 0x0123,
        MENUGETOBJECT = 0x0124,
        UNINITMENUPOPUP = 0x0125,
        MENUCOMMAND = 0x0126,

        CHANGEUISTATE = 0x0127,
        UPDATEUISTATE = 0x0128,
        QUERYUISTATE = 0x0129,

        //              internal const uint LBTRACKPOINT     = 0x0131;
        CTLCOLORMSGBOX = 0x0132,
        CTLCOLOREDIT = 0x0133,
        CTLCOLORLISTBOX = 0x0134,
        CTLCOLORBTN = 0x0135,
        CTLCOLORDLG = 0x0136,
        CTLCOLORSCROLLBAR = 0x0137,
        CTLCOLORSTATIC = 0x0138,
        MOUSEMOVE = 0x0200,
        MOUSEFIRST = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        LBUTTONDBLCLK = 0x0203,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MBUTTONDBLCLK = 0x0209,
        MOUSEWHEEL = 0x020A,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONDOWN = 0x020B,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONUP = 0x020C,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONDBLCLK = 0x020D,
        /// <summary>
        /// Windows Vista and higher only.
        /// </summary>
        MOUSEHWHEEL = 0x020E,
        PARENTNOTIFY = 0x0210,
        ENTERMENULOOP = 0x0211,
        EXITMENULOOP = 0x0212,
        NEXTMENU = 0x0213,
        SIZING = 0x0214,
        CAPTURECHANGED = 0x0215,
        MOVING = 0x0216,
        //              internal const uint POWERBROADCAST   = 0x0218;
        DEVICECHANGE = 0x0219,
        MDICREATE = 0x0220,
        MDIDESTROY = 0x0221,
        MDIACTIVATE = 0x0222,
        MDIRESTORE = 0x0223,
        MDINEXT = 0x0224,
        MDIMAXIMIZE = 0x0225,
        MDITILE = 0x0226,
        MDICASCADE = 0x0227,
        MDIICONARRANGE = 0x0228,
        MDIGETACTIVE = 0x0229,
        /* D&D messages */
        //              internal const uint DROPOBJECT     = 0x022A;
        //              internal const uint QUERYDROPOBJECT  = 0x022B;
        //              internal const uint BEGINDRAG      = 0x022C;
        //              internal const uint DRAGLOOP       = 0x022D;
        //              internal const uint DRAGSELECT     = 0x022E;
        //              internal const uint DRAGMOVE       = 0x022F;
        MDISETMENU = 0x0230,
        ENTERSIZEMOVE = 0x0231,
        EXITSIZEMOVE = 0x0232,
        DROPFILES = 0x0233,
        MDIREFRESHMENU = 0x0234,
        IME_SETCONTEXT = 0x0281,
        IME_NOTIFY = 0x0282,
        IME_CONTROL = 0x0283,
        IME_COMPOSITIONFULL = 0x0284,
        IME_SELECT = 0x0285,
        IME_CHAR = 0x0286,
        IME_REQUEST = 0x0288,
        IME_KEYDOWN = 0x0290,
        IME_KEYUP = 0x0291,
        NCMOUSEHOVER = 0x02A0,
        MOUSEHOVER = 0x02A1,
        NCMOUSELEAVE = 0x02A2,
        MOUSELEAVE = 0x02A3,
        CUT = 0x0300,
        COPY = 0x0301,
        PASTE = 0x0302,
        CLEAR = 0x0303,
        UNDO = 0x0304,
        RENDERFORMAT = 0x0305,
        RENDERALLFORMATS = 0x0306,
        DESTROYCLIPBOARD = 0x0307,
        DRAWCLIPBOARD = 0x0308,
        PAINTCLIPBOARD = 0x0309,
        VSCROLLCLIPBOARD = 0x030A,
        SIZECLIPBOARD = 0x030B,
        ASKCBFORMATNAME = 0x030C,
        CHANGECBCHAIN = 0x030D,
        HSCROLLCLIPBOARD = 0x030E,
        QUERYNEWPALETTE = 0x030F,
        PALETTEISCHANGING = 0x0310,
        PALETTECHANGED = 0x0311,
        HOTKEY = 0x0312,
        PRINT = 0x0317,
        PRINTCLIENT = 0x0318,
        HANDHELDFIRST = 0x0358,
        HANDHELDLAST = 0x035F,
        AFXFIRST = 0x0360,
        AFXLAST = 0x037F,
        PENWINFIRST = 0x0380,
        PENWINLAST = 0x038F,
        APP = 0x8000,
        USER = 0x0400,

        // Our "private" ones
        MOUSE_ENTER = 0x0401,
        ASYNC_MESSAGE = 0x0403,
        REFLECT = USER + 0x1c00,
        CLOSE_INTERNAL = USER + 0x1c01,

        // NotifyIcon (Systray) Balloon messages
        BALLOONSHOW = USER + 0x0002,
        BALLOONHIDE = USER + 0x0003,
        BALLOONTIMEOUT = USER + 0x0004,
        BALLOONUSERCLICK = USER + 0x0005
    }

    [Flags]
    internal enum TrackMouseEventFlags : uint
    {
        HOVER = 0x00000001,
        LEAVE = 0x00000002,
        NONCLIENT = 0x00000010,
        QUERY = 0x40000000,
        CANCEL = 0x80000000,
    }

    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate IntPtr WindowProcedure(IntPtr handle, WindowMessage message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        internal int X;
        internal int Y;

        internal POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        internal Point ToPoint()
        {
            return new Point(X, Y);
        }

        public override string ToString()
        {
            return "Point {" + X.ToString() + ", " + Y.ToString() + ")";
        }
    }
}

#pragma warning restore 3019
#pragma warning restore 0649
#pragma warning restore 0169
#pragma warning restore 0414
