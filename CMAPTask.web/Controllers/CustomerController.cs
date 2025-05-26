using CMAPTask.Domain.Entities.OB;
using Microsoft.AspNetCore.Mvc;

namespace CMAPTask.web.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Receive form submission
        [HttpPost]
        public IActionResult SubmitDetails(CustomerDetails details)
        {
            if (!ModelState.IsValid)
            {
                return View("EnterDetails", details);
            }

            // TODO: Save details to DB or session, then redirect to institutions page
            TempData["CustomerDetails"] = System.Text.Json.JsonSerializer.Serialize(details);

            return RedirectToAction("ShowInstitutions", "OpenBanking");
        }
    }
}
