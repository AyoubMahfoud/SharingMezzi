using SharingMezzi.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ===== IMPORTANTE: Aggiungi IHttpContextAccessor =====
builder.Services.AddHttpContextAccessor();

// Add logging
builder.Services.AddLogging(builder => {
    builder.AddConsole();
    builder.AddDebug();
});

// Configure HttpClient for API communication
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";
    Console.WriteLine($"Configurando HttpClient con BaseUrl: {apiBaseUrl}");
    
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(Convert.ToInt32(builder.Configuration["ApiSettings:Timeout"] ?? "60"));
    client.DefaultRequestHeaders.Add("User-Agent", "SharingMezzi.Web/1.0");
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "DefaultSecretKey")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add API Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBillingService, BillingService>();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Kestrel to use specific ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5050, configure => configure.UseHttps()); // HTTPS
    options.ListenLocalhost(5051); // HTTP
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();

// Verifica connettività API all'avvio con retry logic
await Task.Run(async () => {
    try {
        var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
        Console.WriteLine($"=== VERIFICA CONNETTIVITÀ API ===");
        Console.WriteLine($"URL API: {apiUrl}");
        
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        
        var endpoints = new[] { 
            "/swagger",
            "/api/auth/login", 
            "/api/mezzi",
            "/api/parcheggi"
        };
        
        bool apiRaggiungibile = false;
        
        // Retry con backoff
        for (int retry = 0; retry < 3 && !apiRaggiungibile; retry++)
        {
            if (retry > 0)
            {
                Console.WriteLine($"Tentativo {retry + 1}/3...");
                await Task.Delay(2000 * retry); // Backoff exponential
            }
            
            foreach (var endpoint in endpoints) {
                try {
                    var fullUrl = $"{apiUrl}{endpoint}";
                    Console.WriteLine($"Testing: {fullUrl}");
                    
                    var response = await client.GetAsync(fullUrl);
                    Console.WriteLine($"Response: {response.StatusCode}");
                    
                    // 200 OK, 401 Unauthorized, o 404 NotFound significano che l'API risponde
                    if (response.IsSuccessStatusCode || 
                        response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                        Console.WriteLine($"✅ API backend raggiungibile su {endpoint}");
                        apiRaggiungibile = true;
                        break;
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"❌ Errore su {endpoint}: {ex.Message}");
                }
            }
        }
        
        if (!apiRaggiungibile) {
            Console.WriteLine("⚠️  ATTENZIONE: API backend non raggiungibile!");
            Console.WriteLine("Assicurati che SharingMezzi.Api sia in esecuzione su " + apiUrl);
            Console.WriteLine("Per avviare l'API: cd SharingMezzi.Api && dotnet run");
        } else {
            Console.WriteLine("✅ Connettività API verificata con successo!");
        }
        Console.WriteLine("==================================");
        
    } catch (Exception ex) {
        Console.WriteLine($"❌ Errore durante la verifica dell'API: {ex.Message}");
    }
});

Console.WriteLine($"🚀 SharingMezzi.Web avviato su:");
Console.WriteLine($"   HTTP:  http://localhost:5051");
Console.WriteLine($"   HTTPS: https://localhost:5050");

app.Run();