using System.Collections.Generic;

namespace _7thHeaven.Code
{
    public class AvailableUpdate
    {
        public AvailableUpdate()
        {
            Version = "0.0.0.0";
        }

        public string Version { get; set; }

        public string ReleaseDate { get; set; }

        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Link to open in browser (is not direct download)
        /// </summary>
        public string ReleaseDownloadLink { get; set; }

        public List<AvailableUpdate> PreviousVersions { get; set; }
    }
}
