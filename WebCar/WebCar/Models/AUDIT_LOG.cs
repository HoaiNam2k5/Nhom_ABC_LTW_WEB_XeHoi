using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCar.Models
{
    [Table("AUDIT_LOG")]
    public partial class AUDIT_LOG
    {
        [Key]
        [Display(Name = "Mã log")]
        public int MALOG { get; set; }

        [Display(Name = "Mã tài kho?n")]
        public int? MATK { get; set; }

        [StringLength(100)]
        [Display(Name = "Hành ??ng")]
        public string HANHDONG { get; set; }

        [StringLength(50)]
        [Display(Name = "B?ng tác ??ng")]
        public string BANGTACDONG { get; set; }

        [Display(Name = "Ngày gi?")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? NGAYGIO { get; set; }

        [StringLength(30)]
        [Display(Name = "??a ch? IP")]
        public string IP { get; set; }

        // =========================================
        // NAVIGATION & DISPLAY PROPERTIES
        // =========================================

        /// <summary>
        /// Tên khách hàng (join t? CUSTOMER)
        /// </summary>
        [NotMapped]
        [Display(Name = "Ng??i dùng")]
        public string CustomerName { get; set; }

        /// <summary>
        /// Email khách hàng (join t? CUSTOMER)
        /// </summary>
        [NotMapped]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Role c?a user (join t? ACCOUNT_ROLE)
        /// </summary>
        [NotMapped]
        [Display(Name = "Vai trò")]
        public string RoleName { get; set; }

        // =========================================
        // HELPER PROPERTIES
        // =========================================

        /// <summary>
        /// Hi?n th? tên ng??i dùng ho?c "System"
        /// </summary>
        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomerName))
                    return CustomerName;

                if (MATK.HasValue && MATK.Value > 0)
                    return $"User #{MATK}";

                return "System";
            }
        }

        /// <summary>
        /// Icon cho lo?i hành ??ng
        /// </summary>
        [NotMapped]
        public string ActionIcon
        {
            get
            {
                if (string.IsNullOrEmpty(HANHDONG))
                    return "fas fa-question-circle";

                var action = HANHDONG.ToUpper();

                if (action.Contains("LOGIN"))
                    return "fas fa-sign-in-alt text-success";

                if (action.Contains("LOGOUT"))
                    return "fas fa-sign-out-alt text-warning";

                if (action.Contains("REGISTER"))
                    return "fas fa-user-plus text-info";

                if (action.Contains("INSERT") || action.Contains("CREATE"))
                    return "fas fa-plus-circle text-success";

                if (action.Contains("UPDATE") || action.Contains("EDIT"))
                    return "fas fa-edit text-primary";

                if (action.Contains("DELETE"))
                    return "fas fa-trash-alt text-danger";

                if (action.Contains("ACCESS_DENIED") || action.Contains("DENIED"))
                    return "fas fa-ban text-danger";

                if (action.Contains("GRANT") || action.Contains("ROLE"))
                    return "fas fa-user-shield text-warning";

                return "fas fa-circle text-secondary";
            }
        }

        /// <summary>
        /// CSS class cho badge tr?ng thái
        /// </summary>
        [NotMapped]
        public string StatusBadgeClass
        {
            get
            {
                if (string.IsNullOrEmpty(HANHDONG))
                    return "badge bg-secondary";

                var action = HANHDONG.ToUpper();

                if (action.Contains("LOGIN") || action.Contains("SUCCESS"))
                    return "badge bg-success";

                if (action.Contains("LOGOUT"))
                    return "badge bg-warning";

                if (action.Contains("DELETE") || action.Contains("ACCESS_DENIED"))
                    return "badge bg-danger";

                if (action.Contains("INSERT") || action.Contains("CREATE") || action.Contains("REGISTER"))
                    return "badge bg-info";

                if (action.Contains("UPDATE") || action.Contains("EDIT"))
                    return "badge bg-primary";

                return "badge bg-secondary";
            }
        }

        /// <summary>
        /// Th?i gian ?ã qua (ago format)
        /// </summary>
        [NotMapped]
        public string TimeAgo
        {
            get
            {
                if (!NGAYGIO.HasValue)
                    return "N/A";

                var timeSpan = DateTime.Now - NGAYGIO.Value;

                if (timeSpan.TotalMinutes < 1)
                    return "V?a xong";

                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút tr??c";

                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} gi? tr??c";

                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} ngày tr??c";

                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} tháng tr??c";

                return $"{(int)(timeSpan.TotalDays / 365)} n?m tr??c";
            }
        }

        /// <summary>
        /// Check if action is security-related
        /// </summary>
        [NotMapped]
        public bool IsSecurityEvent
        {
            get
            {
                if (string.IsNullOrEmpty(HANHDONG))
                    return false;

                var action = HANHDONG.ToUpper();
                return action.Contains("LOGIN") ||
                       action.Contains("LOGOUT") ||
                       action.Contains("REGISTER") ||
                       action.Contains("ACCESS_DENIED") ||
                       action.Contains("GRANT") ||
                       action.Contains("REVOKE") ||
                       action.Contains("PASSWORD");
            }
        }

        /// <summary>
        /// Check if action is critical
        /// </summary>
        [NotMapped]
        public bool IsCritical
        {
            get
            {
                if (string.IsNullOrEmpty(HANHDONG))
                    return false;

                var action = HANHDONG.ToUpper();
                return action.Contains("DELETE") ||
                       action.Contains("ACCESS_DENIED") ||
                       action.Contains("FAILED") ||
                       action.Contains("ERROR");
            }
        }

        /// <summary>
        /// Formatted date string
        /// </summary>
        [NotMapped]
        public string FormattedDate
        {
            get
            {
                return NGAYGIO.HasValue
                    ? NGAYGIO.Value.ToString("dd/MM/yyyy HH:mm:ss")
                    : "N/A";
            }
        }

        /// <summary>
        /// Short date string
        /// </summary>
        [NotMapped]
        public string ShortDate
        {
            get
            {
                return NGAYGIO.HasValue
                    ? NGAYGIO.Value.ToString("dd/MM/yyyy")
                    : "N/A";
            }
        }

        /// <summary>
        /// Time only string
        /// </summary>
        [NotMapped]
        public string TimeOnly
        {
            get
            {
                return NGAYGIO.HasValue
                    ? NGAYGIO.Value.ToString("HH:mm:ss")
                    : "N/A";
            }
        }
    }
}