using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class CsvService : ICsvService
    {
        public IEnumerable<TRecord> GetRecords<TRecord>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var assembly = Assembly.GetExecutingAssembly();

            var resource = GetType().Namespace + ".Data." + name;

            using (var stream = assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                var csv = new CsvReader(reader);
                return csv.GetRecords<TRecord>().ToArray();
            }
        }
    }
}
