using BackgroundServices;
using Microsoft.OpenApi.Models;
using Repositories.Interfaces;
using Repositories.Reps;
using Services.Interfaces;
using Services.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GeoBlocker API",
        Version = "v1",
        Description = "Manage blocked countries and validate IP addresses using geolocation.",
        Contact = new OpenApiContact { Name = "Dev Team" }
    });

    // Include XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── HttpClient ────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("GeoApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "GeoBlocker-App/1.0");
});

// ── In-Memory Repository (Singleton = one shared instance) ────────────────────
builder.Services.AddSingleton<IInMemoryRepository, InMemoryRepository>();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ILogService, LogService>();

// ── Background Service ────────────────────────────────────────────────────────
builder.Services.AddHostedService<TemporalBlockCleanupService>();

// ── Build App ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GeoBlocker API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
