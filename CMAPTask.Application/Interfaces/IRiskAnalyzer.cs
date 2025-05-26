using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Application.Interfaces
{
    public interface IRiskAnalyzer
    {
        (RiskSummary, List<Transaction>) AnalyzeTransactions(List<Transaction> transactions);
    }
}
