using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Domain.Enums;
using OpenBanking.Infrastructure.Extensions;
using OpenBanking.web.ViewModel;
using static OpenBanking.Domain.Enums.Enum;

namespace CMAPTask.web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly OBDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyEndUserRepository _companyUserRepository;



        public CustomerController(OBDbContext context, IHttpContextAccessor httpContextAccessor, ICompanyEndUserRepository companyUserRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _companyUserRepository = companyUserRepository;
        }


        [Authorize(Roles = Constants.Roles.Admin)]
        [Route("Customer/dashboard")]
        public async Task<IActionResult> Index()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var users = await _companyUserRepository.GetAllAsync(userId, null);

            var model = users.Select(e => new RecentUserViewModel
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                Status = e.Status,
            }).OrderByDescending(a => a.CreatedAt).ToList();

            return View(model);
        }

        public IActionResult NewRequest()
        {
            return PartialView("_NewRequestPartial", new CustomerDetails());
        }

        // Receive form submission
        [HttpPost]
        public async Task<IActionResult> SubmitDetails(CustomerDetails details)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_NewRequestPartial", details);

                return RedirectToAction("NewRequest");
            }

            var userId = User.GetUserId();

            var entity = new CompanyEndUser
            {
                UserId = userId,
                FirstName = details.FirstName,
                LastName = details.LastName,
                Email = details.Email,
                PhoneNumber = details.PhoneNumber,
                Status = Status.pending,
            };

            var id = await _companyUserRepository.SaveAsync(entity);

            TempData["CustomerDetails"] = System.Text.Json.JsonSerializer.Serialize(details);

            // You can return JSON to trigger a redirect via JS
            return Json(new { redirectUrl = Url.Action("ShowInstitutions", "OpenBanking") });
        }
    }
}
