namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Role")]
    public partial class Role
    {
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}
