using System;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models;
using WebCar.Filters;

namespace WebCar.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly CarRepository _carRepo;

        public OrderController()
        {
            _orderRepo = new OrderRepository();
            _carRepo = new CarRepository();
        }

        // =========================================
        // GET: Order/Create/{carId}
        // =========================================
        public ActionResult Create(int carId)
        {
            try
            {
                // Get car details
                var car = _carRepo.GetCarById(carId);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe";
                    return RedirectToAction("Index", "Product");
                }

                // Check if car is available
                if (car.TRANGTHAI != "Còn hàng")
                {
                    TempData["ErrorMessage"] = "Xe này hiện không còn hàng";
                    return RedirectToAction("Details", "Product", new { id = carId });
                }

                // Get customer info
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", new { carId = carId }) });
                }

                ViewBag.Car = car;
                ViewBag.CustomerId = (int)Session["CustomerId"];
                ViewBag.CustomerName = Session["CustomerName"]?.ToString();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index", "Product");
            }
        }

        // =========================================
        // POST: Order/Create
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int carId, int quantity, string diachi, string sdt, string ghichu)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];

                // Get car details
                var car = _carRepo.GetCarById(carId);
                if (car == null || car.TRANGTHAI != "Còn hàng")
                {
                    TempData["ErrorMessage"] = "Xe không còn hàng";
                    return RedirectToAction("Index", "Product");
                }

                // Create order
                var result = _orderRepo.CreateOrder(customerId, carId, quantity, diachi, sdt, ghichu);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Details", new { id = result.OrderId });
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Create", new { carId = carId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index", "Product");
            }
        }

        // =========================================
        // GET: Order/MyOrders
        // =========================================
        public ActionResult MyOrders()
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                var orders = _orderRepo.GetOrdersByCustomer(customerId);

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }

        // =========================================
        // GET: Order/Details/{id}
        // =========================================
        public ActionResult Details(int id)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // ✅ Get order
                var order = _orderRepo.GetOrderById(id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("MyOrders");
                }

                // ✅ Verify ownership (non-admin)
                int customerId = (int)Session["CustomerId"];
                string role = Session["RoleName"]?.ToString();

                if (role != "ADMIN" && role != "MANAGER" && order.MAKH != customerId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này";
                    return RedirectToAction("MyOrders");
                }

                // ✅ Get order details
                ViewBag.OrderDetails = _orderRepo.GetOrderDetails(id);

                // ✅ DEBUG
                System.Diagnostics.Debug.WriteLine($"Order ID: {order.MADON}");
                System.Diagnostics.Debug.WriteLine($"Customer: {order.CustomerName}");
                System.Diagnostics.Debug.WriteLine($"Details Count: {ViewBag.OrderDetails.Count}");

                return View(order);
            }
            catch (Exception ex)
            {
                // ✅ LOG ERROR
                System.Diagnostics.Debug.WriteLine($"Error in Order/Details: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("MyOrders");
            }
        }

        // =========================================
        // POST: Order/Cancel/{id}
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                var result = _orderRepo.CancelOrder(id, customerId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("MyOrders");
            }
        }

        // =========================================
        // POST: Order/UpdateStatus (ADMIN/MANAGER)
        // =========================================
        [HttpPost]
        [AuthorizeRole("ADMIN", "MANAGER")]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateStatus(int orderId, string status)
        {
            try
            {
                var result = _orderRepo.UpdateOrderStatus(orderId, status);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }
    }
}