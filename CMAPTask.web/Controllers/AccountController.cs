using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.Application.Common.Helpers;
using OpenBanking.web.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CMAPTask.Infrastructure;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Infrastructure.Services;

namespace OpenBanking.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly OBDbContext _db;
        private readonly EmailService _emailService;
        private readonly OBSettings _settings;

        public AccountController(OBDbContext db, EmailService emailService, IOptions<OBSettings> options)
        {
            _db = db;
            _emailService = emailService;
            _settings = options.Value;
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

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.SingleOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                var token = GenerateResetToken();
                var resetToken = new PasswordResetToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Token = token,
                    ExpiryDate = DateTime.UtcNow.AddHours(1)
                };

                _db.PasswordResetTokens.Add(resetToken);
                await _db.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
                var emailBody = GenerateResetEmailHtml(user.CompanyName, resetLink, _settings.SiteBaseURL, "https://openvista.io/img/LogoBlueGraphics.png");
                await _emailService.SendEmailAsync(user.Email, "Password Reset Request", emailBody);
            }

            TempData["MsgSuccess"] = "If an account exists with that email, a password reset link has been sent.";
            return RedirectToAction("Login");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("Error", new ErrorViewModel { Message = "Invalid password reset token." });
            }

            var resetToken = _db.PasswordResetTokens
                .SingleOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                return View("Error", new ErrorViewModel { Message = "Invalid or expired password reset token." });
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetToken = _db.PasswordResetTokens
                .SingleOrDefault(t => t.Token == model.Token && t.ExpiryDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                ModelState.AddModelError("", "Invalid or expired reset token.");
                return View(model);
            }

            var user = _db.Users.SingleOrDefault(u => u.UserId == resetToken.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            _db.PasswordResetTokens.Remove(resetToken);
            await _db.SaveChangesAsync();

            TempData["MsgSuccess"] = "Password reset successfully. Please log in.";
            return RedirectToAction("Login");
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

        private string GenerateResetToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private string GenerateResetEmailHtml(string companyName, string resetLink, string siteBaseUrl, string logoUrl)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{
                            font-family: 'Segoe UI', sans-serif;
                            background-color: #f8f9fa;
                            padding: 20px;
                            color: #333;
                        }}
                        .container {{
                            background-color: #ffffff;
                            max-width: 600px;
                            margin: auto;
                            padding: 30px;
                            border-radius: 10px;
                            box-shadow: 0 0 10px rgba(0,0,0,0.05);
                        }}
                        .logo {{
                            text-align: center;
                            margin-bottom: 20px;
                        }}
                        .logo img {{
                            max-width: 150px;
                        }}
                        .header {{
                            border-bottom: 1px solid #dee2e6;
                            margin-bottom: 20px;
                            text-align: center;
                        }}
                        .header h2 {{
                            color: #0d6efd;
                        }}
                        .content p {{
                            line-height: 1.6;
                        }}
                        .btn {{
                            display: inline-block;
                            margin-top: 20px;
                            padding: 12px 25px;
                            background-color: #0d6efd;
                            color: white !important;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                        }}
                        .footer {{
                            margin-top: 30px;
                            font-size: 0.85em;
                            color: #6c757d;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='logo'>
                            <img src='{logoUrl}' alt='Company Logo' width='150' />
                        </div>
                        <div class='header'>
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {companyName},</p>
                            <p>We received a request to reset your password. Click the button below to set a new password:</p>
                            <a href='{resetLink}' class='btn'>Reset Password</a>
                            <p>If the button doesn’t work, copy and paste the following link into your browser:</p>
                            <p><a href='{resetLink}'>{resetLink}</a></p>
                            <p>This link will expire in 1 hour for security reasons. If you didn’t request a password reset, please ignore this email.</p>
                            <p>Thank you,<br>Your Lending Partner</p>
                        </div>
                        <div class='footer'>
                            <p>This email was sent automatically. Please do not reply to it.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        [HttpGet,HttpPost]
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