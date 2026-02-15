
using CsvHelper.Configuration;

namespace RetailPricing.Api.DTOs
{
    public sealed class PricingCsvMap : ClassMap<PricingCsvDto>
    {
        public PricingCsvMap()
        {
            Map(m => m.StoreId).Name("StoreId");
            Map(m => m.SKU).Name("SKU");
            Map(m => m.Price).Name("Price");
            // Ensure CsvHelper parses dates in dd-MM-yyyy format
            Map(m => m.PriceDate).Name("PriceDate").TypeConverterOption.Format("dd-MM-yyyy");
        }
    }
}