/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrosArc {
    class Program {
        private static string HELP = @"Usage: IrosArc [Create|Extract|List|MakePatch|ApplyPatch] [ArchiveFile] (parameters)

IrosArc Create [ArchiveFile] [BaseFolder] [CompressionType]
    BaseFolder = folder to pack
    CompressionType = Nothing|Everything|ByExtension|ByContent

IrosArc Extract [ArchiveFile] [DestinationFolder] [filter]
    DestinationFolder = folder to extract into. 
    Filter = (Optional) Filename(s) to extract.

IrosArc List [ArchiveFile]
    Describe contents of archive                 

IrosArc MakePatch [OriginalArchive] [NewArchive] [PatchFile] [CompressionType]
    OriginalArchive = original archive you want to make a patch for
    NewArchive = updated version you want the patch to update to
    PatchFile = location to save patch into

IrosArc ApplyPatch [ArchiveFile] [PatchFile]
    ArchiveFile = Original archive
    PatchFile = patch to apply
    Archive is updated IN PLACE. Take a backup copy first if you need to keep the original!
        ";

        static void Main(string[] args) {
            if (args.Length == 0) {
                System.Console.WriteLine(HELP);
                return;
            }
            _7thWrapperLib.RuntimeLog.Enabled = args.Any(s => s.Equals("/LOG", StringComparison.InvariantCultureIgnoreCase));

            Action<double, string> onProgress = (d, s) => {
                            Console.WriteLine(String.Format("{0}%: {1}", (int)(d * 100), s));
                        };

            if (args[0].Equals("LIST", StringComparison.InvariantCultureIgnoreCase)) {
                _7thWrapperLib.IrosArc iro = new _7thWrapperLib.IrosArc(args[1]);
                foreach (string s in iro.GetInformation())
                    Console.WriteLine(s);
            } else if (args[0].Equals("CREATE", StringComparison.InvariantCultureIgnoreCase)) {
                List<_7thWrapperLib.IrosArc.ArchiveCreateEntry> entries = new List<_7thWrapperLib.IrosArc.ArchiveCreateEntry>();
                string dir = args[2];
                foreach(string file in System.IO.Directory.GetFiles(dir, "*", System.IO.SearchOption.AllDirectories))
                    entries.Add(_7thWrapperLib.IrosArc.ArchiveCreateEntry.FromDisk(dir, file.Substring(dir.Length).Trim('/', '\\')));
                _7thWrapperLib.CompressType compress = (_7thWrapperLib.CompressType)Enum.Parse(typeof(_7thWrapperLib.CompressType), args[3]);

                using (var fs = new System.IO.FileStream(args[1], System.IO.FileMode.Create))
                    _7thWrapperLib.IrosArc.Create(fs, 
                        entries, 
                        _7thWrapperLib.ArchiveFlags.None, 
                        compress, 
                        onProgress
                        );
            } else if (args[0].Equals("EXTRACT", StringComparison.InvariantCultureIgnoreCase)) {
                _7thWrapperLib.IrosArc iro = new _7thWrapperLib.IrosArc(args[1]);
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                string filter = args.Length > 3 ? args[3] : String.Empty;
                foreach (string file in iro.AllFileNames()) {
                    if (!String.IsNullOrEmpty(filter) && (file.IndexOf(filter) < 0)) continue;
                    Console.WriteLine("Writing " + file);
                    byte[] data = iro.GetBytes(file);
                    string fn = System.IO.Path.Combine(args[2], file);
                    fn = fn.Replace("%", "___").Replace(":", "---");
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fn));
                    System.IO.File.WriteAllBytes(fn, data);
                }
                sw.Stop();
                Console.WriteLine("Done in " + (sw.ElapsedMilliseconds / 1000f) + " seconds");
            } else if (args[0].Equals("MAKEPATCH", StringComparison.InvariantCultureIgnoreCase)) {
                _7thWrapperLib.IrosArc orig = new _7thWrapperLib.IrosArc(args[1]);
                _7thWrapperLib.IrosArc newarc = new _7thWrapperLib.IrosArc(args[2]);
                _7thWrapperLib.CompressType compress = (_7thWrapperLib.CompressType)Enum.Parse(typeof(_7thWrapperLib.CompressType), args[4]);
                using(var fs = new System.IO.FileStream(args[3], System.IO.FileMode.Create)) {
                    _7thWrapperLib.IrosPatcher.Create(orig, newarc, fs, compress, onProgress);
                }
            } else if (args[0].Equals("APPLYPATCH", StringComparison.InvariantCultureIgnoreCase)) {
                _7thWrapperLib.IrosArc orig = new _7thWrapperLib.IrosArc(args[1], true);
                _7thWrapperLib.IrosArc patch = new _7thWrapperLib.IrosArc(args[2]);
                orig.ApplyPatch(patch, onProgress);
            }
        }
    }
}
