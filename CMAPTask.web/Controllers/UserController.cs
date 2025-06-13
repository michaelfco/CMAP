using CMAPTask.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Common.Helpers;
using OpenBanking.Application.Common.Models;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Infrastructure.Extensions;
using OpenBanking.Infrastructure.Services;
using OpenBanking.web.ViewModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.web.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ICreditRepository _creditRepository;
        private readonly EmailService _emailService;
        private readonly OBSettings _settings;

        public UserController(
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            EmailService emailService,
            IOptions<OBSettings> options,
            ICreditRepository creditRepository) : base(creditRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _emailService = emailService;
            _settings = options.Value;
            _creditRepository = creditRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchQuery = "", int page = 1, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var users = await _userRepository.GetAllAsync(userId);

            var userViewModels = users.Select(e => new UserViewModel
            {
                UserId = e.UserId,
                CompanyName = e.CompanyName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                Role = e.Role,
                CreatedAt = e.CreatedAt,
                UseCredentialId = e.UseCredentialId,
                Environment = e.Environment,
                GoCardlessConfigId = e.GoCardlessConfigId
            });

            if (fromDate.HasValue)
            {
                userViewModels = userViewModels.Where(u => u.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                userViewModels = userViewModels.Where(u => u.CreatedAt <= toDate.Value);
            }

            var pagedResult = PaginationHelper.GetPaged(
                userViewModels,
                page,
                15,
                searchQuery,
                u => u.CompanyName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            );

            pagedResult.FromDate = fromDate;
            pagedResult.ToDate = toDate;

            return View(pagedResult);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var goCardlessSettings = await _userRepository.GetGoCardlessSettingsAsync();
            var settingsOptions = goCardlessSettings.Select(g => new SelectListItem
            {
                Value = g.ConfigId.ToString(),
                Text = $"{g.Environment} ({g.ConfigId})"
            }).ToList();
            settingsOptions.Insert(0, new SelectListItem { Value = "", Text = "Select GoCardless Settings" });

            if (id.HasValue && id != Guid.Empty)
            {
                var user = await _userRepository.GetByUserIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound();
                }
                user.PasswordHash = null;
                return View(new UserEditViewModel
                {
                    UserId = user.UserId,
                    CompanyName = user.CompanyName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    PostalCode = user.PostalCode,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UseCredentialId = user.UseCredentialId,
                    GoCardlessSettingsOptions = settingsOptions
                });
            }

            return View(new UserEditViewModel
            {
                GoCardlessSettingsOptions = settingsOptions
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(UserEditViewModel model)
        {
            var goCardlessSettings = await _userRepository.GetGoCardlessSettingsAsync();
            model.GoCardlessSettingsOptions = goCardlessSettings.Select(g => new SelectListItem
            {
                Value = g.ConfigId.ToString(),
                Text = $"{g.Environment} ({g.ConfigId})"
            }).ToList();
            model.GoCardlessSettingsOptions.Insert(0, new SelectListItem { Value = "", Text = "Select GoCardless Settings" });

            // Clear all PasswordHash-related validation errors
            ModelState.Remove("PasswordHash");
            if (ModelState.ContainsKey("user.PasswordHash"))
            {
                ModelState.Remove("user.PasswordHash");
            }

            var existingUserWithEmail = await _userRepository.GetByEmailAsync(model.Email);
            if (model.UserId == Guid.Empty && existingUserWithEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View("Edit", model);
            }
            else if (model.UserId != Guid.Empty && existingUserWithEmail != null && existingUserWithEmail.UserId != model.UserId)
            {
                ModelState.AddModelError("Email", "This email address is already in use by another user.");
                return View("Edit", model);
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var user = new User
            {
                UserId = model.UserId,
                CompanyName = model.CompanyName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode,
                Role = model.Role,
                CreatedAt = model.CreatedAt,
                UseCredentialId = model.UseCredentialId
            };

            string generatedPassword = null;
            if (user.UserId == Guid.Empty)
            {
                user.UserId = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                if (user.UseCredentialId == Guid.Empty)
                {
                    user.UseCredentialId = Guid.NewGuid();
                }
                generatedPassword = GenerateRandomPassword();
                user.PasswordHash = HashPassword(generatedPassword);
                await _userRepository.SaveAsync(user);

                var htmlBody = GenerateWelcomeEmailHtml(user.CompanyName, user.Email, generatedPassword, _settings.SiteBaseURL, "https://openvista.io/img/Version2-OpenVista-Logo.svg");
                await _emailService.SendEmailAsync(user.Email, "Welcome to Our Platform", htmlBody);

                TempData["MsgSuccess"] = "User created successfully!";
            }
            else
            {
                var existingUser = await _userRepository.GetByUserIdAsync(user.UserId);
                if (string.IsNullOrWhiteSpace(model.PasswordHash))
                {
                    user.PasswordHash = existingUser.PasswordHash; // Retain existing password
                }
                else
                {
                    user.PasswordHash = HashPassword(model.PasswordHash);
                }
                await _userRepository.UpdateAsync(user);
                TempData["MsgSuccess"] = "User updated successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserProfileViewModel
            {
                User = user,
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => $"{x.ErrorMessage} (Key: {x.Exception?.Message})");
                System.Diagnostics.Debug.WriteLine("ModelState Errors: " + string.Join("; ", errors));
                var viewModel = new UserProfileViewModel { User = user, ChangePassword = model };
                return View("Profile", viewModel);
            }

            System.Diagnostics.Debug.WriteLine($"Raw Input: '{model.CurrentPassword}' (Length: {model.CurrentPassword?.Length})");
            var hashedInput = HashPassword(model.CurrentPassword);
            System.Diagnostics.Debug.WriteLine($"Input Hash: {hashedInput}");
            System.Diagnostics.Debug.WriteLine($"Stored Hash: {user.PasswordHash}");

            bool isPasswordValid = string.Equals(hashedInput, user.PasswordHash, StringComparison.Ordinal) ||
                                  string.Equals(model.CurrentPassword?.Trim(), user.PasswordHash, StringComparison.Ordinal);

            if (!isPasswordValid)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                var viewModel = new UserProfileViewModel { User = user, ChangePassword = model };
                return View("Profile", viewModel);
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            await _userRepository.UpdateAsync(user);

            TempData["MsgSuccess"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            var password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random[i] % chars.Length];
            }
            return new string(password);
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            password = password.Trim();
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        private string GenerateWelcomeEmailHtml(string companyName, string username, string password, string siteBaseUrl, string logoUrl)
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
                            <h2>Welcome to Our Platform</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {companyName},</p>
                            <p>Your account has been successfully created. Below are your login credentials:</p>
                            <p><strong>Username:</strong> {username}</p>
                            <p><strong>Password:</strong> {password}</p>
                            <p>Please use these credentials to log in to our platform:</p>
                            <a href='{siteBaseUrl}' class='btn'>Log In Now</a>
                            <p>If the button doesn’t work, copy and paste the following link into your browser:</p>
                            <p><a href='{siteBaseUrl}'>{siteBaseUrl}</a></p>
                            <p>For security, please change your password after your first login.</p>
                            <p>Thank you for choosing us.</p>
                            <p>Best regards,<br>Open Vista</p>
                        </div>
                        <div class='footer'>
                            <p>This email was sent automatically. Please do not reply to it.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}