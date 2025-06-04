using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Interfaces;
using OpenBanking.web.ViewModel;
using System.Security.Claims;

namespace OpenBanking.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly OBDbContext _db;
 

        public AccountController(OBDbContext db)
        {
            _db = db;        
          
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.SingleOrDefault(u => u.Email == model.Email && u.PasswordHash == model.Password);
          

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login.");
                return View(model);
            }

         

           var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("DisplayName", user.CompanyName),
            new Claim("UserId", user.UserId.ToString()),
            //new Claim("PendingCredit", credit.PendingCredit.ToString()),
            //new Claim("ActiveCredit", credit.ActiveCredit.ToString()),
            //new Claim("UsedCredit", credit.UsedCredit.ToString()),
            //new Claim("TotalCredit", credit.Quantity.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // Optional
        };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View(); // Create a simple AccessDenied.cshtml page
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
