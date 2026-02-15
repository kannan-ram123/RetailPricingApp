using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using RetailPricing.Api.Data;
using RetailPricing.Api.DTOs;
using RetailPricing.Api.Models;

namespace RetailPricing.Api.Services
{
    public class CsvUploadService : ICsvUploadService
    {
        private readonly RetailPricingDbDetailContext _Context;
        private const int BatchSize = 500;
        private static readonly string[] AllowedDateFormats = { "dd-MM-yyyy", "d-M-yyyy" };

        public CsvUploadService(RetailPricingDbDetailContext context)
        {
            _Context = context;
        }

        // Interface-compatible overload delegates to the implementation that accepts CancellationToken.
        public Task<Guid> ProcessPricingCsvAsync(IFormFile csvFile)
        {
            return ProcessPricingCsvAsync(csvFile, CancellationToken.None);
        }

        public async Task<Guid> ProcessPricingCsvAsync(IFormFile csvFile, CancellationToken cancellationToken = default)
        {
            if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));

            var uploadId = Guid.NewGuid();
            var uploadHistory = new UploadHistory
            {
                UploadId = uploadId,
                FileName = csvFile.FileName,
                UploadedAt = DateTime.UtcNow,
                Status = "Processing",
                TotalRecords = 0,
                FailedRecords = 0
            };

            await _Context.UploadHistories.AddAsync(uploadHistory, cancellationToken);
            await _Context.SaveChangesAsync(cancellationToken);

            var errors = new List<UploadError>();
            var buffer = new List<PricingRecord>(BatchSize);
            var total = 0;
            var rowNumber = 0;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            await using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            try
            {
                await csv.ReadAsync();
                csv.ReadHeader();
            }
            catch (Exception ex)
            {
                uploadHistory.Status = "Failed";
                uploadHistory.FailedRecords = 0;
                await _Context.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Invalid CSV header or file.", ex);
            }

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                rowNumber++;
                total++;

                try
                {
                    var storeIdField = csv.GetField("StoreId");
                    var sku = csv.GetField("SKU");
                    var priceField = csv.GetField("Price");
                    var priceDateField = csv.GetField("PriceDate");

                    if (!int.TryParse(storeIdField, NumberStyles.Integer, CultureInfo.InvariantCulture, out var storeId))
                    {
                        errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = "Invalid StoreId", RawData = csv.Parser.RawRecord });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(sku))
                    {
                        errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = "SKU is empty", RawData = csv.Parser.RawRecord });
                        continue;
                    }

                    if (!decimal.TryParse(priceField, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
                    {
                        errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = "Invalid Price", RawData = csv.Parser.RawRecord });
                        continue;
                    }

                    if (price < 0m)
                    {
                        errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = "Price must be non-negative", RawData = csv.Parser.RawRecord });
                        continue;
                    }

                    if (!DateTime.TryParseExact(priceDateField, AllowedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var priceDate))
                    {
                        errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = $"Invalid PriceDate (expected formats: {string.Join(",", AllowedDateFormats)})", RawData = csv.Parser.RawRecord });
                        continue;
                    }

                    buffer.Add(new PricingRecord
                    {
                        StoreId = storeId,
                        SKU = sku.Trim(),
                        Price = price,
                        PriceDate = priceDate.Date,
                        UploadBatchID = uploadId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(new UploadError { UploadId = uploadId, RowNumber = rowNumber, Error = ex.Message, RawData = csv.Parser.RawRecord });
                }

                // Flush buffer when full
                if (buffer.Count >= BatchSize)
                {
                    await FlushBufferAsync(buffer, errors, uploadId, cancellationToken);
                    buffer.Clear();
                }
            }

            // Final flush
            if (buffer.Count > 0)
            {
                await FlushBufferAsync(buffer, errors, uploadId, cancellationToken);
                buffer.Clear();
            }

            // Persist errors
            if (errors.Any())
            {
                const int errorBatch = 500;
                for (var i = 0; i < errors.Count; i += errorBatch)
                {
                    var batch = errors.Skip(i).Take(errorBatch);
                    await _Context.UploadErrors.AddRangeAsync(batch, cancellationToken);
                    await _Context.SaveChangesAsync(cancellationToken);
                }
            }

            uploadHistory.TotalRecords = total;
            uploadHistory.FailedRecords = errors.Count;
            uploadHistory.Status = errors.Any() ? "CompletedWithErrors" : "Completed";
            _Context.UploadHistories.Update(uploadHistory);
            await _Context.SaveChangesAsync(cancellationToken);

            return uploadId;
        }

        // Helper: validate FK existence, detect duplicates in DB and record errors, then persist remaining records.
        private async Task FlushBufferAsync(List<PricingRecord> buffer, List<UploadError> errors, Guid uploadId, CancellationToken cancellationToken)
        {
            if (!buffer.Any()) return;

            // batch-level FK checks
            var storeIds = buffer.Select(b => b.StoreId).Distinct().ToList();
            var skus = buffer.Select(b => b.SKU).Distinct().ToList();
            var dates = buffer.Select(b => b.PriceDate).Distinct().ToList();

            var existingStores = await _Context.Stores.Where(s => storeIds.Contains(s.StoreId)).Select(s => s.StoreId).ToListAsync(cancellationToken);
            var missingStores = storeIds.Except(existingStores).ToHashSet();

            var existingProducts = await _Context.Products.Where(p => skus.Contains(p.SKU)).Select(p => p.SKU).ToListAsync(cancellationToken);
            var missingProducts = skus.Except(existingProducts).ToHashSet();

            // query existing pricing records that conflict with unique index
            var existingPricing = await _Context.PricingRecords
                .Where(p => storeIds.Contains(p.StoreId) && skus.Contains(p.SKU) && dates.Contains(p.PriceDate))
                .Select(p => new { p.StoreId, p.SKU, p.PriceDate })
                .ToListAsync(cancellationToken);

            var existingKeys = new HashSet<string>(existingPricing.Select(e => $"{e.StoreId}|{e.SKU}|{e.PriceDate:yyyy-MM-dd}"));

            // Filter buffer into toInsert and record errors for missing FK or duplicate
            var toInsert = new List<PricingRecord>();
            foreach (var rec in buffer)
            {
                var key = $"{rec.StoreId}|{rec.SKU}|{rec.PriceDate:yyyy-MM-dd}";
                if (missingStores.Contains(rec.StoreId))
                {
                    errors.Add(new UploadError { UploadId = uploadId, RowNumber = 0, Error = $"Unknown StoreId {rec.StoreId}", RawData = $"{rec.StoreId},{rec.SKU},{rec.Price},{rec.PriceDate:yyyy-MM-dd}" });
                    continue;
                }

                if (missingProducts.Contains(rec.SKU))
                {
                    errors.Add(new UploadError { UploadId = uploadId, RowNumber = 0, Error = $"Unknown SKU {rec.SKU}", RawData = $"{rec.StoreId},{rec.SKU},{rec.Price},{rec.PriceDate:yyyy-MM-dd}" });
                    continue;
                }

                if (existingKeys.Contains(key))
                {
                    // duplicate per DB unique index — record and skip
                    errors.Add(new UploadError { UploadId = uploadId, RowNumber = 0, Error = $"Duplicate record for StoreId={rec.StoreId}, SKU={rec.SKU}, PriceDate={rec.PriceDate:yyyy-MM-dd}", RawData = $"{rec.StoreId},{rec.SKU},{rec.Price},{rec.PriceDate:yyyy-MM-dd}" });
                    continue;
                }

                toInsert.Add(rec);
            }

            if (toInsert.Any())
            {
                await _Context.PricingRecords.AddRangeAsync(toInsert, cancellationToken);
                try
                {
                    await _Context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException dbEx)
                {
                    // best-effort: capture duplicates or constraint failures not caught earlier
                    errors.Add(new UploadError { UploadId = uploadId, RowNumber = 0, Error = $"DbUpdateException: {dbEx.Message}", RawData = null });
                }
            }
        }
    }
}
