using CMAPTask.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CMAPTask.web.Controllers
{
    public class OpenBankingController : Controller
    {
        private readonly IOpenBankingService _openBankingService;

        public OpenBankingController(IOpenBankingService openBankingService)
        {
            _openBankingService = openBankingService;
        }

        // Async GET action to call the token service
        public async Task<IActionResult> Index()
        {
           return View();
        }
        
        public async Task<IActionResult> ShowInstitutions()
        {
            var token = await _openBankingService.UseTokenAsync();
            var institutions = await _openBankingService.GetInstitutionsAsync(token.Access, "gb");

            return View("Institutions", institutions);
        }   


    }
}
