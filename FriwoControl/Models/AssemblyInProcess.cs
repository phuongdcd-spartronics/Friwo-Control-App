namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AssemblyInProcess")]
    public partial class AssemblyInProcess
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AssemblyId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProcessId { get; set; }

        public int OrderNo { get; set; }

        public virtual ProdAssembly ProdAssembly { get; set; }

        public virtual ProdProcess ProdProcess { get; set; }
    }
}
