using SharingMezzi.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAZIONE PORTA =====
builder.WebHost.UseUrls("http://localhost:5050", "https://localhost:5051");

// Add services to the container.
builder.Services.AddRazorPages();

// ===== SERVIZI FONDAMENTALI =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ===== REGISTRA SERVIZI =====
builder.Services.AddScoped<IApiService, DirectApiService>();

// Configure Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Configure CORS per permettere chiamate all'API backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBackend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001", "http://localhost:7000", "https://localhost:7001")
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
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowBackend");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

Console.WriteLine("🚀 SharingMezzi Web Application started successfully!");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Frontend URL: http://localhost:5050");
Console.WriteLine($"🔗 Frontend HTTPS: https://localhost:5051");
Console.WriteLine($"🔌 Backend API: {builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000"}");
Console.WriteLine("📄 Homepage con dati reali disponibile su /");

app.Run();