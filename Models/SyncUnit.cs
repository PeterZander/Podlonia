using System;
using System.Collections.Generic;

#nullable disable

namespace Podlonia.Models
{
    public partial class SyncUnit
    {
        public long Id { get; set; }
        public string Path { get; set; }

        public int MaxFilesPerFeed { get; set; }
        public long MaxStorageSpacePerFeed { get; set; }
        public int MaxAgeDays { get; set; }
    }
}
