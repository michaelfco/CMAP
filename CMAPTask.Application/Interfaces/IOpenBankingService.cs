using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Application.Interfaces
{
    public interface IOpenBankingService
    {
        Task<OBTokenResponse> UseTokenAsync();
        Task<List<InstitutionsResponse>> GetInstitutionsAsync(string accessToken, string country = "gb");
        Task<string> CreateConsentSessionAsync(string institutionId);
        Task<List<TransactionResponse>> GetTransactionsAsync(string accessToken);
        Task<string> ExchangeRefForAccessTokenAsync(string requisitionRef);
        Task<RequisitionResponse> CreateRequisitionAsync(string institutionId, string agreementId, string redirectUri);
        Task<(List<BankAccount> Accounts, string Status)> GetAccountsByRequisitionIdAsync(string requisitionId);
        Task<TransactionResponse> GetTransactionsByAccountIdAsync(string accountId);
        Task<AccountDetails> GetAccountDetailsAsync(string accountId);
    }
}
