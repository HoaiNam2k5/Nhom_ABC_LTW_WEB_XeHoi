using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    [Table("FEEDBACK")]
    public partial class FEEDBACK
    {
        [Key]
        public int MAFB { get; set; }

        [Required]
        public int MAKH { get; set; }

        [Required]
        public int MAXE { get; set; }

        [StringLength(1000)]
        public string NOIDUNG { get; set; }

        public int? DIEMDANHGIA { get; set; }

        public DateTime? NGAYDANHGIA { get; set; }

        // ? Navigation properties
        [ForeignKey("MAKH")]
        public virtual CUSTOMER CUSTOMER { get; set; }

        [ForeignKey("MAXE")]
        public virtual CAR CAR { get; set; }
    }
}