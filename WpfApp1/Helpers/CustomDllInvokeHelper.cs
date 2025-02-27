using System;
using System.Runtime.InteropServices;

namespace ERad5TestGUI.Helpers
{
    public class CustomDllInvokeHelper
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(string path);
        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr lib, String funcName);
        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr lib);
        private IntPtr MLib;
        public CustomDllInvokeHelper(string dllPath)
        {
            if (!System.IO.File.Exists(dllPath))
            {
                throw new System.IO.FileNotFoundException($"File not found! {dllPath}");
            }

            MLib = LoadLibrary(dllPath);
        }
        ~CustomDllInvokeHelper()
        {
            FreeLibrary(MLib);
        }
        public TDelegate Invoke<TDelegate>(string APIName) where TDelegate : System.Delegate
        {
            IntPtr api = GetProcAddress(MLib, APIName);
            ///不能将此方法用于通过 C++ 获取的函数指针
            ///只能将此方法用于纯非托管函数指针
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(api);
        }
    }
}
