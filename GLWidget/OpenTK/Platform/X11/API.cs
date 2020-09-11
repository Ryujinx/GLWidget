/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * Contributions from Erik Ylvisaker
 * See license.txt for license info
 */

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 1591    // Missing XML comments
#pragma warning disable 3019    // CLS-compliance checking
#pragma warning disable 0649    // struct members not explicitly initialized
#pragma warning disable 0169    // field / method is never used.
#pragma warning disable 0414    // field assigned but never used.

namespace OpenTK.Platform.X11
{
    // using XID = System.Int32;
    using Window = System.IntPtr;
    using Drawable = System.IntPtr;
    using Font = System.IntPtr;
    using Pixmap = System.IntPtr;
    using Cursor = System.IntPtr;
    using Colormap = System.IntPtr;
    using GContext = System.IntPtr;
    using KeySym = System.IntPtr;
    using Mask = System.IntPtr;
    using Atom = System.IntPtr;
    using VisualID = System.IntPtr;
    using Time = System.IntPtr;
    using KeyCode = System.Byte;    // Or maybe ushort?

    using Display = System.IntPtr;
    using XPointer = System.IntPtr;

    using XcursorBool = System.Int32;
    using XcursorUInt = System.UInt32;
    using XcursorDim = System.UInt32;
    using XcursorPixel = System.UInt32;

    // Randr and Xrandr
    using Bool = System.Boolean;
    using XRRScreenConfiguration = System.IntPtr; // opaque datatype
    using Rotation = System.UInt16;
    using Status = System.Int32;
    using SizeID = System.UInt16;

    internal static class API
    {
        private const string _dll_name = "libX11";
        private const string _dll_name_vid = "libXxf86vm";

        internal static Display DefaultDisplay { get; private set; }

        //internal static Window RootWindow { get { return rootWindow; } }
        internal static int ScreenCount { get; }

        internal static object Lock = new object();

        static API()
        {
            int has_threaded_x = Functions.XInitThreads();
            Debug.Print("Initializing threaded X11: {0}.", has_threaded_x.ToString());

            DefaultDisplay = Functions.XOpenDisplay(IntPtr.Zero);

            if (DefaultDisplay == IntPtr.Zero)
            {
                throw new PlatformException("Could not establish connection to the X-Server.");
            }

            using (new XLock(DefaultDisplay))
            {
                ScreenCount = Functions.XScreenCount(DefaultDisplay);
            }
            Debug.Print("Display connection: {0}, Screen count: {1}", DefaultDisplay, ScreenCount);

            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XF86VidModeModeLine
        {
            public short hdisplay;   /* Number of display pixels horizontally */
            public short hsyncstart; /* Horizontal sync start */
            public short hsyncend;   /* Horizontal sync end */
            public short htotal;     /* Total horizontal pixels */
            public short vdisplay;   /* Number of display pixels vertically */
            public short vsyncstart; /* Vertical sync start */
            public short vsyncend;   /* Vertical sync start */
            public short vtotal;     /* Total vertical pixels */
            public int flags;      /* Mode flags */
            public int privsize;   /* Size of private */
            public IntPtr _private;   /* Server privates */
        }

        /// <summary>
        /// Specifies an XF86 display mode.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct XF86VidModeModeInfo
        {
            /// <summary>
            /// Pixel clock.
            /// </summary>
            public int dotclock;

            /// <summary>
            /// Number of display pixels horizontally
            /// </summary>
            public short hdisplay;

            /// <summary>
            /// Horizontal sync start
            /// </summary>
            public short hsyncstart;

            /// <summary>
            /// Horizontal sync end
            /// </summary>
            public short hsyncend;

            /// <summary>
            /// Total horizontal pixel
            /// </summary>
            public short htotal;

            /// <summary>
            ///
            /// </summary>
            public short hskew;

            /// <summary>
            /// Number of display pixels vertically
            /// </summary>
            public short vdisplay;

            /// <summary>
            /// Vertical sync start
            /// </summary>
            public short vsyncstart;

            /// <summary>
            /// Vertical sync end
            /// </summary>
            public short vsyncend;

            /// <summary>
            /// Total vertical pixels
            /// </summary>
            public short vtotal;

            /// <summary>
            ///
            /// </summary>
            public short vskew;

            /// <summary>
            /// Mode flags
            /// </summary>
            public int flags;

            private int privsize;   /* Size of private */
            private IntPtr _private;   /* Server privates */
        }

        [DllImport(_dll_name_vid)]
        extern public static bool XF86VidModeQueryVersion(
            Display display,
            out int major_version_return,
            out int minor_version_return);

        [DllImport(_dll_name_vid)]
        extern public static bool XF86VidModeGetModeLine(
            Display display,
            int screen,
            out int dotclock_return,
            out XF86VidModeModeLine modeline);

        [DllImport(_dll_name_vid)]
        extern public static bool XF86VidModeGetAllModeLines(
            Display display,
            int screen,
            out int modecount_return,
            /*XF86VidModeModeInfo***  <-- yes, that's three *'s. */
            out IntPtr modesinfo);

        [DllImport(_dll_name_vid)]
        extern public static bool XF86VidModeGetViewPort(
            Display display,
            int screen,
            out int x_return,
            out int y_return);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XVisualInfo
    {
        public IntPtr Visual;
        public VisualID VisualID;
        public int Screen;
        public int Depth;
        public XVisualClass Class;
        public long RedMask;
        public long GreenMask;
        public long blueMask;
        public int ColormapSize;
        public int BitsPerRgb;

        public override string ToString()
        {
            return String.Format("id ({0}), screen ({1}), depth ({2}), class ({3})",
                VisualID, Screen, Depth, Class);
        }
    }

    internal struct XRRScreenSize
    {
        internal int Width, Height;
        internal int MWidth, MHeight;
    };


    [StructLayout(LayoutKind.Sequential)]
    internal struct MotifWmHints
    {
        internal IntPtr flags;
        internal IntPtr functions;
        internal IntPtr decorations;
        internal IntPtr input_mode;
        internal IntPtr status;

        public override string ToString ()
        {
            return string.Format("MotifWmHints <flags={0}, functions={1}, decorations={2}, input_mode={3}, status={4}", (MotifFlags)flags.ToInt32 (), (MotifFunctions)functions.ToInt32 (), (MotifDecorations)decorations.ToInt32 (), (MotifInputMode)input_mode.ToInt32 (), status.ToInt32 ());
        }
    }

    [Flags]
    internal enum MotifFlags
    {
        Functions    = 1,
        Decorations  = 2,
        InputMode    = 4,
        Status       = 8
    }

    [Flags]
    internal enum MotifFunctions
    {
        All         = 0x01,
        Resize      = 0x02,
        Move        = 0x04,
        Minimize    = 0x08,
        Maximize    = 0x10,
        Close       = 0x20
    }

    [Flags]
    internal enum MotifDecorations
    {
        All         = 0x01,
        Border      = 0x02,
        ResizeH     = 0x04,
        Title       = 0x08,
        Menu        = 0x10,
        Minimize    = 0x20,
        Maximize    = 0x40,

    }

    [Flags]
    internal enum MotifInputMode
    {
        Modeless                = 0,
        ApplicationModal        = 1,
        SystemModal             = 2,
        FullApplicationModal    = 3
    }

    internal enum ErrorCodes : int
    {
        Success = 0,
        BadRequest = 1,
        BadValue = 2,
        BadWindow = 3,
        BadPixmap = 4,
        BadAtom = 5,
        BadCursor = 6,
        BadFont = 7,
        BadMatch = 8,
        BadDrawable = 9,
        BadAccess = 10,
        BadAlloc = 11,
        BadColor = 12,
        BadGC = 13,
        BadIDChoice = 14,
        BadName = 15,
        BadLength = 16,
        BadImplementation = 17,
    }

    public enum XVisualClass : int
    {
        StaticGray = 0,
        GrayScale = 1,
        StaticColor = 2,
        PseudoColor = 3,
        TrueColor = 4,
        DirectColor = 5,
    }

    [Flags]
    public enum XVisualInfoMask
    {
        No = 0x0,
        ID = 0x1,
        Screen = 0x2,
        Depth = 0x4,
        Class = 0x8,
        Red = 0x10,
        Green = 0x20,
        Blue = 0x40,
        ColormapSize = 0x80,
        BitsPerRGB = 0x100,
        All = 0x1FF,
    }
    
    internal static partial class Functions
    {
        internal const string X11Library = "libX11";

        private const string XrandrLibrary = "libXrandr.so.2";

        [DllImport(XrandrLibrary)]
        public static extern XRRScreenConfiguration XRRGetScreenInfo(Display dpy, Drawable draw);

        [DllImport(XrandrLibrary)]
        public static extern void XRRFreeScreenConfigInfo(XRRScreenConfiguration config);

        [DllImport(XrandrLibrary)]
        public static extern Status XRRSetScreenConfig(Display dpy, XRRScreenConfiguration config,
            Drawable draw, int size_index, Rotation rotation, Time timestamp);

        [DllImport(XrandrLibrary)]
        public static extern SizeID XRRConfigCurrentConfiguration(XRRScreenConfiguration config, out Rotation rotation);

        [DllImport(XrandrLibrary)]
        public static extern short XRRConfigCurrentRate(XRRScreenConfiguration config);

        [DllImport(XrandrLibrary)]
        private unsafe static extern IntPtr XRRSizes(Display dpy, int screen, int* nsizes);

        public static XRRScreenSize[] XRRSizes(Display dpy, int screen)
        {
            XRRScreenSize[] sizes;
            //IntPtr ptr;
            int count;
            unsafe
            {
                //ptr = XRRSizes(dpy, screen, &nsizes);

                byte* data = (byte*)XRRSizes(dpy, screen, &count); //(byte*)ptr;
                if (count == 0)
                {
                    return null;
                }
                sizes = new XRRScreenSize[count];
                for (int i = 0; i < count; i++)
                {
                    sizes[i] = new XRRScreenSize();
                    sizes[i] = (XRRScreenSize)Marshal.PtrToStructure((IntPtr)data, typeof(XRRScreenSize));
                    data += Marshal.SizeOf(typeof(XRRScreenSize));
                }
                //XFree(ptr);   // Looks like we must not free this.
                return sizes;
            }
        }

        [DllImport(XrandrLibrary)]
        private unsafe static extern short* XRRRates(Display dpy, int screen, int size_index, int* nrates);

        public static short[] XRRRates(Display dpy, int screen, int size_index)
        {
            short[] rates;
            int count;
            unsafe
            {
                short* data = (short*)XRRRates(dpy, screen, size_index, &count);
                if (count == 0)
                {
                    return null;
                }
                rates = new short[count];
                for (int i = 0; i < count; i++)
                {
                    rates[i] = *(data + i);
                }
            }
            return rates;
        }

        [DllImport(XrandrLibrary)]
        public static extern Time XRRTimes(Display dpy, int screen, out Time config_timestamp);

        [DllImport(X11Library)]
        public static extern int XScreenCount(Display display);

        [DllImport(X11Library)]
        private unsafe static extern int *XListDepths(Display display, int screen_number, int* count_return);

        public static int[] XListDepths(Display display, int screen_number)
        {
            unsafe
            {
                int count;
                int* data = XListDepths(display, screen_number, &count);
                if (count == 0)
                {
                    return null;
                }
                int[] depths = new int[count];
                for (int i = 0; i < count; i++)
                {
                    depths[i] = *(data + i);
                }

                return depths;
            }
        }
    }

    // Helper structure for calling XLock/UnlockDisplay
    internal struct XLock : IDisposable
    {
        private IntPtr _display;

        public IntPtr Display
        {
            get
            {
                if (_display == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Internal error (XLockDisplay with IntPtr.Zero). Please report this at https://github.com/opentk/opentk/issues");
                }
                return _display;
            }
            set
            {
                if (value == IntPtr.Zero)
                {
                    throw new ArgumentException();
                }
                _display = value;
            }
        }

        public XLock(IntPtr display)
            : this()
        {
            Display = display;
            Functions.XLockDisplay(Display);
        }

        public void Dispose()
        {
            Functions.XUnlockDisplay(Display);
        }
    }

    // XAllowEvent modes
    internal enum EventMode
    {
        AsyncPointer = 0,
        SyncPointer,
        ReplayPointer,
        AsyncKeyboard,
        SyncKeyboard,
        ReplayKeyboard,
        AsyncBoth,
        SyncBoth
    }
}