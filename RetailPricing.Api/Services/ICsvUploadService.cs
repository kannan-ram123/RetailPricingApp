namespace RetailPricing.Api.Services
{
    public interface ICsvUploadService
    {
        Task<Guid> ProcessPricingCsvAsync(IFormFile csvFile);
    }
}
