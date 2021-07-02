using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Podlonia.Models
{
    public partial class PodloniaContext : DbContext
    {
        public PodloniaContext()
        {
        }

        public PodloniaContext( DbContextOptions<PodloniaContext> options )
            : base(options)
        {
        }

        public virtual DbSet<RSSEnclosure> Enclosures { get; set; }
        public virtual DbSet<RSSFeed> Feeds { get; set; }
        public virtual DbSet<SyncEnclosure> SyncEnclosures { get; set; }
        public virtual DbSet<SyncUnit> SyncUnits { get; set; }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            if ( !optionsBuilder.IsConfigured )
            {
                if ( Program.Configuration is null ) Program.ReadConfiguration();
                
                optionsBuilder.UseSqlite( $"data source={Program.Configuration.DatabaseFile};" );
            }
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            OnModelCreatingPartial( modelBuilder );
        }

        partial void OnModelCreatingPartial( ModelBuilder modelBuilder );
    }
}
