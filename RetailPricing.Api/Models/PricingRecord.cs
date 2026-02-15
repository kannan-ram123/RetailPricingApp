using System.ComponentModel.DataAnnotations;

namespace RetailPricing.Api.Models
{
    public class PricingRecord
    {
        [Key]
        public long PricingRecordId { get; set; }

        [Range(1, int.MaxValue)]
        public int StoreId { get; set; }

        [Required, StringLength(200)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.0, 10000000.0)]
        public decimal Price { get; set; }

        public DateTime PriceDate { get; set; }

        public Guid? UploadBatchID { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
