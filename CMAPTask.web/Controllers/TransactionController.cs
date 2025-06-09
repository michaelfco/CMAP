using CMAPTask.Infrastructure;
using CMAPTask.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Common.Helpers;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Infrastructure.Repository;
using OpenBanking.Infrastructure.Services;
using OpenBanking.web.ViewModel;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.web.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly OBDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyEndUserRepository _companyUserRepository;
        private readonly ICreditRepository _creditRepository;
        private readonly OBSettings _settings;

        public TransactionController(OBDbContext context, IHttpContextAccessor httpContextAccessor, ICompanyEndUserRepository companyUserRepository, IOptions<OBSettings> options, ICreditRepository creditRepository) : base(creditRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _companyUserRepository = companyUserRepository;
            _settings = options.Value;
            _creditRepository = creditRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string searchQuery = "", int page = 1, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var users = await _companyUserRepository.GetAllAsync(userId, null);

            var userViewModels = users.Select(e => new RecentUserViewModel
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                Status = e.Status,
                UserId = e.UserId,
                EndUserId = e.EndUserId
            }).Where(u => u.Status == Status.pending);

            // Apply date filtering
            if (fromDate.HasValue)
                userViewModels = userViewModels.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                userViewModels = userViewModels.Where(u => u.CreatedAt <= toDate.Value);

            var pagedResult = PaginationHelper.GetPaged(
                userViewModels,
                page,
                15,
                searchQuery,
                u => $"{u.FirstName} {u.LastName}".Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            );

            pagedResult.FromDate = fromDate;
            pagedResult.ToDate = toDate;

            return View(pagedResult);
        }

        [Authorize]
        public async Task<IActionResult> Complete(string searchQuery = "", int page = 1, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var users = await _companyUserRepository.GetAllAsync(userId, null);

            var userViewModels = users.Select(e => new RecentUserViewModel
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                CreatedAt = e.CreatedAt,
                Status = e.Status,
                UserId = e.UserId,
                EndUserId = e.EndUserId
            }).Where(u => u.Status == Status.Complete);

            // Apply date filtering
            if (fromDate.HasValue)
                userViewModels = userViewModels.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                userViewModels = userViewModels.Where(u => u.CreatedAt <= toDate.Value);

            var pagedResult = PaginationHelper.GetPaged(
                userViewModels,
                page,
                15,
                searchQuery,
                u => $"{u.FirstName} {u.LastName}".Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            );

            pagedResult.FromDate = fromDate;
            pagedResult.ToDate = toDate;

            return View(pagedResult);
        }
    }
}
