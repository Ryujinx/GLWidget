﻿using System;
using OpenTK;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;

namespace OpenTK
{
    public class GTKBindingHelper : IBindingsContext
    {
        public static OSPlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return OSPlatform.Windows;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return OSPlatform.Linux;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return OSPlatform.OSX;
                }

                return OSPlatform.Create("None");
            }
        }

        public static bool Loaded;

        private const string GlxLibrary = "libGL.so.1";
        private const string WglLibrary = "opengl32.dll";
        private const string OSXLibrary = "libdl.dylib";


        /// <summary>
        /// Currently loaded libraries.
        /// </summary>
        private static readonly Dictionary<string, IntPtr> _LibraryHandles = new Dictionary<string, IntPtr>();

        public bool IsGlxRequired { get; set; }

        public IntPtr GetProcAddress(string procName)
        {
            if (CurrentPlatform == OSPlatform.Windows)
            {
                var addr = GetProcAddressWgl(procName);
                if (addr == null || addr == IntPtr.Zero)
                {
                    var library = UnsafeNativeMethods.LoadLibrary(WglLibrary);

                    addr = UnsafeNativeMethods.GetProcAddress(library, procName);
                }

                if (addr != IntPtr.Zero)
                {
                    Loaded = true;
                }
                return addr;
            }
            else if (CurrentPlatform == OSPlatform.Linux)
            {
                return GetProcAddressGlx(procName);
            }
            else if(CurrentPlatform == OSPlatform.OSX)
            {
                var osxAddr = !IsGlxRequired ? GetProcAddressOSX(procName) : GetProcAddressGlx(procName);
                if (osxAddr != IntPtr.Zero)
                {
                    Loaded = true;
                }
                return osxAddr;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void InitializeGlBindings()
        {
            // We don't put a hard dependency on OpenTK.Graphics here.
            // So we need to use reflection to initialize the GL bindings, so users don't have to.

            // Try to load OpenTK.Graphics assembly.
            Assembly assembly;
            try
            {
                assembly = Assembly.Load("OpenTK.Graphics");
            }
            catch
            {
                // Failed to load graphics, oh well.
                // Up to the user I guess?
                // TODO: Should we expose this load failure to the user better?
                return;
            }

            var provider = new GTKBindingHelper();

            void LoadBindings(string typeNamespace)
            {
                var type = assembly.GetType($"OpenTK.Graphics.{typeNamespace}.GL");
                if (type == null)
                {
                    return;
                }

                var load = type.GetMethod("LoadBindings");
                load.Invoke(null, new object[] { provider });
            }

            LoadBindings("ES11");
            LoadBindings("ES20");
            LoadBindings("ES30");
            LoadBindings("OpenGL");
            LoadBindings("OpenGL4");
        }

        private static IntPtr GetProcAddressWgl(string function)
        {
            return UnsafeNativeMethods.wglGetProcAddress(function);
        }

        private static void LoadLibraries()
        {
            if (Loaded)
            {
                return;
            }

            string function = "glXGetProcAddress";

            IntPtr handle = GetLibraryHandle(GlxLibrary, true);

            if (handle == IntPtr.Zero)
                throw new ArgumentNullException(nameof(handle));

            IntPtr functionPtr = UnsafeNativeMethods.dlsym(handle, function);

            if (functionPtr != IntPtr.Zero)
                Delegates.pglXGetProcAddress = (Delegates.glXGetProcAddress)Marshal.GetDelegateForFunctionPointer(functionPtr, typeof(Delegates.glXGetProcAddress));
            
            Loaded = true;
        }

        internal static IntPtr GetLibraryHandle(string libraryPath, bool throws)
        {
            IntPtr libraryHandle;

            if (_LibraryHandles.TryGetValue(libraryPath, out libraryHandle) == false)
            {
                if ((libraryHandle = UnsafeNativeMethods.dlopen(libraryPath, UnsafeNativeMethods.RTLD_LAZY)) == IntPtr.Zero)
                {
                    if (throws)
                        throw new InvalidOperationException($"unable to load library at {libraryPath}", new InvalidOperationException(UnsafeNativeMethods.dlerror()));
                }

                _LibraryHandles.Add(libraryPath, libraryHandle);
            }

            return libraryHandle;
        }

        private static IntPtr GetProcAddressGlx(string function)
        {
            LoadLibraries();

            return Delegates.pglXGetProcAddress != null ? Delegates.pglXGetProcAddress(function) : IntPtr.Zero;
        }

        private static IntPtr GetProcAddressOSX(string function)
        {
            string fname = "_" + function;
            if (!UnsafeNativeMethods.NSIsSymbolNameDefined(fname))
                return IntPtr.Zero;

            IntPtr symbol = UnsafeNativeMethods.NSLookupAndBindSymbol(fname);

            if (symbol != IntPtr.Zero)
                symbol = UnsafeNativeMethods.NSAddressOfSymbol(symbol);

            return symbol;
        }

        internal static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr handle, string funcname);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr handle, IntPtr funcname);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr LoadLibrary(string dllName);

            [DllImport(WglLibrary, EntryPoint = "wglGetProcAddress", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr wglGetProcAddress(string lpszProc);

            [DllImport(WglLibrary, EntryPoint = "wglGetCurrentContext")]
            public extern static IntPtr wglGetCurrentContext();

            [DllImport(WglLibrary, EntryPoint = "wglGetCurrentDC")]
            public static extern IntPtr wglGetCurrentDC();

            [DllImport(WglLibrary, EntryPoint = "wglSwapBuffers", ExactSpelling = true)]
            public static extern bool wglSwapBuffers(IntPtr hdc);

            [DllImport("opengl32.dll", SetLastError = true, EntryPoint = "wglMakeCurrent")]
            public static extern bool WglMakeCurrent(IntPtr hdc, IntPtr hglrc);

            [DllImport(OSXLibrary, EntryPoint = "NSIsSymbolNameDefined")]
            public static extern bool NSIsSymbolNameDefined(string s);

            [DllImport(OSXLibrary, EntryPoint = "NSLookupAndBindSymbol")]
            public static extern IntPtr NSLookupAndBindSymbol(string s);

            [DllImport(OSXLibrary, EntryPoint = "NSAddressOfSymbol")]
            public static extern IntPtr NSAddressOfSymbol(IntPtr symbol);

            [DllImport(GlxLibrary, EntryPoint = "glXGetCurrentContext")]
            public static extern IntPtr glXGetCurrentContext();

            [DllImport(GlxLibrary, EntryPoint = "glXMakeCurrent")]
            public static extern bool glXMakeCurrent(IntPtr display, IntPtr drawable, IntPtr context);

            [DllImport(GlxLibrary, EntryPoint = "glXSwapBuffers")]
            public static extern void glXSwapBuffers(IntPtr display, IntPtr drawable);

            public const int RTLD_LAZY = 1;

            public const int RTLD_NOW = 2;

			[DllImport("dl")]
			public static extern IntPtr dlopen(string filename, int flags);

			[DllImport("dl")]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("dl")]
            public static extern string dlerror();

            public const string GdkNativeDll = "libgdk-3-0.dll";

            [DllImport(GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr gdk_win32_drawable_get_handle(IntPtr raw);

            [DllImport(GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr gdk_win32_hdc_get(IntPtr drawable, IntPtr gc, int usage);

            [DllImport(GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern void gdk_win32_hdc_release(IntPtr drawable, IntPtr gc, int usage);

            [DllImport("libgdk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr raw);

            [DllImport("libgdk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr gdk_x11_window_get_xid(IntPtr raw);

            [DllImport(GdkNativeDll, CallingConvention = CallingConvention.Cdecl)]
            public static extern void gdk_window_get_internal_paint_info(IntPtr raw, out IntPtr real_drawable, out int x, out int y);
        }

        private static class Delegates
        {
            public delegate IntPtr glXGetProcAddress(string procName);

            public static glXGetProcAddress pglXGetProcAddress;
        }
    }
}