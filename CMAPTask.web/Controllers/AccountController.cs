using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.web.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var trimmedPassword = model.Password?.Trim() ?? string.Empty;
            var hashedPassword = HashPassword(trimmedPassword);
            System.Diagnostics.Debug.WriteLine($"Login Raw Input: '{trimmedPassword}' (Length: {trimmedPassword.Length})");
            System.Diagnostics.Debug.WriteLine($"Login Input Hash: {hashedPassword}");

            var user = _db.Users
                .AsEnumerable()
                .SingleOrDefault(u =>
                {
                    if (u.Email != model.Email) return false;
                    bool isBase64Match = u.PasswordHash == hashedPassword;
                    bool isPlaintextMatch = u.PasswordHash == trimmedPassword;
                    bool isHexMatch = IsHexHash(u.PasswordHash) && ConvertHexToBase64(u.PasswordHash) == hashedPassword;
                    if (isHexMatch)
                        System.Diagnostics.Debug.WriteLine($"Hex Hash: {u.PasswordHash}, Converted to Base64: {ConvertHexToBase64(u.PasswordHash)}");
                    return isBase64Match || isPlaintextMatch || isHexMatch;
                });

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"No user found or password mismatch for Email: {model.Email}");
                ModelState.AddModelError("", "Invalid login.");
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine($"User found: {user.Email}, Stored Hash: {user.PasswordHash}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("DisplayName", user.CompanyName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        private bool IsHexHash(string hash)
        {
            return !string.IsNullOrEmpty(hash) && hash.Length == 64 &&
                   hash.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'));
        }

        private string ConvertHexToBase64(string hex)
        {
            try
            {
                var bytes = Enumerable.Range(0, hex.Length / 2)
                    .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                    .ToArray();
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        [HttpGet, HttpPost]
        [Route("Account/Logout")] 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}