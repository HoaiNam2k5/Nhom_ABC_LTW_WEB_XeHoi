using System;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models;
using WebCar.Filters;

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

        // =========================================
        // GET: Admin/Orders
        // =========================================
        public ActionResult Orders(string status, int page = 1)
        {
            try
            {
                var orders = _orderRepo.GetAllOrders(status);

                // Pagination
                int pageSize = 10;
                var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)orders.Count / pageSize);
                ViewBag.StatusFilter = status;

                return View(pagedOrders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
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