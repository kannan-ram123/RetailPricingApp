using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using RetailPricing.Api.Data;
using RetailPricing.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<RetailPricingDbDetailContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RetailPricingDbContext") ?? throw new InvalidOperationException("Connection string 'RetailPricingDbContext' not found.")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ICsvUploadService, CsvUploadService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI in Development (or remove the env check to enable always)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RetailPricing API v1");
        // Optional: serve Swagger UI at app root:
        // c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
