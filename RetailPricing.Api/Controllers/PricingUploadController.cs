using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPricing.Api.Data;
using RetailPricing.Api.Services;
using RetailPricing.Api.Models;

namespace RetailPricing.Api.Controllers
{
    [ApiController]
    [Route("api/pricing/upload")]
    public class PricingUploadController : ControllerBase
    {
        private readonly ICsvUploadService _service;
        private readonly RetailPricingDbDetailContext _context;

        public PricingUploadController(ICsvUploadService service, RetailPricingDbDetailContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> UploadPricingCsv(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var batchId = await _service.ProcessPricingCsvAsync(file);
            return Ok(new { BatchId = batchId });
        }

        // GET api/pricing/upload/{uploadId}
        [HttpGet("{uploadId:guid}")]
        public async Task<IActionResult> GetUploadStatus([FromRoute] Guid uploadId, CancellationToken cancellationToken = default)
        {
            var history = await _context.UploadHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.UploadId == uploadId, cancellationToken);

            if (history == null) return NotFound();
            return Ok(history);
        }

        // GET api/pricing/upload/{uploadId}/errors
        [HttpGet("{uploadId:guid}/errors")]
        public async Task<IActionResult> DownloadUploadErrors([FromRoute] Guid uploadId, CancellationToken cancellationToken = default)
        {
            var historyExists = await _context.UploadHistories
                .AsNoTracking()
                .AnyAsync(h => h.UploadId == uploadId, cancellationToken);

            if (!historyExists) return NotFound();

            var errors = await _context.UploadErrors
                .AsNoTracking()
                .Where(e => e.UploadId == uploadId)
                .OrderBy(e => e.RowNumber)
                .ToListAsync(cancellationToken);

            if (!errors.Any())
                return NoContent();

            await using var mem = new MemoryStream();
            await using var writer = new StreamWriter(mem, Encoding.UTF8, leaveOpen: true);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteHeader<UploadError>();
                await csv.NextRecordAsync();

                foreach (var err in errors)
                {
                    csv.WriteRecord(err);
                    await csv.NextRecordAsync();
                }
            }

            mem.Position = 0;
            var fileName = $"upload_{uploadId}_errors.csv";
            return File(mem.ToArray(), "text/csv", fileName);
        }
    }
}
