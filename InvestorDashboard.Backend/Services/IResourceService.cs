using System.Collections.Generic;
using System.IO;

namespace InvestorDashboard.Backend.Services
{
    public interface IResourceService
    {
        IEnumerable<TRecord> GetCsvRecords<TRecord>(string name, bool asStream = false);
        TextReader GetResourceTextReader(string name);
    }
}
