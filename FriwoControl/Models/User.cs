namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("User")]
    public partial class User
    {
        [Key]
        [StringLength(32)]
        public string Username { get; set; }

        [Required]
        [StringLength(256)]
        public string FullName { get; set; }

        [Required]
        [StringLength(512)]
        public string Password { get; set; }

        [Required]
        [StringLength(512)]
        public string Roles { get; set; }
    }
}
