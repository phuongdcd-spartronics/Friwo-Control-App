namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BoxItem")]
    public partial class BoxItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Package { get; set; }

        [Required]
        [StringLength(128)]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(32)]
        public string WO { get; set; }

        [Required]
        [StringLength(255)]
        public string AssemblyName { get; set; }

        public int SeqNo { get; set; }

        public DateTime PrintedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Machine { get; set; }
    }
}
