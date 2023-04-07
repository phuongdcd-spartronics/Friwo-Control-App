namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ProductionOrder")]
    public partial class ProductionOrder
    {
        [Key]
        [StringLength(32)]
        public string WO { get; set; }

        [Required]
        [StringLength(256)]
        public string AssemblyName { get; set; }

        public decimal Quantity { get; set; }

        [Required]
        [StringLength(32)]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
