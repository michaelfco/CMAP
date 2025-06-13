using Castle.Core.Resource;
using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure;
using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Domain.Enums;
using OpenBanking.Infrastructure.Extensions;
using OpenBanking.Infrastructure.Services;
using OpenBanking.web.Controllers;
using OpenBanking.web.ViewModel;
using static OpenBanking.Domain.Enums.Enum;

namespace CMAPTask.web.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly OBDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyEndUserRepository _companyUserRepository;
        private readonly ICreditRepository _creditRepository;
        private readonly OBSettings _settings;
        private readonly EmailService _emailService;



        public CustomerController(OBDbContext context, IHttpContextAccessor httpContextAccessor, ICompanyEndUserRepository companyUserRepository, IOptions<OBSettings> options, EmailService emailService, ICreditRepository creditRepository) : base(creditRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _companyUserRepository = companyUserRepository;
            _settings = options.Value;
            _emailService = emailService;
            _creditRepository = creditRepository;
        }


        [Authorize]
        [Route("Customer/dashboard")]
        public async Task<IActionResult> Index()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var users = await _companyUserRepository.GetAllAsync(userId, null);
            var credit = await _creditRepository.GetCreditUsage(userId);

            var model = users.Select(e => new RecentUserViewModel
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                Status = e.Status,
                UserId = e.UserId,
                EndUserId = e.EndUserId,
                Credits = credit
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
            try
            {
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

                var creditUsage = new CreditUsage
                {
                    UserId = userId,
                    EndUserId = id,
                    Status = Status.pending

                };

                await _creditRepository.AddPendingCreditUsageAsync(creditUsage);

                var urlToSend = $"{_settings.SiteBaseURL}OpenBanking/ShowInstitutions?u={id}&c={userId}";

                var displayName = User.Claims.FirstOrDefault(c => c.Type == "DisplayName")?.Value;

                var htmlBody = GenerateConsentEmailHtml(details.FirstName, urlToSend, displayName, "https://openvista.io/img/LogoBlueGraphics.png");
                await _emailService.SendEmailAsync(details.Email, "Please Provide Your Consent", htmlBody);


                TempData["MsgSuccess"] = "Customer details submitted successfully!";
                TempData["CustomerDetails"] = System.Text.Json.JsonSerializer.Serialize(details);
                return Json(new
                {
                    success = true,
                    message = "Customer details submitted successfully!"
                });
            }
            catch (Exception ex) {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });

            }



        }


        public static string GenerateConsentEmailHtml(string customerName, string consentLink, string displayCompanyName, string logoUrl)
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
                                <h2>Action Required: Provide Your Consent</h2>
                            </div>
                            <div class='content'>
                                <p>Dear {customerName},</p>

                                <p>You’ve been invited to complete a request with us. Before we can proceed with the evaluation, we need your permission to securely access your financial data.</p>

                                <p>This step is essential for us to analyze your information and provide you with the best possible offer.</p>

                                <p>Please click the button below to provide your consent:</p>

                                <a href='{consentLink}' class='btn'>Provide Consent</a>

                                <p>If the button doesn’t work, copy and paste the following link into your browser:</p>
                                <p><a href='{consentLink}'>{consentLink}</a></p>

                                <p>Thank you for choosing us.</p>
                                <p>Best regards,<br>{displayCompanyName}</p>
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
