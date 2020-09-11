﻿//https://github.com/Nihlus/GLWidgetTest/blob/master/GLWidget/GLWidgetGTK3/GLWidget.cs
#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library, except where noted.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.ComponentModel;


using OpenTK.Graphics;
using OpenTK.Platform;

using Gtk;

namespace OpenTK
{
    [ToolboxItem(true)]
    public class GLWidget : DrawingArea
    {

        #region Static attrs.

        private static int _graphicsContextCount;
        private static bool _sharedContextInitialized;

        public bool IsRenderHandler { get; set; } = false;

        #endregion

        #region Attributes

        private IGraphicsContext _graphicsContext;
        private IWindowInfo _windowInfo;
        private bool _initialized;
        private int swapInterval = 0;

        #endregion

        #region Properties

        /// <summary>Use a single buffer versus a double buffer.</summary>
        [Browsable(true)]
        public bool SingleBuffer { get; set; }

        /// <summary>Color Buffer Bits-Per-Pixel</summary>
        public int ColorBPP { get; set; }

        /// <summary>Accumulation Buffer Bits-Per-Pixel</summary>
        public int AccumulatorBPP { get; set; }

        /// <summary>Depth Buffer Bits-Per-Pixel</summary>
        public int DepthBPP { get; set; }

        /// <summary>Stencil Buffer Bits-Per-Pixel</summary>
        public int StencilBPP { get; set; }

        /// <summary>Number of samples</summary>
        public int Samples { get; set; }

        /// <summary>Indicates if steropic renderering is enabled</summary>
        public bool Stereo { get; set; }

        /// <summary>The major version of OpenGL to use.</summary>
        public int GLVersionMajor { get; set; }

        /// <summary>The minor version of OpenGL to use.</summary>
        public int GLVersionMinor { get; set; }

        public int SwapInterval
        {
            get => swapInterval; set
            {
                swapInterval = value;

                if (GraphicsContext != null)
                {
                    GraphicsContext.SwapInterval = value;
                }
            }
        }
        public GraphicsContextFlags GraphicsContextFlags
        {
            get;
            set;
        }

        #endregion

        #region Construction/Destruction

        /// <summary>Constructs a new GLWidget.</summary>
        public GLWidget()
            : this(GraphicsMode.Default)
        {
        }

        /// <summary>Constructs a new GLWidget using a given GraphicsMode</summary>
        public GLWidget(GraphicsMode graphicsMode)
            : this(graphicsMode, 1, 0, GraphicsContextFlags.Default)
        {
        }

        /// <summary>Constructs a new GLWidget</summary>
        public GLWidget(GraphicsMode graphicsMode, int glVersionMajor, int glVersionMinor, GraphicsContextFlags graphicsContextFlags)
        {
            SingleBuffer = graphicsMode.Buffers == 1;
            ColorBPP = graphicsMode.ColorFormat.BitsPerPixel;
            AccumulatorBPP = graphicsMode.AccumulatorFormat.BitsPerPixel;
            DepthBPP = graphicsMode.Depth;
            StencilBPP = graphicsMode.Stencil;
            Samples = graphicsMode.Samples;
            Stereo = graphicsMode.Stereo;

            GLVersionMajor = glVersionMajor;
            GLVersionMinor = glVersionMinor;
            GraphicsContextFlags = graphicsContextFlags;
        }

        ~GLWidget()
        {
            Dispose(false);
        }

        public void MakeCurrent()
        {
            GraphicsContext.MakeCurrent(_windowInfo);
        }

        public void ClearCurrent()
        {
            Gdk.GLContext.ClearCurrent();
            GraphicsContext.MakeCurrent(null);
        }

        public void Swapbuffers()
        {
            GraphicsContext.SwapBuffers();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    MakeCurrent();
                }
                catch (Exception ex)
                {

                }

                OnShuttingDown();

                if (OpenTK.Graphics.GraphicsContext.ShareContexts && (Interlocked.Decrement(ref _graphicsContextCount) == 0))
                {
                    OnGraphicsContextShuttingDown();
                    _sharedContextInitialized = false;
                }

                GraphicsContext.Dispose();
            }
        }

        #endregion

        #region New Events

        // Called when the first GraphicsContext is created in the case of GraphicsContext.ShareContexts == True;
        public static event EventHandler GraphicsContextInitialized;

        private static void OnGraphicsContextInitialized()
        {
            GraphicsContextInitialized?.Invoke(null, EventArgs.Empty);
        }

        // Called when the first GraphicsContext is being destroyed in the case of GraphicsContext.ShareContexts == True;
        public static event EventHandler GraphicsContextShuttingDown;

        private static void OnGraphicsContextShuttingDown()
        {
            GraphicsContextShuttingDown?.Invoke(null, EventArgs.Empty);
        }

        // Called when this GLWidget has a valid GraphicsContext
        public event EventHandler Initialized;

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        // Called when this GLWidget needs to render a frame
        public event EventHandler RenderFrame;

        protected virtual void OnRenderFrame()
        {
            if (RenderFrame != null)
            {
                RenderFrame(this, EventArgs.Empty);
            }
        }

        // Called when this GLWidget is being Disposed
        public event EventHandler ShuttingDown;

        protected virtual void OnShuttingDown()
        {
            if (ShuttingDown != null)
            {
                ShuttingDown(this, EventArgs.Empty);
            }
        }

        #endregion

        // Called when a widget is realized. (window handles and such are valid)
        // protected override void OnRealized() { base.OnRealized(); }

        // Called when the widget needs to be (fully or partially) redrawn.

        protected override bool OnDrawn(Cairo.Context cr)
        {
            if (!_initialized)
                Initialize();
            else if (!IsRenderHandler)
                MakeCurrent();

            return true;
        }

        // Called on Resize
        protected override bool OnConfigureEvent(Gdk.EventConfigure evnt)
        {
            if (GraphicsContext != null)
            {
                GraphicsContext.Update(WindowInfo);
            }

            return true;
        }

        private void Initialize()
        {
            Toolkit.Init();

            _initialized = true;

            Gdk.GLContext.ClearCurrent();

            // If this looks uninitialized...  initialize.
            if (ColorBPP == 0)
            {
                ColorBPP = 32;

                if (DepthBPP == 0)
                {
                    DepthBPP = 16;
                }
            }

            ColorFormat colorBufferColorFormat = new ColorFormat(ColorBPP);

            ColorFormat accumulationColorFormat = new ColorFormat(AccumulatorBPP);

            int buffers = 2;
            if (SingleBuffer)
            {
                buffers--;
            }

            GraphicsMode graphicsMode = GraphicsMode.Default;//new GraphicsMode(colorBufferColorFormat, DepthBPP, StencilBPP, Samples, accumulationColorFormat, buffers, Stereo);

            this.Window.EnsureNative();

            // IWindowInfo
            if (OpenTK.Configuration.RunningOnWindows)
            {
                WindowInfo = InitializeWindows();
            }
            else if (OpenTK.Configuration.RunningOnMacOS)
            {
                WindowInfo = InitializeOSX();
            }
            else
            {
                WindowInfo = InitializeX(graphicsMode);
            }

            // GraphicsContext
            GraphicsContext = new GraphicsContext(graphicsMode, WindowInfo, GLVersionMajor, GLVersionMinor, GraphicsContextFlags);
            MakeCurrent();

            if (OpenTK.Graphics.GraphicsContext.ShareContexts)
            {
                Interlocked.Increment(ref _graphicsContextCount);

                if (!_sharedContextInitialized)
                {
                    _sharedContextInitialized = true;
                    ((IGraphicsContextInternal)GraphicsContext).LoadAll();
                    OnGraphicsContextInitialized();
                }
            }
            else
            {
                ((IGraphicsContextInternal)GraphicsContext).LoadAll();
                OnGraphicsContextInitialized();
            }

            GraphicsContext.SwapInterval = SwapInterval;

            var swap = GraphicsContext.SwapInterval;

            GTKBindingHelper.InitializeGlBindings();

            ClearCurrent();
            MakeCurrent();

            OnInitialized();
        }

        #region Windows Specific initalization

        IWindowInfo InitializeWindows()
        {
            IntPtr windowHandle = gdk_win32_window_get_handle(this.Window.Handle);
            return Utilities.CreateWindowsWindowInfo(windowHandle);
        }

        [SuppressUnmanagedCodeSecurity, DllImport("libgdk-3-0.dll")]
        public static extern IntPtr gdk_win32_window_get_handle(IntPtr d);

        #endregion

        #region OSX Specific Initialization

        IWindowInfo InitializeOSX()
        {
            IntPtr windowHandle = gdk_quartz_window_get_nswindow(this.Window.Handle);
            //IntPtr viewHandle = gdk_quartz_window_get_nsview(this.GdkWindow.Handle);
            return Utilities.CreateMacOSWindowInfo(windowHandle);
        }

        [SuppressUnmanagedCodeSecurity, DllImport("libgdk-3.0.dylib")]
        static extern IntPtr gdk_quartz_window_get_nswindow(IntPtr handle);

        [SuppressUnmanagedCodeSecurity, DllImport("libgdk-3.0.dylib")]
        static extern IntPtr gdk_quartz_window_get_nsview(IntPtr handle);

        #endregion

        #region X Specific Initialization

        const string UnixLibGdkName = "libgdk-3.so.0";

        const string UnixLibX11Name = "libX11.so.6";
        const string UnixLibGLName = "libGL.so.1";

        const int GLX_NONE = 0;
        const int GLX_USE_GL = 1;
        const int GLX_BUFFER_SIZE = 2;
        const int GLX_LEVEL = 3;
        const int GLX_RGBA = 4;
        const int GLX_DOUBLEBUFFER = 5;
        const int GLX_STEREO = 6;
        const int GLX_AUX_BUFFERS = 7;
        const int GLX_RED_SIZE = 8;
        const int GLX_GREEN_SIZE = 9;
        const int GLX_BLUE_SIZE = 10;
        const int GLX_ALPHA_SIZE = 11;
        const int GLX_DEPTH_SIZE = 12;
        const int GLX_STENCIL_SIZE = 13;
        const int GLX_ACCUM_RED_SIZE = 14;
        const int GLX_ACCUM_GREEN_SIZE = 15;
        const int GLX_ACCUM_BLUE_SIZE = 16;
        const int GLX_ACCUM_ALPHA_SIZE = 17;

        public enum XVisualClass
        {
            StaticGray = 0,
            GrayScale = 1,
            StaticColor = 2,
            PseudoColor = 3,
            TrueColor = 4,
            DirectColor = 5,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct XVisualInfo
        {
            public IntPtr Visual;
            public IntPtr VisualID;
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
                return $"id ({VisualID}), screen ({Screen}), depth ({Depth}), class ({Class})";
            }
        }

        [Flags]
        internal enum XVisualInfoMask
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

        private IWindowInfo InitializeX(GraphicsMode mode)
        {
            IntPtr display = gdk_x11_display_get_xdisplay(Display.Handle);
            int screen = Screen.Number;

            IntPtr windowHandle = gdk_x11_window_get_xid(Window.Handle);
            IntPtr rootWindow = gdk_x11_window_get_xid(RootWindow.Handle);

            IntPtr visualInfo;

            if (mode.Index.HasValue)
            {
                XVisualInfo info = new XVisualInfo();
                info.VisualID = mode.Index.Value;
                int dummy;
                visualInfo = XGetVisualInfo(display, XVisualInfoMask.ID, ref info, out dummy);
            }
            else
            {
                visualInfo = GetVisualInfo(display);
            }

            IWindowInfo retval = Utilities.CreateX11WindowInfo(display, screen, windowHandle, rootWindow, visualInfo);
            XFree(visualInfo);

            return retval;
        }

        private static IntPtr XGetVisualInfo(IntPtr display, XVisualInfoMask vinfo_mask, ref XVisualInfo template, out int nitems)
        {
            return XGetVisualInfoInternal(display, (IntPtr)(int)vinfo_mask, ref template, out nitems);
        }

        private IntPtr GetVisualInfo(IntPtr display)
        {
            try
            {
                int[] attributes = AttributeList.ToArray();
                return glXChooseVisual(display, Screen.Number, attributes);
            }
            catch (DllNotFoundException e)
            {
                throw new DllNotFoundException("OpenGL dll not found!", e);
            }
            catch (EntryPointNotFoundException enf)
            {
                throw new EntryPointNotFoundException("Glx entry point not found!", enf);
            }
        }

        private List<int> AttributeList
        {
            get
            {
                List<int> attributeList = new List<int>(24);

                attributeList.Add(GLX_RGBA);

                if (!SingleBuffer)
                    attributeList.Add(GLX_DOUBLEBUFFER);

                if (Stereo)
                    attributeList.Add(GLX_STEREO);

                attributeList.Add(GLX_RED_SIZE);
                attributeList.Add(ColorBPP / 4); // TODO support 16-bit

                attributeList.Add(GLX_GREEN_SIZE);
                attributeList.Add(ColorBPP / 4); // TODO support 16-bit

                attributeList.Add(GLX_BLUE_SIZE);
                attributeList.Add(ColorBPP / 4); // TODO support 16-bit

                attributeList.Add(GLX_ALPHA_SIZE);
                attributeList.Add(ColorBPP / 4); // TODO support 16-bit

                attributeList.Add(GLX_DEPTH_SIZE);
                attributeList.Add(DepthBPP);

                attributeList.Add(GLX_STENCIL_SIZE);
                attributeList.Add(StencilBPP);

                attributeList.Add(GLX_ACCUM_RED_SIZE);
                attributeList.Add(AccumulatorBPP / 4);// TODO support 16-bit

                attributeList.Add(GLX_ACCUM_GREEN_SIZE);
                attributeList.Add(AccumulatorBPP / 4);// TODO support 16-bit

                attributeList.Add(GLX_ACCUM_BLUE_SIZE);
                attributeList.Add(AccumulatorBPP / 4);// TODO support 16-bit

                attributeList.Add(GLX_ACCUM_ALPHA_SIZE);
                attributeList.Add(AccumulatorBPP / 4);// TODO support 16-bit

                attributeList.Add(GLX_NONE);

                return attributeList;
            }
        }

        public IGraphicsContext GraphicsContext { get => _graphicsContext; set => _graphicsContext = value; }
        public IWindowInfo WindowInfo { get => _windowInfo; set => _windowInfo = value; }

        [DllImport(UnixLibX11Name, EntryPoint = "XGetVisualInfo")]
        private static extern IntPtr XGetVisualInfoInternal(IntPtr display, IntPtr vinfo_mask, ref XVisualInfo template, out int nitems);

        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibX11Name)]
        private static extern void XFree(IntPtr handle);

        /// <summary> Returns the X resource (window or pixmap) belonging to a GdkDrawable. </summary>
        /// <remarks> XID gdk_x11_drawable_get_xid(GdkDrawable *drawable); </remarks>
        /// <param name="gdkDisplay"> The GdkDrawable. </param>
        /// <returns> The ID of drawable's X resource. </returns>
        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGdkName)]
        private static extern IntPtr gdk_x11_drawable_get_xid(IntPtr gdkDisplay);

        /// <summary> Returns the X resource (window or pixmap) belonging to a GdkDrawable. </summary>
        /// <remarks> XID gdk_x11_drawable_get_xid(GdkDrawable *drawable); </remarks>
        /// <param name="gdkDisplay"> The GdkDrawable. </param>
        /// <returns> The ID of drawable's X resource. </returns>
        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGdkName)]
        private static extern IntPtr gdk_x11_window_get_xid(IntPtr gdkDisplay);

        /// <summary> Returns the X display of a GdkDisplay. </summary>
        /// <remarks> Display* gdk_x11_display_get_xdisplay(GdkDisplay *display); </remarks>
        /// <param name="gdkDisplay"> The GdkDrawable. </param>
        /// <returns> The X Display of the GdkDisplay. </returns>
        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGdkName)]
        private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr gdkDisplay);

        [SuppressUnmanagedCodeSecurity, DllImport(UnixLibGLName)]
        private static extern IntPtr glXChooseVisual(IntPtr display, int screen, int[] attr);

        #endregion
    }
}