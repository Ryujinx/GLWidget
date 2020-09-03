using System;
using OpenTK;
using OpenGL;
using System.Runtime.InteropServices;
using Khronos;
using System.Collections.Generic;

namespace GLWidgetTestGTK3
{
    public class GTKBindingContext : IBindingsContext
    {
        private static bool _loaded;

        private const string GlxLibrary = "libGL.so.1";
        private const string WglLibrary = "opengl32.dll";
        private const string OSXLibrary = "libdl.dylib";


        /// <summary>
        /// Currently loaded libraries.
        /// </summary>
        private static readonly Dictionary<string, IntPtr> _LibraryHandles = new Dictionary<string, IntPtr>();
        
        public IntPtr GetProcAddress(string procName)
        {
            switch (Platform.CurrentPlatformId)
            {
                case Platform.Id.WindowsNT:
                    return GetProcAddressWgl(procName);
                case Platform.Id.Linux:
                    return GetProcAddressGlx(procName);
                case Platform.Id.MacOS:
                    return !Glx.IsRequired ? GetProcAddressOSX(procName) : GetProcAddressGlx(procName);
                default:
                    throw new NotSupportedException();
            }
        }

        private static IntPtr GetProcAddressWgl(string function)
        {
            return UnsafeNativeMethods.wglGetProcAddress(function);
        }

        private static void LoadLibraries()
        {
            if (_loaded)
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
            
            _loaded = true;
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

        private static class UnsafeNativeMethods
        {
            [DllImport(WglLibrary, EntryPoint = "wglGetProcAddress", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr wglGetProcAddress(string lpszProc);

            [DllImport(OSXLibrary, EntryPoint = "NSIsSymbolNameDefined")]
            public static extern bool NSIsSymbolNameDefined(string s);

            [DllImport(OSXLibrary, EntryPoint = "NSLookupAndBindSymbol")]
            public static extern IntPtr NSLookupAndBindSymbol(string s);

            [DllImport(OSXLibrary, EntryPoint = "NSAddressOfSymbol")]
            public static extern IntPtr NSAddressOfSymbol(IntPtr symbol);

            public const int RTLD_LAZY = 1;

            public const int RTLD_NOW = 2;

			[DllImport("dl")]
			public static extern IntPtr dlopen(string filename, int flags);

			[DllImport("dl")]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("dl")]
            public static extern string dlerror();
        }

        private static class Delegates
        {
            public delegate IntPtr glXGetProcAddress(string procName);

            public static glXGetProcAddress pglXGetProcAddress;
        }
    }
}