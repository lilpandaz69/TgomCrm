using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tagom.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ============================
// ✅ Configure DbContext
// ============================
builder.Services.AddDbContext<TagomDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

// ============================
// ✅ Add Controllers + Session
// ============================
builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // allow cookies over HTTP
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // allow HTTP (no HTTPS required)
});

// ============================
// ✅ Configure CORS for Angular
// ============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // only HTTP, remove HTTPS version
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================
// ✅ JWT Authentication
// ============================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

// ============================
// ✅ Use only HTTP (no HTTPS)
// ============================
builder.WebHost.UseUrls("http://localhost:5062");

var app = builder.Build();

// ============================
// ✅ Middleware Pipeline
// ============================
app.UseRouting();

// CORS must come before session/auth
app.UseCors("AllowAngularApp");



app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
