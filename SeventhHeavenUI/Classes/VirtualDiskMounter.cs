using DiscUtils;
using DiscUtils.Ntfs;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using System.IO;

namespace _7thHeaven.Code
{
    public class VirtualDiskMounter
    {
        Vanara.IO.VirtualDisk _attachedDisk = null;

        /// <summary>
        /// Creates a .vhd formatted as an NTFS partition with the label 'FF7DISC1'
        /// </summary>
        /// <param name="vhdPath"></param>
        /// <remarks>
        /// code adapted from: https://stackoverflow.com/questions/54762062/discutils-create-an-ntfs-vhd
        /// </remarks>
        public static void CreateVirtualDisk(string vhdPath)
        {
            var diskSize = 15 * 1024 * 1024; // 15 MB - need some space to make ntfs partition on virtual disk

            using (var fs = new FileStream(vhdPath, FileMode.OpenOrCreate))
            {
                DiscUtils.Vhd.Disk destDisk = DiscUtils.Vhd.Disk.InitializeDynamic(fs, Ownership.None, diskSize);


                BiosPartitionTable.Initialize(destDisk, WellKnownPartitionType.WindowsNtfs);
                var volMgr = new VolumeManager(destDisk);

                var label = $"FF7DISC1";

                using (var destNtfs = NtfsFileSystem.Format(volMgr.GetLogicalVolumes()[0], label, new NtfsFormatOptions()))
                {
                    destNtfs.NtfsOptions.ShortNameCreation = ShortFileNameOption.Disabled;
                }

                //commit everything to the stream before closing
                fs.Flush();
            }
        }

        /// <summary>
        /// opens the given .vhd (creates it if not exist) and attaches as a disk to assign it a drive letter.
        /// <see cref="_attachedDisk"/> will hold the reference to the mounted virtual disk.
        /// </summary>
        /// <param name="vhdPath"></param>
        public void MountVirtualDisk(string vhdPath)
        {
            if (!File.Exists(vhdPath))
            {
                CreateVirtualDisk(vhdPath);
            }

            _attachedDisk = Vanara.IO.VirtualDisk.Open(vhdPath, false);
            _attachedDisk.Attach();
        }

        /// <summary>
        /// detaches, closes, and disposes of <see cref="_attachedDisk"/> if not null.
        /// </summary>
        public void UnmountVirtualDisk()
        {
            if (_attachedDisk != null)
            {
                _attachedDisk.Detach();
                _attachedDisk.Close();
                _attachedDisk.Dispose();
            }

            _attachedDisk = null;
        }
    }
}
