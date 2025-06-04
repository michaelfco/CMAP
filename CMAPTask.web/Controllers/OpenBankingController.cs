using CMAPTask.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.Application.Interfaces;

namespace CMAPTask.web.Controllers
{
    public class OpenBankingController : Controller
    {
        private readonly IOpenBankingService _openBankingService;
        private readonly ICompanyEndUserRepository _companyUserRepo;

        public OpenBankingController(IOpenBankingService openBankingService, ICompanyEndUserRepository companyUserRepo)
        {
            _openBankingService = openBankingService;
            _companyUserRepo = companyUserRepo;
        }

        // Async GET action to call the token service
        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ShowInstitutions(string u, string c)
        {
            ViewBag.UserId = u;
            ViewBag.CompanyId = c;

            if (!Guid.TryParse(u, out Guid userId) || !Guid.TryParse(c, out Guid companyId))
            {
                return View("Invalid");
            }

            var companyUser = await _companyUserRepo.GetByEndUserIdAndPendingAsync(userId);

            if (companyUser != null)
            {
                var token = await _openBankingService.UseTokenAsync(c);
                var institutions = await _openBankingService.GetInstitutionsAsync(token.Access, "gb");


                return View("Institutions", institutions);
            }
            else
                return View("Invalid");
        }

        public async Task<IActionResult> Invalid()
        {
            return View();
        }


    }
}
