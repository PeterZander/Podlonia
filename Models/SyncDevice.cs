using System.IO;

namespace Podlonia.Models
{
    public class SyncDevice
    {
        public long Id;
        public DriveInfo Info;

        public SyncDevice( long id, DriveInfo info )
        {
            Id = id;
            Info = info;
        }

        public override string ToString()
        {
            return Info.RootDirectory.FullName;
        }
    }
}