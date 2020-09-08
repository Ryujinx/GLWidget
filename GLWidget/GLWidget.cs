//https://github.com/Nihlus/GLWidgetTest/blob/master/GLWidget/GLWidgetGTK3/GLWidget.cs
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
using OpenTK.Graphics.OpenGL;


using Gtk;
using Cairo;
using GLib;
using Gdk;
using OpenGL;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace OpenTK
{
	[ToolboxItem(true)]
	public class GLWidget : DrawingArea
	{
        private static bool xThreadInit;

        /// <summary>
        /// Get or set the OpenGL minimum color buffer bits.
        /// </summary>
        [Property("color-bits")]
		public uint ColorBits
		{
			get { return (_ColorBits); }
			set { _ColorBits = value; }
		}

		/// <summary>
		/// The OpenGL color buffer bits.
		/// </summary>
		private uint _ColorBits = 24;

		/// <summary>
		/// Get or set the OpenGL minimum depth buffer bits.
		/// </summary>
		[Property("depth-bits")]
		public uint DepthBits
		{
			get { return (_DepthBits); }
			set { _DepthBits = value; }
		}

		/// <summary>
		/// The OpenGL color buffer bits.
		/// </summary>
		private uint _DepthBits;

		/// <summary>
		/// Get or set the OpenGL minimum stencil buffer bits.
		/// </summary>
		[Property("stencil-bits")]
		public uint StencilBits
		{
			get { return (_StencilBits); }
			set { _StencilBits = value; }
		}

		/// <summary>
		/// The OpenGL color buffer bits.
		/// </summary>
		private uint _StencilBits;

		/// <summary>
		/// Get or set the OpenGL minimum multisample buffer "bits".
		/// </summary>
		[Property("multisample-bits")]
		public uint MultisampleBits
		{
			get { return (_MultisampleBits); }
			set { _MultisampleBits = value; }
		}

		/// <summary>
		/// The OpenGL multisample buffer bits.
		/// </summary>
		private uint _MultisampleBits;

		/// <summary>
		/// Get or set the OpenGL swap buffers interval.
		/// </summary>
		[Property("swap-interval")]
		public int SwapInterval
		{
			get { return (_SwapInterval); }
			set { _SwapInterval = value; }
		}

		/// <summary>
		/// The OpenGL swap buffers interval.
		/// </summary>
		private int _SwapInterval = 1;

		/// <summary>
		/// The <see cref="DevicePixelFormat"/> describing the minimum pixel format required by this control.
		/// </summary>
		private DevicePixelFormat ControlPixelFormat
		{
			get
			{
				DevicePixelFormat controlReqFormat = new DevicePixelFormat();

				controlReqFormat.RgbaUnsigned = true;
				controlReqFormat.RenderWindow = true;

				controlReqFormat.ColorBits = (int)ColorBits;
				controlReqFormat.DepthBits = (int)DepthBits;
				controlReqFormat.StencilBits = (int)StencilBits;
				controlReqFormat.MultisampleBits = (int)MultisampleBits;
				controlReqFormat.DoubleBuffer = true;

				return (controlReqFormat);
			}
		}



		#region Static attrs.
        public bool HandleRendering { get; set; } = false;

        #endregion

        #region Attributes

        private bool _initialized;

		#endregion

		#region Properties

		/// <summary>The major version of OpenGL to use.</summary>
		public int GLVersionMajor { get; set; }

		/// <summary>The minor version of OpenGL to use.</summary>
		public int GLVersionMinor { get; set; }

        private DeviceContext _deviceContext;
        private IntPtr _graphicsContext;
        private int _error;

		private IGraphicsContext _context;
        private GLContext _gdkGlContext;

        public bool ForwardCompatible { get; }
        public DeviceContext DeviceContext { get => _deviceContext; set => _deviceContext = value; }

        #endregion

        #region Construction/Destruction

        /// <summary>Constructs a new GLWidget</summary>
        public GLWidget() : this(new Version(4, 0), true)
        {
			/*this.ColorBits = 32;
			this.DepthBits = 24;
			this.StencilBits = 8;*/
        }

        /// <summary>Constructs a new GLWidget</summary>
        public GLWidget(Version apiVersion, bool forwardCompatible)
		{
            GLVersionMajor = apiVersion.Major;
			GLVersionMinor = apiVersion.Minor;
            ForwardCompatible = forwardCompatible;
		}

		~GLWidget()
		{
			Dispose(false);
		}
		
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				OnShuttingDown();

				DeviceContext?.DeleteContext(_graphicsContext);

				DeviceContext?.Dispose();

                _gdkGlContext?.Dispose();
            }
		}

		#endregion

		#region New Events

		// Called when the first GraphicsContext is created in the case of GraphicsContext.ShareContexts == True;
		public static event EventHandler GraphicsContextInitialized;

        // Called when the first GraphicsContext is being destroyed in the case of GraphicsContext.ShareContexts == True;
        public static event EventHandler GraphicsContextShuttingDown;

        // Called when this GLWidget has a valid GraphicsContext
        public event EventHandler Initialized;

		protected virtual void OnInitialized()
		{
			if (Initialized != null)
			{
				Initialized(this, EventArgs.Empty);
			}
		}

		// Called when this GLWidget needs to render a frame
		public event EventHandler RenderFrame;

		protected virtual void OnRenderFrame()
		{
            RenderFrame?.Invoke(this, EventArgs.Empty);
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

		// Called when the widget needs to be (fully or partially) redrawn.

		protected override bool OnDrawn(Cairo.Context cr)
		{
			if (!_initialized)
				Initialize();

			ClearCurrent();

            return true;
		}

        public void Swapbuffers()
        {
            _context?.SwapBuffers();
        }

		public void MakeCurrent()
		{
			ClearCurrent();

			_context?.MakeCurrent();

			_error = Marshal.GetLastWin32Error();
		}

        public void ClearCurrent()
		{
            if (GTKBindingHelper.CurrentPlatform == OSPlatform.Windows)
            {
                DeviceContext?.MakeCurrent(IntPtr.Zero);
            }
            else
            {
            	Gdk.GLContext.ClearCurrent();
            }
        }

		private void CreateContext()
		{
			if (_graphicsContext != IntPtr.Zero)
				throw new InvalidOperationException("context already created");

			IntPtr sharingContext = IntPtr.Zero;

			if (Gl.PlatformExtensions.CreateContext_ARB)
			{
				List<int> attributes = new List<int>();
				uint contextProfile = 0, contextFlags = 0;
				bool debuggerAttached = Debugger.IsAttached;

				#region WGL_ARB_create_context|GLX_ARB_create_context

				#endregion

				#region WGL_ARB_create_context_profile|GLX_ARB_create_context_profile

				if (Gl.PlatformExtensions.CreateContextProfile_ARB)
				{

				}

				#endregion

				#region WGL_ARB_create_context_robustness|GLX_ARB_create_context_robustness

				if (Gl.PlatformExtensions.CreateContextRobustness_ARB)
				{

				}

				#endregion

				Debug.Assert(Wgl.CONTEXT_FLAGS_ARB == Glx.CONTEXT_FLAGS_ARB);
				if (contextFlags != 0)
					attributes.AddRange(new int[] { Wgl.CONTEXT_FLAGS_ARB, unchecked((int)contextFlags) });

				Debug.Assert(Wgl.CONTEXT_PROFILE_MASK_ARB == Glx.CONTEXT_PROFILE_MASK_ARB);
				Debug.Assert(contextProfile == 0 || Gl.PlatformExtensions.CreateContextProfile_ARB);
				if (contextProfile != 0)
					attributes.AddRange(new int[] { Wgl.CONTEXT_PROFILE_MASK_ARB, unchecked((int)contextProfile) });

				attributes.Add(0);

				if ((_graphicsContext = _deviceContext.CreateContextAttrib(sharingContext, attributes.ToArray())) == IntPtr.Zero)
					throw new InvalidOperationException(String.Format("unable to create render context ({0})", Gl.GetError()));
			}
			else
			{
				// Create OpenGL context using compatibility profile
				if ((_graphicsContext = _deviceContext.CreateContext(sharingContext)) == IntPtr.Zero)
					throw new InvalidOperationException("unable to create render context");
			}
		}

		private void CreateDeviceContext(DevicePixelFormat controlReqFormat)
		{
			#region Create device context

			DeviceContext = DeviceContext.Create(GTKBindingHelper.GetDisplayHandle(Display.Handle), GTKBindingHelper.GetWindowHandle(Window.Handle));
			DeviceContext.IncRef();

			#endregion

			#region Set pixel format

			DevicePixelFormatCollection pixelFormats = DeviceContext.PixelsFormats;
			List<DevicePixelFormat> matchingPixelFormats = pixelFormats.Choose(controlReqFormat);

			if ((matchingPixelFormats.Count == 0) && controlReqFormat.MultisampleBits > 0)
			{
				// Try to select the maximum multisample configuration
				int multisampleBits = 0;

				pixelFormats.ForEach(delegate (DevicePixelFormat item) { multisampleBits = Math.Max(multisampleBits, item.MultisampleBits); });

				controlReqFormat.MultisampleBits = multisampleBits;

				matchingPixelFormats = pixelFormats.Choose(controlReqFormat);
			}

			if ((matchingPixelFormats.Count == 0) && controlReqFormat.DoubleBuffer)
			{
				// Try single buffered pixel formats
				controlReqFormat.DoubleBuffer = false;

				matchingPixelFormats = pixelFormats.Choose(controlReqFormat);
				if (matchingPixelFormats.Count == 0)
					throw new InvalidOperationException(String.Format("unable to find a suitable pixel format: {0}", pixelFormats.GuessChooseError(controlReqFormat)));
			}
			else if (matchingPixelFormats.Count == 0)
				throw new InvalidOperationException(String.Format("unable to find a suitable pixel format: {0}", pixelFormats.GuessChooseError(controlReqFormat)));

			DeviceContext.SetPixelFormat(matchingPixelFormats[0]);

			#endregion

			#region Set V-Sync

			if (Gl.PlatformExtensions.SwapControl)
			{
				int swapInterval = SwapInterval;

				// Mask value in case it is not supported
				if (!Gl.PlatformExtensions.SwapControlTear && swapInterval == -1)
					swapInterval = 1;

				DeviceContext.SwapInterval(swapInterval);
			}

			#endregion
		}

        private void CreateGdkGlContext()
        {
            _gdkGlContext = Window.CreateGlContext();

            _gdkGlContext.SetRequiredVersion(GLVersionMajor, GLVersionMinor);

            _gdkGlContext.ForwardCompatible = ForwardCompatible;

            _gdkGlContext.SetUseEs(0);

            _gdkGlContext.Realize();

            _gdkGlContext.MakeCurrent();
        }

        private void Initialize()
		{
			ClearCurrent();

            Khronos.KhronosApi.LogEnabled = true;

			Window.EnsureNative();

            if (GTKBindingHelper.CurrentPlatform == OSPlatform.Windows)
            {
                CreateDeviceContext(ControlPixelFormat);

                CreateContext();

                DeviceContext.MakeCurrent(_graphicsContext);
            }
			else {
                GraphicsContext.Display = Display.Handle;

                CreateGdkGlContext();
            }

			_context = GraphicsContext.GetCurrentContext(Window.Handle);

			MakeCurrent();

			GTKBindingHelper.InitializeGlBindings();

            OnInitialized();

			OpenTK.GraphicsContext.GetCurrentContext(Window.Handle).SwapInterval(1);

			ClearCurrent();

			_initialized = true;

		}
	}
}