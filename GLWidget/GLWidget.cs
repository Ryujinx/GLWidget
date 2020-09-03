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

namespace OpenTK
{
	[ToolboxItem(true)]
	public class GLWidget : DrawingArea
	{

		#region Static attrs.

		private static int _graphicsContextCount;
		private static bool _sharedContextInitialized;

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

        private int _framebuffer;
        private int _renderbuffer;
        private int _stencilbuffer;
        private bool _recreateFramebuffer;

        public Gdk.GLContext GraphicsContext { get; set; }
        public bool ForwardCompatible { get; }

        #endregion

        #region Construction/Destruction

        /// <summary>Constructs a new GLWidget</summary>
        public GLWidget() : this(new Version(4, 0), true)
        {

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
				
				GraphicsContext.Dispose();
			}
		}

		#endregion

		#region New Events

		// Called when the first GraphicsContext is created in the case of GraphicsContext.ShareContexts == True;
		public static event EventHandler GraphicsContextInitialized;

		private static void OnGraphicsContextInitialized()
		{
			if (GraphicsContextInitialized != null)
			{
				GraphicsContextInitialized(null, EventArgs.Empty);
			}
		}

		// Called when the first GraphicsContext is being destroyed in the case of GraphicsContext.ShareContexts == True;
		public static event EventHandler GraphicsContextShuttingDown;

		private static void OnGraphicsContextShuttingDown()
		{
			if (GraphicsContextShuttingDown != null)
			{
				GraphicsContextShuttingDown(null, EventArgs.Empty);
			}
		}

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
				System.Threading.Tasks.Task.Run(Initialize).Wait();
			
			if (HandleRendering)
            {
                MakeCurrent();

				OnRenderFrame();
            }

            cr.SetSourceColor(new Color(0, 0, 0, 1));
            cr.Paint();

            var scale = this.ScaleFactor;

            Gdk.CairoHelper.DrawFromGl(cr, this.Window, _renderbuffer, (int)ObjectLabelIdentifier.Renderbuffer, scale, 0, 0, AllocatedWidth, AllocatedHeight);

            return true;
		}

        public void Swapbuffers()
        {
            GL.Flush();

            QueueDraw();

			RecreateFramebuffer();

            if (HandleRendering)
            {
				ClearCurrent();
            }
        }

        public void MakeCurrent()
        {
            GraphicsContext?.MakeCurrent();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        }

        public void ClearCurrent()
        {
            Gdk.GLContext.ClearCurrent();
        }

        // Called on Resize
        protected override bool OnConfigureEvent(Gdk.EventConfigure evnt)
		{
			if (GraphicsContext != null)
			{
				_recreateFramebuffer = true;               
            }

			return true;
		}

		public void RecreateFramebuffer()
        {
            if (!_recreateFramebuffer)
            {
				return;
            }

			_recreateFramebuffer = false;

			GraphicsContext.Window.Resize(AllocatedWidth, AllocatedHeight);

			DeleteBuffers();

			CreateFramebuffer();
		}

        private void DeleteBuffers()
        {
            if (_framebuffer != 0)
            {
                GL.DeleteFramebuffer(_framebuffer);
                GL.DeleteRenderbuffer(_renderbuffer);
            }
        }

        private void CreateFramebuffer()
        {
            _framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

            _renderbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _renderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, AllocatedWidth, AllocatedHeight);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _renderbuffer);

            _stencilbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _stencilbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, AllocatedWidth, AllocatedHeight);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _stencilbuffer);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            var state = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        }

        private void Initialize()
		{
			_initialized = true;

            GraphicsContext = Window.CreateGlContext();

            GraphicsContext.SetRequiredVersion(GLVersionMajor, GLVersionMinor);

            GraphicsContext.SetUseEs(0);

            GraphicsContext.ForwardCompatible = ForwardCompatible;

            GraphicsContext.Realize();

            GraphicsContext.MakeCurrent();

            if (!GTKBindingContext.Loaded)
            {
				GTKBindingContext.InitializeGlBindings();
            }

            CreateFramebuffer();

            OnInitialized();

			ClearCurrent();
		}
	}
}