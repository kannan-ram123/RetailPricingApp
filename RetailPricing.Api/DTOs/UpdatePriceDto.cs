using System.ComponentModel.DataAnnotations;

namespace RetailPricing.Api.DTOs
{
    public sealed class UpdatePriceDto
    {
        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than zero and within allowed range.")]
        public decimal Price { get; set; }

        // Optional concurrency token (base64), use if you add RowVersion to the model.
        public string? RowVersion { get; set; }
    }
}