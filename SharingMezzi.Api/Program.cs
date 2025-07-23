using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using SharingMezzi.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<SharingMezziContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=sharingmezzi.db"));

// ===== CONTROLLERS =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== JWT AUTHENTICATION =====
var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "SharingMezzi-SecretKey-2024-VeryLongAndSecureKey123456789";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Disabilita per test
        ValidateAudience = false, // Disabilita per test
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ===== SWAGGER =====
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SharingMezzi API",
        Version = "v1",
        Description = "API per il sistema di sharing mezzi"
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(
                "http://localhost:5050", 
                "https://localhost:7050",
                "https://localhost:5050",
                "http://localhost:5051",
                "https://localhost:5051"
               )
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// ===== CONFIGURE PORTS =====
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, configure => configure.UseHttps()); // HTTPS
    options.ListenLocalhost(5000); // HTTP
});

var app = builder.Build();

// ===== DATABASE INITIALIZATION =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SharingMezziContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Crea utente di test se non esiste
    if (!context.Utenti.Any())
    {
        Console.WriteLine("🌱 Creazione utente admin...");
        
        // Crea dinamicamente l'utente senza dipendere da enum specifici
        var adminUser = new SharingMezzi.Core.Entities.Utente();
        
        // Imposta le proprietà di base
        adminUser.Email = "admin@sharingmezzi.it";
        adminUser.Nome = "Admin";
        adminUser.Cognome = "System";
        adminUser.Password = HashPassword("admin123");
        adminUser.DataRegistrazione = DateTime.UtcNow;
        adminUser.Credito = 100.00m;
        
        // Usa reflection per impostare proprietà che potrebbero esistere
        var userType = adminUser.GetType();
        
        // Imposta Ruolo se esiste
        var ruoloProperty = userType.GetProperty("Ruolo");
        if (ruoloProperty != null)
        {
            try
            {
                if (ruoloProperty.PropertyType.IsEnum)
                {
                    // Prova con "Amministratore"
                    try
                    {
                        var adminRole = Enum.Parse(ruoloProperty.PropertyType, "Amministratore");
                        ruoloProperty.SetValue(adminUser, adminRole);
                    }
                    catch
                    {
                        // Prova con "Admin"  
                        var adminRole = Enum.Parse(ruoloProperty.PropertyType, "Admin");
                        ruoloProperty.SetValue(adminUser, adminRole);
                    }
                }
                else if (ruoloProperty.PropertyType == typeof(string))
                {
                    ruoloProperty.SetValue(adminUser, "Amministratore");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Avviso: Non riesco a impostare il ruolo - {ex.Message}");
            }
        }
        
        // Imposta Stato se esiste
        var statoProperty = userType.GetProperty("Stato");
        if (statoProperty != null)
        {
            try
            {
                if (statoProperty.PropertyType.IsEnum)
                {
                    var attivoValue = Enum.Parse(statoProperty.PropertyType, "Attivo");
                    statoProperty.SetValue(adminUser, attivoValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Avviso: Non riesco a impostare lo stato - {ex.Message}");
            }
        }
        
        // Imposta PuntiEco se esiste
        var puntiEcoProperty = userType.GetProperty("PuntiEco");
        if (puntiEcoProperty != null)
        {
            try
            {
                puntiEcoProperty.SetValue(adminUser, 50);
            }
            catch { }
        }
        
        context.Utenti.Add(adminUser);
        await context.SaveChangesAsync();
        
        Console.WriteLine("✅ Utente admin creato:");
        Console.WriteLine($"   Email: admin@sharingmezzi.it");
        Console.WriteLine($"   Password: admin123");
    }
    else
    {
        Console.WriteLine("✅ Database già inizializzato");
    }
}

// ===== PIPELINE =====
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharingMezzi API v1");
    c.RoutePrefix = "swagger";
});

// CORS prima dell'autenticazione
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== ENDPOINTS DI TEST =====
app.MapGet("/", () => "SharingMezzi API è attiva! Vai su /swagger per la documentazione");

app.MapGet("/health", () => new {
    Status = "OK",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Database = "SQLite",
    Message = "API funzionante"
});

// ===== ENDPOINT TEST AUTH =====
app.MapPost("/api/test-login", async (SharingMezziContext context) =>
{
    var admin = await context.Utenti.FirstOrDefaultAsync(u => u.Email == "admin@sharingmezzi.it");
    if (admin == null)
    {
        return Results.NotFound("Admin user not found");
    }
    
    return Results.Ok(new {
        Message = "Admin user exists",
        Email = admin.Email,
        Nome = admin.Nome,
        HasPassword = !string.IsNullOrEmpty(admin.Password)
    });
});

Console.WriteLine("🚀 SharingMezzi.Api avviato su:");
Console.WriteLine("   HTTP:  http://localhost:5000");
Console.WriteLine("   HTTPS: https://localhost:5001");
Console.WriteLine("   📚 Swagger: https://localhost:5001/swagger");
Console.WriteLine("   🧪 Test: http://localhost:5000/health");

app.Run();

// ===== HELPER FUNCTIONS =====
static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}