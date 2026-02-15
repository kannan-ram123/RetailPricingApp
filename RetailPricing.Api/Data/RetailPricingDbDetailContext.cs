using Microsoft.EntityFrameworkCore;
using RetailPricing.Api.Models;

namespace RetailPricing.Api.Data
{
    public class RetailPricingDbDetailContext : DbContext
    {
        public RetailPricingDbDetailContext(DbContextOptions<RetailPricingDbDetailContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<PricingRecord> PricingRecords { get; set; }
        public DbSet<Stores> Stores { get; set; }
        public DbSet<UploadHistory> UploadHistories { get; set; }
        public DbSet<UploadError> UploadErrors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PricingRecords table mapping: EF property -> DB column
            modelBuilder.Entity<PricingRecord>(b =>
            {
                b.ToTable("PricingRecords");
                b.HasKey(p => p.PricingRecordId);
                b.Property(p => p.PricingRecordId).HasColumnName("PricingRecordId");
                b.Property(p => p.UploadBatchID).HasColumnName("UploadBatchId");
                b.HasIndex(p => p.SKU).HasDatabaseName("IX_Pricing_SKU_Date");
                b.HasIndex(p => new { p.StoreId, p.PriceDate }).HasDatabaseName("IX_Pricing_Store_Date");
                b.HasIndex(p => p.UploadBatchID).HasDatabaseName("IX_Pricing_UploadBatch");
            });

            // Stores table mapping
            modelBuilder.Entity<Stores>(b =>
            {
                b.ToTable("Stores");
                b.HasKey(s => s.StoreId);
                b.Property(s => s.StoreId).HasColumnName("StoreId");
            });

            // Products table mapping (SKU primary key)
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(p => p.SKU);
                b.Property(p => p.SKU).HasColumnName("SKU");
            });

            // UploadHistory table mapping (DB uses UploadedAt)
            modelBuilder.Entity<UploadHistory>(b =>
            {
                b.ToTable("UploadHistory");
                b.HasKey(u => u.UploadId);
                b.Property(u => u.UploadedAt).HasColumnName("UploadedAt");
            });

            // UploadErrors
            modelBuilder.Entity<UploadError>(b =>
            {
                b.ToTable("UploadErrors");
                b.HasKey(e => e.Id);
                b.HasIndex(e => e.UploadId).HasDatabaseName("IX_UploadErrors_UploadId");
            });
        }
    }
}
