using System;
using System.Collections.Generic;

#nullable disable

namespace Podlonia.Models
{
    public partial class SyncEnclosure
    {
        public long Id { get; set; }
        public long DeviceId { get; set; }
        public long FeedId { get; set; }
        public long EncId { get; set; }
        public string FullName { get; set; }
    }
}
