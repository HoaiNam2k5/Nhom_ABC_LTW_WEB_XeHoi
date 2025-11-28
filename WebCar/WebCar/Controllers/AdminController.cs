using System;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models;
using WebCar.Filters;
using System.Collections.Generic;

namespace WebCar.Controllers
{
    [Authorize]
    [AuthorizeRole("ADMIN", "MANAGER")]
    public class AdminController : Controller
    {
        private readonly CarRepository _carRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly OrderRepository _orderRepo;

        public AdminController()
        {
            _carRepo = new CarRepository();
            _customerRepo = new CustomerRepository();
            _orderRepo = new OrderRepository();
        }

        // =========================================
        // GET: Admin/Index (Dashboard)
        // =========================================
        public ActionResult Index()
        {
            try
            {
                ViewBag.TotalCars = _carRepo.GetTotalCars();
                ViewBag.TotalCustomers = _customerRepo.GetTotalCustomers();
                ViewBag.TotalOrders = _orderRepo.GetTotalOrders();
                ViewBag.TotalRevenue = _orderRepo.GetTotalRevenue();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dashboard: " + ex.Message;
                return View();
            }
        }

        // =========================================
        // GET: Admin/Cars
        // =========================================
        public ActionResult Cars(string search, int page = 1)
        {
            try
            {
                var cars = _carRepo.GetAllCars(search);

                // Pagination
                int pageSize = 10;
                var pagedCars = cars.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)cars.Count / pageSize);
                ViewBag.SearchTerm = search;

                return View(pagedCars);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }

        // =========================================
        // GET: Admin/Customers
        // =========================================
        public ActionResult Customers(string search, int page = 1)
        {
            try
            {
                var customers = _customerRepo.GetAllCustomers(search);

                // Pagination
                int pageSize = 10;
                var pagedCustomers = customers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)customers.Count / pageSize);
                ViewBag.SearchTerm = search;

                return View(pagedCustomers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }

        public ActionResult Orders(string status, string fromDate, string toDate, int page = 1)
        {
            try
            {
                // Lấy tất cả đơn hàng
                var orders = _orderRepo.GetAllOrders(status);

                // ✅ Filter by date range if provided
                if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime from))
                {
                    orders = orders.Where(x => x.NGAYDAT >= from).ToList();
                }

                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime to))
                {
                    orders = orders.Where(x => x.NGAYDAT <= to.AddDays(1)).ToList();
                }

                // ✅ Statistics (calculate once before pagination)
                ViewBag.TotalOrders = orders.Count;
                ViewBag.PendingOrders = orders.Count(x => x.TRANGTHAI == "Chờ xử lý");
                ViewBag.ProcessingOrders = orders.Count(x => x.TRANGTHAI == "Đang xử lý");
                ViewBag.CompletedOrders = orders.Count(x => x.TRANGTHAI == "Đã giao");
                ViewBag.CancelledOrders = orders.Count(x => x.TRANGTHAI == "Đã hủy");

                // ✅ Pagination
                const int pageSize = 10;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)orders.Count / pageSize);
                ViewBag.StatusFilter = status;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                var pagedOrders = orders
                    .OrderByDescending(x => x.NGAYDAT) // ✅ Sort by newest first
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return View(pagedOrders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Admin/Orders: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đơn hàng: " + ex.Message;

                // ✅ Return empty list with default ViewBag values
                ViewBag.TotalOrders = 0;
                ViewBag.PendingOrders = 0;
                ViewBag.ProcessingOrders = 0;
                ViewBag.CompletedOrders = 0;
                ViewBag.CancelledOrders = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 0;

                return View(new List<OrderViewModel>());
            }
        }

        // =========================================
        // GET: Admin/Users
        // =========================================
        [AuthorizeRole("ADMIN")]
        public ActionResult Users()
        {
            try
            {
                var users = _customerRepo.GetAllCustomersWithRoles();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }

        // =========================================
        // GET: Admin/Security
        // =========================================
        [AuthorizeRole("ADMIN")]
        public ActionResult Security()
        {
            try
            {
                ViewBag.FailedLogins = _customerRepo.GetFailedLoginCount();
                ViewBag.ActiveSessions = _customerRepo.GetActiveSessionCount();
                ViewBag.LastSecurityCheck = DateTime.Now;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }
    }
}