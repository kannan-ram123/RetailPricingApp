using System;
using System.ComponentModel.DataAnnotations;

namespace RetailPricing.Api.Models
{
    public class Stores
    {
        [Key]
        public int StoreId { get; set; }

        [Required, StringLength(200)]
        public string StoreName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public string? Region { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
