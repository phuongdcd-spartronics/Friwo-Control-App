namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SerialLinking")]
    public partial class SerialLinking
    {
        [Key]
        [Column(Order = 0)]
        public string InternalSerial { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CustomSerial { get; set; }

        [Required]
        [StringLength(255)]
        public string Station { get; set; }

        public virtual BarcodeInfo BarcodeInfo { get; set; }
    }
}
