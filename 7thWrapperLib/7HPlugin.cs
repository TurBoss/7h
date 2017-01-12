/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop {
    public abstract class _7HPlugin {
        public abstract void Start(_7thWrapperLib.RuntimeMod mod);
        public abstract void Stop();
    }
}
