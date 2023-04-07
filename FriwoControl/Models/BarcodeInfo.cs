namespace FriwoControl.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BarcodeInfo")]
    public partial class BarcodeInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BarcodeInfo()
        {
            SerialLinkings = new HashSet<SerialLinking>();
            SerialRoutings = new HashSet<SerialRouting>();
        }

        [Key]
        public string Barcode { get; set; }

        [Required]
        [StringLength(32)]
        public string AssemblyName { get; set; }

        [Required]
        [StringLength(32)]
        public string WorkOrder { get; set; }

        [Required]
        [StringLength(32)]
        public string SerialNumber { get; set; }

        [StringLength(32)]
        public string Revision { get; set; }

        [Required]
        [StringLength(32)]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(32)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SerialLinking> SerialLinkings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SerialRouting> SerialRoutings { get; set; }
    }
}
