namespace WebCar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FEEDBACK")]
    public partial class FEEDBACK
    {
        [Key]
        public int MAFB { get; set; }

        public int MAKH { get; set; }

        public int MAXE { get; set; }

        [StringLength(1000)]
        public string NOIDUNG { get; set; }

        public int? DIEMDANHGIA { get; set; }

        public DateTime? NGAYDANHGIA { get; set; }

        public virtual CAR CAR { get; set; }

        public virtual CUSTOMER CUSTOMER { get; set; }
    }
}
