using System;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Filters;

namespace WebCar.Controllers
{
    [Authorize]
    [AuthorizeRole("ADMIN", "MANAGER")]
    public class AuditController : Controller
    {
        private readonly AuditRepository _auditRepo;

        public AuditController()
        {
            _auditRepo = new AuditRepository();
        }

        // =========================================
        // GET: Audit/Index
        // =========================================
        public ActionResult Index(string action, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            try
            {
                var logs = _auditRepo.GetAuditLogs(action, fromDate, toDate);

                // Pagination
                int pageSize = 20;
                int totalRecords = logs.Count;
                var pagedLogs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                ViewBag.ActionFilter = action;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                // Get unique actions for filter
                ViewBag.Actions = _auditRepo.GetUniqueActions();

                // ✅ Debug info
                System.Diagnostics.Debug.WriteLine($"Total logs: {totalRecords}");
                System.Diagnostics.Debug.WriteLine($"Paged logs: {pagedLogs.Count}");

                return View(pagedLogs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Audit/Index: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new System.Collections.Generic.List<Models.AUDIT_LOG>());
            }
        }

        // =========================================
        // GET: Audit/Dashboard
        // =========================================
        [AuthorizeRole("ADMIN")]
        public ActionResult Dashboard()
        {
            try
            {
                ViewBag.TotalLogins = _auditRepo.GetLoginCount();
                ViewBag.FailedLogins = _auditRepo.GetFailedLoginCount();
                ViewBag.TotalActions = _auditRepo.GetTotalActionCount();
                ViewBag.UniqueUsers = _auditRepo.GetUniqueUserCount();

                // Chart data
                ViewBag.LoginChart = _auditRepo.GetLoginChartData();
                ViewBag.ActionChart = _auditRepo.GetActionChartData();

                // ✅ Debug
                System.Diagnostics.Debug.WriteLine($"Total Logins: {ViewBag.TotalLogins}");
                System.Diagnostics.Debug.WriteLine($"Chart data count: {ViewBag.LoginChart.Count}");

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Dashboard: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;

                // ✅ Set default values
                ViewBag.TotalLogins = 0;
                ViewBag.FailedLogins = 0;
                ViewBag.TotalActions = 0;
                ViewBag.UniqueUsers = 0;
                ViewBag.LoginChart = new System.Collections.Generic.Dictionary<string, int>();
                ViewBag.ActionChart = new System.Collections.Generic.Dictionary<string, int>();

                return View();
            }
        }

        // =========================================
        // GET: Audit/SecurityEvents
        // =========================================
        [AuthorizeRole("ADMIN")]
        public ActionResult SecurityEvents()
        {
            try
            {
                var securityEvents = _auditRepo.GetSecurityEvents();

                System.Diagnostics.Debug.WriteLine($"Security events: {securityEvents.Count}");

                return View(securityEvents);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SecurityEvents: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new System.Collections.Generic.List<Models.AUDIT_LOG>());
            }
        }
    }
}