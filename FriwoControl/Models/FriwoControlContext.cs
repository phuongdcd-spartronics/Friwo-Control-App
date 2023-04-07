using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace FriwoControl.Models
{
    public partial class FriwoControlContext : DbContext
    {
        public FriwoControlContext()
            : base("name=FriwoControlContext")
        {
        }

        public virtual DbSet<AssemblyInProcess> AssemblyInProcesses { get; set; }
        public virtual DbSet<ATE_Record> ATE_Record { get; set; }
        public virtual DbSet<BarcodeInfo> BarcodeInfoes { get; set; }
        public virtual DbSet<BoxItem> BoxItems { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<ProdAssembly> ProdAssemblies { get; set; }
        public virtual DbSet<ProdProcess> ProdProcesses { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrders { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<SerialLinking> SerialLinkings { get; set; }
        public virtual DbSet<SerialRouting> SerialRoutings { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BarcodeInfo>()
                .HasMany(e => e.SerialLinkings)
                .WithRequired(e => e.BarcodeInfo)
                .HasForeignKey(e => e.InternalSerial)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BarcodeInfo>()
                .HasMany(e => e.SerialRoutings)
                .WithRequired(e => e.BarcodeInfo)
                .HasForeignKey(e => e.SerialNumber)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProdAssembly>()
                .HasMany(e => e.AssemblyInProcesses)
                .WithRequired(e => e.ProdAssembly)
                .HasForeignKey(e => e.AssemblyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProdProcess>()
                .HasMany(e => e.AssemblyInProcesses)
                .WithRequired(e => e.ProdProcess)
                .HasForeignKey(e => e.ProcessId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductionOrder>()
                .Property(e => e.Quantity)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Status>()
                .HasMany(e => e.SerialRoutings)
                .WithRequired(e => e.Status1)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);
        }
    }
}
