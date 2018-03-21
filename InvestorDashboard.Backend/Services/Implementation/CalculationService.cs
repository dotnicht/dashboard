using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class CalculationService : ICalculationService
    {
        private readonly Dictionary<Currency, IOptions<CryptoSettings>> _settings;

        public CalculationService(IOptions<EthereumSettings> ethereumSettings, IOptions<BitcoinSettings> bitcoinSettings)
        {
            _settings = new Dictionary<Currency, IOptions<CryptoSettings>>
            {
                { ethereumSettings.Value.Currency, ethereumSettings },
                { bitcoinSettings.Value.Currency, bitcoinSettings }
            };
        }

        public decimal ToDecimalValue(string value, Currency currency)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // TODO: check this.
            var bi = BigInteger.Parse(value);
            var result = Math.Exp(BigInteger.Log(bi) - BigInteger.Log(new BigInteger(Math.Pow(10, _settings[currency].Value.Denomination ))));
            return Convert.ToDecimal(result);
        }

        public string ToStringValue(decimal value, Currency currency)
        {
            return new BigInteger((double)value * Math.Pow(10, _settings[currency].Value.Denomination)).ToString();
        }
    }
}
