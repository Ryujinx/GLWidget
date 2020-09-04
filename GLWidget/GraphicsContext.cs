using System;
using System.Runtime.InteropServices;
using static OpenTK.GTKBindingHelper;

namespace OpenTK
{
    public interface IGraphicsContext
    {
        void MakeCurrent();
        void SwapBuffers();

        void ClearCurrent();
    }

    public abstract class GraphicsContext : IGraphicsContext
    {
        public static IntPtr Display{ get; set; }

        public abstract void MakeCurrent();

        public abstract void SwapBuffers();

        public static IGraphicsContext GetCurrentContext(IntPtr handle)
        {
            var currentPlatform = CurrentPlatform;

            if(currentPlatform == OSPlatform.Windows){
                return WglGraphicsContext.GetCurrent(handle);
            }
            else if(currentPlatform == OSPlatform.Linux){
                if (Display == null || Display == IntPtr.Zero)
                {
                    throw new InvalidOperationException("No Display set");
                }
                return GlxGraphicsContext.GetCurrent(handle, Display);
            }

            return null;
        }

        public abstract void ClearCurrent();
    }

    public class WglGraphicsContext : GraphicsContext
    {
        private IntPtr _windowHandle;
        private IntPtr _deviceContext;

        public WglGraphicsContext(IntPtr deviceContext, IntPtr graphicsContext, IntPtr windowHandle = default)
        {
            _deviceContext = deviceContext;
            _graphicsContext = graphicsContext;
            _windowHandle = windowHandle;
        }

        private IntPtr _graphicsContext;
        
        public static WglGraphicsContext GetCurrent(IntPtr windowHandle)
        {
            IntPtr dc = UnsafeNativeMethods.wglGetCurrentDC();
            IntPtr gc = UnsafeNativeMethods.wglGetCurrentContext();

            return new WglGraphicsContext(dc, gc, windowHandle);
        }

        public override void MakeCurrent()
        {
            UnsafeNativeMethods.WglMakeCurrent(_deviceContext, _graphicsContext);
        }

        public override void SwapBuffers()
        {
            UnsafeNativeMethods.wglSwapBuffers(_deviceContext);
        }

        public override void ClearCurrent()
        {
            throw new NotImplementedException();
        }
    }

    public class GlxGraphicsContext : GraphicsContext
    {
        private IntPtr _windowHandle;

        private IntPtr _drawable;

        public static GlxGraphicsContext GetCurrent(IntPtr windowHandle, IntPtr display)
        {
            var gc = UnsafeNativeMethods.glXGetCurrentContext();

            return new GlxGraphicsContext(windowHandle, gc, display);
        }

        public GlxGraphicsContext(IntPtr windowHandle, IntPtr graphicsContext, IntPtr display)
        {
            _windowHandle = UnsafeNativeMethods.gdk_x11_window_get_xid(windowHandle);
            _display = UnsafeNativeMethods.gdk_x11_display_get_xdisplay(display);
            _graphicsContext = graphicsContext;
        }

        private IntPtr _graphicsContext;
        private IntPtr _display;
        
        public override void MakeCurrent()
        {
            UnsafeNativeMethods.glXMakeCurrent(_display, _windowHandle, _graphicsContext);
        }

        public override void SwapBuffers()
        {
            UnsafeNativeMethods.glXSwapBuffers(_display, _windowHandle);
        }

        public override void ClearCurrent()
        {
            UnsafeNativeMethods.glXMakeCurrent(_display, _windowHandle, IntPtr.Zero);
        }
    }
}