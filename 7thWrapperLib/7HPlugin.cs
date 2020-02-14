/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Windows.Forms;

namespace Iros._7th.Workshop
{
    public abstract class _7HPlugin {
        public abstract void Start(_7thWrapperLib.RuntimeMod mod);
        public abstract void Stop();
    }

    /// <summary>
    /// Class to implement IWin32Window interface so WPF Window handle can be used as an Owner handle for Winform plugins
    /// </summary>
    /// <remarks>
    /// reference: https://stackoverflow.com/questions/7822164/winform-dialog-with-wpf-window-as-parent
    /// </remarks>
    public class Wpf32Window : IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(IntPtr wpfHandle)
        {
            Handle = wpfHandle;
        }
    }
}
