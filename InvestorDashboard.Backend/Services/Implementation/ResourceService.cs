using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ResourceService : IResourceService
    {
        public IEnumerable<TRecord> GetCsvRecords<TRecord>(string name, bool asStream = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            try
            {
                using (var reader = GetResourceTextReader(name))
                {
                    var result = new CsvReader(reader).GetRecords<TRecord>();

                    if (!asStream)
                    {
                        return result.ToArray();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while parsing CSV file.", ex);
            }
        }

        public TextReader GetResourceTextReader(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resource = GetType().Namespace + ".Data." + name;
            return new StreamReader(assembly.GetManifestResourceStream(resource));
        }

        public string GetResourceString(string name)
        {
            return GetResourceTextReader(name).ReadToEnd();
        }
    }
}
