using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    [Table("ORDER_DETAIL")]
    public partial class ORDER_DETAIL
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MADON { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAXE { get; set; }

        public int? SOLUONG { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? DONGIA { get; set; }

        // ? Navigation properties
        [ForeignKey("MADON")]
        public virtual ORDER ORDER { get; set; }

        [ForeignKey("MAXE")]
        public virtual CAR CAR { get; set; }

        // Not mapped - for display
        [NotMapped]
        public string TENXE { get; set; }

        [NotMapped]
        public string HANGXE { get; set; }

        [NotMapped]
        public string HINHANH { get; set; }
    }
}