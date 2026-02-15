using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailPricing.Api.Models
{
    public class UploadError
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UploadId { get; set; }

        public int RowNumber { get; set; }

        [Required]
        public string Error { get; set; } = string.Empty;

        public string? RawData { get; set; }
    }
}