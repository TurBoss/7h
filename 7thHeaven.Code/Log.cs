/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop {
    public static class Log {
        private static object streamLock = new object();
        private static System.IO.StreamWriter _sw;

        static Log() {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string file = System.IO.Path.Combine(appPath, "7thWorkshop", "applog.txt");

            if (System.IO.File.Exists(file)) {
                try {
                    System.IO.File.Delete(file);
                } catch (System.IO.IOException) {

                }
            }
            try {
                _sw = new System.IO.StreamWriter(file) { AutoFlush = true };
            } catch (System.IO.IOException) {

            }
        }

        public static void Write(string s) {
            if (_sw != null)
                lock (streamLock)
                    _sw.WriteLine(s);
        }
    }
}
