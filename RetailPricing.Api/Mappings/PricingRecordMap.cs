using CsvHelper.Configuration;
using RetailPricing.Api.Models;
namespace RetailPricing.Api.Mappings
{
    public sealed class PricingRecordMap: ClassMap<PricingRecord>
    {
        public PricingRecordMap()
        {
            Map(m => m.StoreId);
            Map(m => m.Price);
            Map(m => m.PriceDate)
                .TypeConverterOption
                .Format("dd-MM-yyyy");
        }
    }
}
