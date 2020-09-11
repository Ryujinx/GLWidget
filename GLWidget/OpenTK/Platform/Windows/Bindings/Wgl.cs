//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2013 Stefanos Apostolopoulos
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

using System;
using System.Runtime.InteropServices;
using System.Security;
using OpenTK.Core.Native;

namespace OpenTK.Platform.Windows
{
#pragma warning disable 3019
#pragma warning disable 1591

    internal partial class Wgl
    {
        static Wgl()
        {
            EntryPointNames = new string[]
            {
                "wglCreateContextAttribsARB",
                "wglGetExtensionsStringARB",
                "wglGetPixelFormatAttribivARB",
                "wglGetPixelFormatAttribfvARB",
                "wglChoosePixelFormatARB",
                "wglMakeContextCurrentARB",
                "wglGetCurrentReadDCARB",
                "wglCreatePbufferARB",
                "wglGetPbufferDCARB",
                "wglReleasePbufferDCARB",
                "wglDestroyPbufferARB",
                "wglQueryPbufferARB",
                "wglBindTexImageARB",
                "wglReleaseTexImageARB",
                "wglSetPbufferAttribARB",
                "wglGetExtensionsStringEXT",
                "wglSwapIntervalEXT",
                "wglGetSwapIntervalEXT",
            };
            EntryPoints = new IntPtr[EntryPointNames.Length];
        }

        public delegate IntPtr CreateContextAttribsARBDelegate(IntPtr hDC, IntPtr shareHGLRC, int[] attribs);
        public static CreateContextAttribsARBDelegate wglCreateContextAttribs;

        public delegate bool SwapIntervalEXT(int interval);
        public static SwapIntervalEXT wglSwapIntervalEXT;

        public delegate int GetSwapIntervalEXT();
        public static GetSwapIntervalEXT wglGetSwapIntervalEXT;

        internal delegate IntPtr GetExtensionsStringARBDelegate(IntPtr hdc);
        internal static GetExtensionsStringARBDelegate wglGetExtensionsStringARB;
        
        internal delegate IntPtr GetExtensionsStringEXTDelegate();
        internal static GetExtensionsStringEXTDelegate wglGetExtensionsStringEXT;

        public delegate bool ChoosePixelFormatARB(IntPtr hdc, int[] piAttribIList, float[] pfAttribFList, uint nMaxFormats, int[] piFormats, ref uint nNumFormats);
        public static ChoosePixelFormatARB wglChoosePixelFormatARB;

        public delegate bool GetPixelFormatAttribivARB(IntPtr hdc, int iPixelFormat, int iLayerPlane, uint nAttributes, int[] piAttributes, int[] piValues);
        public static GetPixelFormatAttribivARB WglGetPixelFormatAttribivARB;

        public static string GetExtensionsStringARB(IntPtr hdc)
        {
            unsafe
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(wglGetExtensionsStringARB(hdc));
            }
        }
        
        public static string GetExtensionsStringEXT()
        {
            unsafe
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(wglGetExtensionsStringEXT());
            }
        }

        public static TDelegate GetProcAddress<TDelegate>(string name) where TDelegate : class
        {
            IntPtr addr = GetProcAddress(name);
            if (addr == IntPtr.Zero) return null;
            return (TDelegate)(object)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(addr, typeof(TDelegate));
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglCreateContext", ExactSpelling = true, SetLastError = true)]
        internal extern static IntPtr CreateContext(IntPtr hDc);
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglDeleteContext", ExactSpelling = true, SetLastError = true)]
        internal extern static Boolean DeleteContext(IntPtr oldContext);
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglGetCurrentContext", ExactSpelling = true, SetLastError = true)]
        internal extern static IntPtr GetCurrentContext();
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglMakeCurrent", ExactSpelling = true, SetLastError = true)]
        internal extern static Boolean MakeCurrent(IntPtr hDc, IntPtr newContext);
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglGetCurrentDC", ExactSpelling = true, SetLastError = true)]
        internal extern static IntPtr GetCurrentDC();
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglGetProcAddress", ExactSpelling = true, SetLastError = true)]
        internal extern static IntPtr GetProcAddress(String lpszProc);
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglGetProcAddress", ExactSpelling = true, SetLastError = true)]
        internal extern static IntPtr GetProcAddress(IntPtr lpszProc);
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Wgl.Library, EntryPoint = "wglShareLists", ExactSpelling = true, SetLastError = true)]
        internal extern static Boolean ShareLists(IntPtr hrcSrvShare, IntPtr hrcSrvSource);
    }
}
