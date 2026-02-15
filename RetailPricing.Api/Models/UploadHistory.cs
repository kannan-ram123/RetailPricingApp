using System;
using System.ComponentModel.DataAnnotations;
namespace RetailPricing.Api.Models
{
    public class UploadHistory
    {
        [Key]
        public Guid UploadId { get; set; }

        [Required, StringLength(300)]
        public string FileName { get; set; } = string.Empty;

        public string? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public int? TotalRecords { get; set; }
        public int? FailedRecords { get; set; }

        public string? Remarks { get; set; }
    }
}
