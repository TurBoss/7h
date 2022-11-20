using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace _7thWrapperProxy
{
    static unsafe class Proxy
    {
        private static Assembly? lib;
        private static Type? t;
        private static MethodInfo? m;

        [UnmanagedCallersOnly]
        public static int Main()
        {
            try
            {
                lib = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), "7thWrapperLib.dll"));
                t = lib.GetType("_7thWrapperLib.Wrap");

                if (t != null) m = t.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                if (m != null) m.Invoke(null, new object[] { Process.GetCurrentProcess(), Type.Missing });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return 0;
        }
    }
}