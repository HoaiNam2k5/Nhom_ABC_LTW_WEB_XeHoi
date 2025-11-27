using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models;
using WebCar.Filters;

namespace WebCar.Controllers
{
    public class ProductController : Controller
    {
        private readonly CarRepository _carRepo;

        public ProductController()
        {
            _carRepo = new CarRepository();
        }

        // =========================================
        // GET: Product/Index (Danh sách xe)
        // =========================================
        public ActionResult Index(string search, string brand, decimal? minPrice, decimal? maxPrice, short? year, int page = 1)
        {
            try
            {
                var cars = _carRepo.GetAllCars(search, brand, minPrice, maxPrice, year);

                // Pagination
                int pageSize = 12;
                var pagedCars = cars.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)cars.Count / pageSize);
                ViewBag.TotalCars = cars.Count;

                // Filter values
                ViewBag.SearchTerm = search;
                ViewBag.BrandFilter = brand;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.YearFilter = year;

                // Get brands for filter
                ViewBag.Brands = _carRepo.GetBrands();

                return View(pagedCars);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách xe: " + ex.Message;
                return View();
            }
        }

        // =========================================
        // GET: Product/Details/{id}
        // =========================================
        public ActionResult Details(int id)
        {
            try
            {
                var car = _carRepo.GetCarById(id);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe";
                    return RedirectToAction("Index");
                }

                // Get related cars
                ViewBag.RelatedCars = _carRepo.GetRelatedCars(car.MAXE, car.HANGXE, 4);

                return View(car);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // =========================================
        // GET: Product/Create (Thêm xe mới - ADMIN)
        // =========================================
        [Authorize]
        [AuthorizeRole("ADMIN", "MANAGER")]
        public ActionResult Create()
        {
            ViewBag.Brands = _carRepo.GetBrands();
            return View();
        }

        // =========================================
        // POST: Product/Create
        // =========================================
        [HttpPost]
        [Authorize]
        [AuthorizeRole("ADMIN", "MANAGER")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CAR car, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string uploadPath = Server.MapPath("~/images/cars/");

                        // Create directory if not exists
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(filePath);

                        car.HINHANH = "images/cars/" + fileName;
                    }
                    else
                    {
                        car.HINHANH = "images/cars/default.jpg";
                    }

                    var result = _carRepo.CreateCar(car);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = "Thêm xe thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }

                ViewBag.Brands = _carRepo.GetBrands();
                return View(car);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                ViewBag.Brands = _carRepo.GetBrands();
                return View(car);
            }
        }

        // =========================================
        // GET: Product/Edit/{id}
        // =========================================
        [Authorize]
        [AuthorizeRole("ADMIN", "MANAGER")]
        public ActionResult Edit(int id)
        {
            try
            {
                var car = _carRepo.GetCarById(id);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe";
                    return RedirectToAction("Index");
                }

                ViewBag.Brands = _carRepo.GetBrands();
                return View(car);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // =========================================
        // POST: Product/Edit/{id}
        // =========================================
        [HttpPost]
        [Authorize]
        [AuthorizeRole("ADMIN", "MANAGER")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CAR car, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string uploadPath = Server.MapPath("~/images/cars/");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(filePath);

                        car.HINHANH = "images/cars/" + fileName;
                    }

                    var result = _carRepo.UpdateCar(car);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật xe thành công!";
                        return RedirectToAction("Details", new { id = car.MAXE });
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }

                ViewBag.Brands = _carRepo.GetBrands();
                return View(car);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                ViewBag.Brands = _carRepo.GetBrands();
                return View(car);
            }
        }

        // =========================================
        // POST: Product/Delete/{id}
        // =========================================
        [HttpPost]
        [Authorize]
        [AuthorizeRole("ADMIN")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _carRepo.DeleteCar(id);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Xóa xe thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}