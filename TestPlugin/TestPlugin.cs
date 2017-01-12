/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7thHeaven
{
    public static class Main
    {
        public static void Init(_7thWrapperLib.RuntimeMod mod) {
            using(var sr = new System.IO.StreamReader(mod.Read("data.txt"))) {
                System.Diagnostics.Debug.WriteLine(sr.ReadToEnd());
            }
        }
    }
}
