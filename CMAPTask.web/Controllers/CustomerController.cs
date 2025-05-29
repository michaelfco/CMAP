using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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


        public CustomerController(OBDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        [Authorize(Roles = Constants.Roles.Admin)]
        public IActionResult Index()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Query the list for this user
            var users = _context.CompanyEndUsers
                .Where(e => e.UserId == userId && (e.IsDeleted == null || e.IsDeleted == 0))
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new RecentUserViewModel
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    CreatedAt = e.CreatedAt
                })
                .ToList();

            return View(users);
        }
    

        // Receive form submission
        [HttpPost]
        public async Task<IActionResult> SubmitDetails(CustomerDetails details)
        {
            if (!ModelState.IsValid)
            {
                return View("EnterDetails", details);
            }
            var userId = User.GetUserId();

            var newEndUser = new CompanyEndUser
            {
                EndUserId = Guid.NewGuid(),
                UserId = userId,
                FirstName = details.FirstName,
                LastName = details.LastName,
                Email = details.Email,
                PhoneNumber = details.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.CompanyEndUsers.Add(newEndUser);
            await _context.SaveChangesAsync();

            var id = newEndUser.EndUserId.ToString();

            // TODO: Save details to DB or session, then redirect to institutions page
            TempData["CustomerDetails"] = System.Text.Json.JsonSerializer.Serialize(details);

            return RedirectToAction("ShowInstitutions", "OpenBanking");
        }
    }
}
