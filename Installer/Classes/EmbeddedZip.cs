using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer.Classes
{
    public static class EmbeddedZip
    {
        public static string tempPath 
        {
            get
            {
                return Environment.GetEnvironmentVariable("TEMP") + "\\";
            }
        }
        /// <summary>
        /// Extracts the contents of a zip file to the 
        /// Temp Folder
        /// </summary>
        public static void ExtractZip(string resName, string extractTo)
        {
            try
            {
                //write the resource zip file to the temp directory
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resources." + resName + ".zip"))
                {
                    using (FileStream bw = new FileStream(extractTo, FileMode.Create))
                    {
                        //read until we reach the end of the file
                        while (stream.Position < stream.Length)
                        {
                            //byte array to hold file bytes
                            byte[] bits = new byte[stream.Length];
                            //read in the bytes
                            stream.Read(bits, 0, (int)stream.Length);
                            //write out the bytes
                            bw.Write(bits, 0, (int)stream.Length);
                        }
                    }
                    stream.Close();
                }

            }
            catch (Exception e)
            {
                //handle the error
            }
        }
    }
}
