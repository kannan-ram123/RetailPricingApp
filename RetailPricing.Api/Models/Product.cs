using System;
using System.ComponentModel.DataAnnotations;

namespace RetailPricing.Api.Models
{
    public class Product
    {
        [Key]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string ProductName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
