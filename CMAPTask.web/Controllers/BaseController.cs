using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenBanking.Application.Interfaces;
using System.Threading.Tasks;

namespace OpenBanking.web.Controllers;

public abstract class BaseController : Controller
{
    private readonly ICreditRepository _creditRepository;

    protected BaseController(ICreditRepository creditRepository)
    {
        _creditRepository = creditRepository;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                var credit = await _creditRepository.GetCreditUsage(userId);
                ViewBag.Active = credit?.ActiveCredit ?? 0;
                ViewBag.Pending = credit?.PendingCredit ?? 0;
            }
            else
            {
                ViewBag.Credit = null;
            }
        }
        else
        {
            ViewBag.Credit = null;
        }

        await next();
    }
}
