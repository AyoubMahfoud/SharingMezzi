using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http;
using SharingMezzi.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configure HttpClient specifically for API calls
builder.Services.AddHttpClient<IApiService, DirectApiService>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    Console.WriteLine($"Configurando DirectApiService con BaseAddress: {apiBaseUrl}");
    
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// Add API Services
// Nota: IApiService è già registrato tramite AddHttpClient sopra
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

// Verifica connettività API all'avvio
await Task.Run(async () => {
    try {
        var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
        Console.WriteLine($"Verificando connettività API: {apiUrl}");
        using var client = new HttpClient();
        // Proviamo diversi endpoint comuni che potrebbero esistere
        var endpoints = new[] { 
            "/api/auth", 
            "/api/mezzi",
            "/api/parcheggi",
            "/api"
        };
        
        bool apiRaggiungibile = false;
        foreach (var endpoint in endpoints) {
            try {
                Console.WriteLine($"Tentativo di connessione a: {apiUrl}{endpoint}");
                var response = await client.GetAsync($"{apiUrl}{endpoint}");
                Console.WriteLine($"Risposta da {endpoint}: {response.StatusCode}");
                
                // Anche se riceviamo 401 Unauthorized, significa che l'API è in esecuzione
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    Console.WriteLine($"API backend raggiungibile su {endpoint}. Stato: {response.StatusCode}");
                    apiRaggiungibile = true;
                    break;
                }
            } catch {
                // Continuiamo a provare con altri endpoint
                continue;
            }
        }
        
        if (!apiRaggiungibile) {
            Console.WriteLine("ATTENZIONE: API non raggiungibile. Assicurati che il backend API sia in esecuzione su " + apiUrl);
        }
    } catch (Exception ex) {
        Console.WriteLine($"Errore durante la verifica dell'API: {ex.Message}");
        Console.WriteLine("Assicurati che il backend API sia in esecuzione prima di utilizzare il frontend.");
    }
});

app.Run();
