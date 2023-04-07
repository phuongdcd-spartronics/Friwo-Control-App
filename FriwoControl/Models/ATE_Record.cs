namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ATE_Record
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(10)]
        public string Result { get; set; }

        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(32)]
        public string CreatedBy { get; set; }
    }
}
