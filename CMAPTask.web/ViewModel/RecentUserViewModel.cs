﻿using OpenBanking.Application.DTOs;
using static OpenBanking.Domain.Enums.Enum;

namespace OpenBanking.web.ViewModel
{
    public class RecentUserViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Status? Status { get; set; }
        public Guid UserId { get; set; }
        public Guid EndUserId { get; set; }

        public CreditDto Credits { get; set; }
    }
}
