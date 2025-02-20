using Microsoft.AspNetCore.Mvc;

namespace CMAPTask.web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
