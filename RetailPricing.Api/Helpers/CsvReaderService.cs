
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using RetailPricing.Api.DTOs;

namespace RetailPricing.Api.Helpers
{
    public static class CsvReaderService
    {
        public static IList<PricingCsvDto> ReadPricingCsv(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<PricingCsvMap>();
            return csv.GetRecords<PricingCsvDto>().ToList();
        }
    }
}