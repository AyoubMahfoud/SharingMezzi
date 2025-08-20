using SharingMezzi.Web.Services;
using SharingMezzi.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAZIONE SOLO HTTP =====
builder.WebHost.UseUrls("http://0.0.0.0:5050");

// Add services to the container.
builder.Services.AddRazorPages();
// Register controllers (attribute-routed controllers like AuthController)
builder.Services.AddControllers();

// ===== SERVIZI FONDAMENTALI =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ===== REGISTRA TUTTI I SERVIZI =====
builder.Services.AddScoped<IApiService, DirectApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBillingService, BillingService>();

// Configure Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        // Use the web app's Razor Pages for auth UI. Login page is at /Login.
        options.LoginPath = "/Login";
        // Logout/AccessDenied pages can be added later; keep sensible defaults.
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Configure CORS per permettere comunicazione con backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBackend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "http://0.0.0.0:5000",
                "http://*:5000"  // Allow any IP on port 5000
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

// RIMUOVIAMO HTTPS REDIRECT
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowBackend");
app.UseSession();
app.UseAutoLogin(); // Middleware per auto-login dai cookie persistenti
app.UseAuthenticationMiddleware(); // Middleware per proteggere le pagine autenticate
app.UseAuthentication();
app.UseAuthorization();

// Ensure attribute routed controllers are mapped (AuthController -> /Auth/*, BillingController -> /api/billing/*)
app.MapControllers();

app.MapRazorPages();

// ===== LOG DI AVVIO =====
Console.WriteLine("🚀 SharingMezzi Web Application started successfully!");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Frontend URL: http://localhost:5050");
Console.WriteLine($"🔌 Backend API: http://localhost:5000");
Console.WriteLine("📄 Homepage disponibile su http://localhost:5050");
Console.WriteLine("🔑 Credenziali: admin@sharingmezzi.it / admin123");
Console.WriteLine("✅ Tutti i servizi registrati:");
Console.WriteLine("   - IApiService → DirectApiService");
Console.WriteLine("   - IAuthService → AuthService");
Console.WriteLine("   - IVehicleService → VehicleService");
Console.WriteLine("   - IParkingService → ParkingService");
Console.WriteLine("   - IUserService → UserService");
Console.WriteLine("   - ITripService → TripService");
Console.WriteLine("   - IBillingService → BillingService");
Console.WriteLine("📊 Sistema pronto per il test!");

app.Run();