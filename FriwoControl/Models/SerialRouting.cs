namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SerialRouting")]
    public partial class SerialRouting
    {
        public long Id { get; set; }

        [Required]
        [StringLength(128)]
        public string SerialNumber { get; set; }

        public int Step { get; set; }

        [Required]
        [StringLength(255)]
        public string Station { get; set; }

        [StringLength(32)]
        public string WorkOrder { get; set; }

        [StringLength(32)]
        public string AssemblyName { get; set; }

        [StringLength(32)]
        public string Shift { get; set; }

        public DateTime? Timespan { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; }

        public virtual BarcodeInfo BarcodeInfo { get; set; }

        public virtual Status Status1 { get; set; }
    }
}
