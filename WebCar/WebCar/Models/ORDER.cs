using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    [Table("ORDERS")]
    public partial class ORDER
    {
        // ? Constructor to initialize collection
        public ORDER()
        {
            ORDER_DETAIL = new HashSet<ORDER_DETAIL>();
        }

        [Key]
        public int MADON { get; set; }

        [Required]
        public int MAKH { get; set; }

        public DateTime? NGAYDAT { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? TONGTIEN { get; set; }

        [StringLength(30)]
        public string TRANGTHAI { get; set; }

        // ? Navigation properties
        [ForeignKey("MAKH")]
        public virtual CUSTOMER CUSTOMER { get; set; }

        public virtual ICollection<ORDER_DETAIL> ORDER_DETAIL { get; set; }

        // Not mapped - for display only
        [NotMapped]
        public string CustomerName { get; set; }

        [NotMapped]
        public string CustomerEmail { get; set; }

        // Helper properties
        [NotMapped]
        public string StatusBadgeClass
        {
            get
            {
                if (string.IsNullOrEmpty(TRANGTHAI))
                    return "badge bg-secondary";

                if (TRANGTHAI == "Ch? x? lý")
                    return "badge bg-warning";

                if (TRANGTHAI == "?ang x? lý")
                    return "badge bg-info";

                if (TRANGTHAI == "?ã giao")
                    return "badge bg-success";

                if (TRANGTHAI == "?ã h?y")
                    return "badge bg-danger";

                return "badge bg-secondary";
            }
        }

        [NotMapped]
        public string StatusIcon
        {
            get
            {
                if (string.IsNullOrEmpty(TRANGTHAI))
                    return "fas fa-question-circle";

                if (TRANGTHAI == "Ch? x? lý")
                    return "fas fa-clock";

                if (TRANGTHAI == "?ang x? lý")
                    return "fas fa-spinner";

                if (TRANGTHAI == "?ã giao")
                    return "fas fa-check-circle";

                if (TRANGTHAI == "?ã h?y")
                    return "fas fa-times-circle";

                return "fas fa-question-circle";
            }
        }

        [NotMapped]
        public string FormattedDate
        {
            get
            {
                return NGAYDAT.HasValue
                    ? NGAYDAT.Value.ToString("dd/MM/yyyy HH:mm")
                    : "N/A";
            }
        }

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                if (!NGAYDAT.HasValue)
                    return "N/A";

                var timeSpan = DateTime.Now - NGAYDAT.Value;

                if (timeSpan.TotalMinutes < 1)
                    return "V?a xong";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút tr??c";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} gi? tr??c";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} ngày tr??c";

                return NGAYDAT.Value.ToString("dd/MM/yyyy");
            }
        }
    }
}