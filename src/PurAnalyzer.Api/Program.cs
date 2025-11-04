using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PurAnalyzer.Api.Authentication;
using System.Text;
using PurAnalyzer.Application.Analysis;
using PurAnalyzer.Application.Parsing;
using PurAnalyzer.Infrastructure.Parsing;
using PurAnalyzer.Infrastructure.Persistence;
using PurAnalyzer.Api.Validation;
using System.Reflection;
using PurAnalyzer.Infrastructure.Persistence.Services;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// --- Logging (Serilog) ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// ProblemDetails (RFC 7807) + traceId
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

// --- Database (PostgreSQL via EF Core) ---
builder.Services.AddDbContext<PurDbContext>(options =>
{
    // Retrieve connection string either from appsettings.json or environment variable
    var cs = builder.Configuration.GetConnectionString("Postgres")
             ?? Environment.GetEnvironmentVariable("POSTGRES_CONNSTR");

    // Register DbContext using Npgsql provider
    options.UseNpgsql(cs, npgsqlOptions =>
    {
        // Configure the EF Core migrations history table
        // It will also use snake_case and can live in a custom schema (here: 'meta')
        npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "meta");
    })
    // Enforce snake_case naming convention for tables, columns, keys, etc.
    .UseSnakeCaseNamingConvention();
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

// Application DI
builder.Services.AddScoped<IPurFileAnalyzer, PurFileAnalyzer>();
builder.Services.AddScoped<IPurFileParser, PurFileParser>();
builder.Services.AddScoped<PurFileValidator>();
builder.Services.AddScoped<IDocumentWriter, DocumentWriter>();

// BasicAuth
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// --- HTTP Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// HTTPS redirection for local dev (dev-certs + launchSettings with https).
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
