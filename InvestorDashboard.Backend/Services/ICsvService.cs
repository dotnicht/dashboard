using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services
{
    public interface ICsvService
    {
        IEnumerable<TRecord> GetRecords<TRecord>(string name);
    }
}
