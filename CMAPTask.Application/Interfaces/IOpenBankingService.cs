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
        Task<OBTokenResponse> UseTokenAsync(string c);
        Task<List<InstitutionsResponse>> GetInstitutionsAsync(string accessToken, string country = "gb");
        Task<string> CreateConsentSessionAsync(string institutionId, string cu);
        Task<List<TransactionResponse>> GetTransactionsAsync(string accessToken);
        Task<string> ExchangeRefForAccessTokenAsync(string requisitionRef, string c);
        Task<RequisitionResponse> CreateRequisitionAsync(string institutionId, string agreementId, string redirectUri, string c);
        Task<(List<BankAccount> Accounts, string Status)> GetAccountsByRequisitionIdAsync(string requisitionId, string c);
        Task<TransactionResponse> GetTransactionsByAccountIdAsync(string accountId, string c);
        Task<AccountDetails> GetAccountDetailsAsync(string accountId, string c);
    }
}
