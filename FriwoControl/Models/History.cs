namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("History")]
    public partial class History
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Reference { get; set; }

        [Required]
        [StringLength(255)]
        public string Action { get; set; }

        public string Description { get; set; }

        [Required]
        [StringLength(32)]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
