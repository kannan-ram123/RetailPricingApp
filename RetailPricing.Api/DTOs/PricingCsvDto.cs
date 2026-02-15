namespace RetailPricing.Api.DTOs
{
    public class PricingCsvDto
    {
        public int StoreId { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public DateTime PriceDate { get; set; }
    }
}
