using System;
using System.Runtime.InteropServices;
using OpenGL;

using static OpenTK.GTKBindingHelper;

namespace OpenTK
{
    public interface ILegacyGraphicsContext
    {
        void MakeCurrent();
        void SwapBuffers();
        void ClearCurrent();
        void SwapInterval(int interval);
    }

    public abstract class LegacyGraphicsContext : ILegacyGraphicsContext
    {
        public static IntPtr Display{ get; set; }

        public abstract void MakeCurrent();

        public abstract void SwapBuffers();

        public static void InitializeDefaults(IntPtr handle)
        {
            if (CurrentPlatform == OSPlatform.Windows)
            {
                IntPtr deviceContext = Wgl.GetDC(UnsafeNativeMethods.gdk_win32_window_get_handle(handle));

                Wgl.PIXELFORMATDESCRIPTOR pfd = new Wgl.PIXELFORMATDESCRIPTOR(24);

                pfd.dwFlags |= Wgl.PixelFormatDescriptorFlags.DepthDontCare | Wgl.PixelFormatDescriptorFlags.DoublebufferDontCare | Wgl.PixelFormatDescriptorFlags.StereoDontCare;

                int pFormat = Wgl.ChoosePixelFormat(deviceContext, ref pfd);

                bool res = Wgl.DescribePixelFormat(deviceContext, pFormat, (uint)pfd.nSize, ref pfd) != 0;

                res = Wgl.SetPixelFormat(deviceContext, pFormat, ref pfd);
            }
        }

        public static ILegacyGraphicsContext GetCurrentContext(IntPtr handle)
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
            else if (currentPlatform == OSPlatform.OSX)
            {
                return CglGraphicsContext.GetCurrent();
            }

            return null;
        }

        public abstract void ClearCurrent();
        public abstract void SwapInterval(int interval);
    }

    public class WglGraphicsContext : LegacyGraphicsContext
    {
        private delegate int wglSwapIntervalExtDelegate(int interval);
        private static wglSwapIntervalExtDelegate wglSwapIntervalExt = null;

        private IntPtr _windowHandle;
        private IntPtr _deviceContext;

        public WglGraphicsContext(IntPtr deviceContext, IntPtr graphicsContext, IntPtr windowHandle = default)
        {
            _deviceContext = deviceContext;
            _graphicsContext = graphicsContext;
            _windowHandle = windowHandle;

            IntPtr swapIntervalPointer = UnsafeNativeMethods.wglGetProcAddress("wglSwapIntervalEXT");

            if(swapIntervalPointer != null && swapIntervalPointer != IntPtr.Zero)
            {
                wglSwapIntervalExt = (wglSwapIntervalExtDelegate)Marshal.GetDelegateForFunctionPointer(
                        swapIntervalPointer, typeof(wglSwapIntervalExtDelegate));
            }
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
            UnsafeNativeMethods.WglMakeCurrent(_deviceContext, IntPtr.Zero);
        }

        public override void SwapInterval(int interval)
        {
            wglSwapIntervalExt?.Invoke(interval);
        }
    }

    public class GlxGraphicsContext : LegacyGraphicsContext
    {
        private IntPtr _windowHandle;

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

        public override void SwapInterval(int interval)
        {
            UnsafeNativeMethods.glXSwapIntervalEXT(interval);
        }
    }

    public class CglGraphicsContext : LegacyGraphicsContext
    {
        private IntPtr _windowHandle;

        public static CglGraphicsContext GetCurrent()
        {
            var gc = UnsafeNativeMethods.CGLGetCurrentContext();

            return new CglGraphicsContext(gc);
        }

        public CglGraphicsContext(IntPtr graphicsContext)
        {
            _graphicsContext = graphicsContext;
        }

        private IntPtr _graphicsContext;
        private IntPtr _display;

        public override void MakeCurrent()
        {
            UnsafeNativeMethods.CGLSetCurrentContext(_graphicsContext);
        }

        public override void SwapBuffers()
        {
            UnsafeNativeMethods.CGLFlushDrawable(_graphicsContext);
        }

        public override void ClearCurrent()
        {
            UnsafeNativeMethods.CGLSetCurrentContext(IntPtr.Zero);
        }

        public override void SwapInterval(int interval)
        {
            UnsafeNativeMethods.CGLSetParameter(_graphicsContext, 222 , ref interval);
        }
    }
}