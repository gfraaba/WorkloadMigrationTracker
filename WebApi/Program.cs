using System.Reflection;
using WebApi.Data;
using WebApi.Utils;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    connectionString = Utils.ReplaceEnvironmentVariables(connectionString);
    // Log the connection string (remove sensitive info in production)
    Console.WriteLine($"Using connection string: {Regex.Replace(connectionString, "Password=[^;]*", "Password=***")}");
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(); // Enable detailed SQL query logging
    options.LogTo(Console.WriteLine); // Log SQL queries to the console
});

// Configure CORS from configuration (AllowedOrigins can be a semicolon-separated list)
var allowedOriginsConfig = builder.Configuration.GetValue<string>("AllowedOrigins") ??
                           builder.Configuration.GetValue<string>("ALLOWED_ORIGINS") ?? string.Empty;
var allowedOrigins = allowedOriginsConfig
    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.Trim())
    .Where(s => !string.IsNullOrEmpty(s))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            // Development fallback: allow any origin to avoid blocking local runs when not configured.
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production: require explicit configuration of allowed origins.
            policy.WithOrigins() // no origins configured intentionally
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Keep XML comments if present; avoid referencing OpenApi model types directly to
    // maintain compatibility across package versions.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workload Migration API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

// Initialize the database with retry to handle DB creation/recovery timing
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Retry loop: wait for DB to be connectable before running migrations/seeding.
        var maxAttempts = int.TryParse(Environment.GetEnvironmentVariable("DB_CONNECT_MAX_ATTEMPTS") ?? "30", out var ma) ? ma : 30;
        var delayMs = int.TryParse(Environment.GetEnvironmentVariable("DB_CONNECT_DELAY_MS") ?? "2000", out var dm) ? dm : 2000;
        var connected = false;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting DB connect ({Attempt}/{MaxAttempts})", attempt, maxAttempts);
                if (context.Database.CanConnect())
                {
                    connected = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "DB connect attempt failed");
            }
            System.Threading.Tasks.Task.Delay(delayMs).GetAwaiter().GetResult();
        }

        if (!connected)
        {
            logger.LogError("Could not connect to the database after {MaxAttempts} attempts.", maxAttempts);
            throw new Exception("Database not ready");
        }

        context.Database.EnsureCreated(); // Creates database if not exists
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();