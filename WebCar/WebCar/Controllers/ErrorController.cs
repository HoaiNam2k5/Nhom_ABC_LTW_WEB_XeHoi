using System.Web.Mvc;

namespace WebCar.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error/Index (500)
        public ActionResult Index()
        {
            Response.StatusCode = 500;
            return View();
        }

        // GET: Error/NotFound (404)
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        // GET: Error/Forbidden (403)
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View("~/Views/Account/AccessDenied.cshtml");
        }
    }
}