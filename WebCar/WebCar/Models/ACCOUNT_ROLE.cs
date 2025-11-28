using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    [Table("ACCOUNT_ROLE")]
    public partial class ACCOUNT_ROLE
    {
        [Key]
        public int MATK { get; set; }

        [Required]
        public int MAKH { get; set; }

        [StringLength(30)]
        public string ROLENAME { get; set; }

        // ✅ Navigation property
        [ForeignKey("MAKH")]
        public virtual CUSTOMER CUSTOMER { get; set; }
    }
}