using Microsoft.AspNetCore.Mvc;
using RetailPricing.Api.Data;
using Microsoft.EntityFrameworkCore;
using RetailPricing.Api.Models;
using RetailPricing.Api.DTOs;

namespace RetailPricing.Api.Controllers
{
    [ApiController]
    [Route("api/pricing")]
    public class PricingSerachController : ControllerBase
    {
        private readonly RetailPricingDbDetailContext _Context;
        private const int MaxResults = 1000;

        public PricingSerachController(RetailPricingDbDetailContext context)
        {
            _Context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int? storeId,
            [FromQuery] string? sku,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            if (storeId.HasValue && storeId.Value <= 0)
                return BadRequest("storeId must be greater than zero.");

            if (!string.IsNullOrEmpty(sku) && sku.Length > 100)
                return BadRequest("sku length must be 100 characters or fewer.");

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                return BadRequest("fromDate must be earlier than or equal to toDate.");

            if (page <= 0 || pageSize <= 0 || pageSize > 1000) return BadRequest("Invalid paging parameters.");

            var query = _Context.PricingRecords.AsNoTracking().AsQueryable();

            if (storeId.HasValue) query = query.Where(x => x.StoreId == storeId.Value);
            if (!string.IsNullOrEmpty(sku)) query = query.Where(x => x.SKU == sku);
            if (fromDate.HasValue) query = query.Where(x => x.PriceDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.PriceDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var results = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return Ok(results);
        }

        [HttpPut("{pricingrecordId:long}")]
        public async Task<IActionResult> UpdatePrice(long pricingrecordId, [FromBody] UpdatePriceDto model, CancellationToken cancellationToken = default)
        {
            // ApiController will automatically return 400 if model validation fails.
            var record = await _Context.PricingRecords.FindAsync(new object[] { pricingrecordId }, cancellationToken);
            if (record == null) return NotFound();

            // Business rule: price must be reasonable
            if (model.Price <= 0) return BadRequest("Price must be greater than zero.");

            record.Price = model.Price;
            record.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // If using RowVersion/Timestamp, return 409
                return Conflict("The record was updated by another process. Please refresh and retry.");
            }

            return Ok(record);
        }
    }
}
