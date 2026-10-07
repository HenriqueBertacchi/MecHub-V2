using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using MecHub.Services;
using System.Globalization;
using System.Text;

var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. JWT — variáveis de configuração
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "SuaChaveSuperSecretaEMuitoLongaParaODevelopmentMecHub2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MecHubAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MecHubApp";

// ============================================================
// 2. AUTENTICAÇÃO UNIFICADA (Cookie + Google + JWT Bearer)
// ============================================================
builder.Services.AddAuthentication(options =>
{
    // Web MVC usa Cookie por padrão
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Desafios de API usam JWT
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ============================================================
// 3. CORS — libera React Native / Expo
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================================
// 4. EMAIL
// ============================================================
builder.Services.AddHttpClient<IEmailService, EmailService>();

// ============================================================
// 5. QUESTPDF + SERVIÇOS DA APLICAÇÃO
// ============================================================
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddScoped<OrdemServicoPdfService>();  // ✅ tipo explícito
builder.Services.AddScoped<TokenService>();            // ✅ TokenService para JWT

// ============================================================
// 6. LOCALIZATION
// ============================================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// ============================================================
// 7. BANCO DE DADOS — MySQL
// ⚠️ Troque AppDbContext pelo nome real do SEU contexto
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>   // ✅ tipo explícito
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

// ============================================================
// 8. MVC + API Controllers
// ============================================================
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MecHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// 9. FORWARDED HEADERS — Railway / Render / Proxy HTTPS
// ============================================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>   // ✅ genérico explícito
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ============================================================
// BUILD
// ============================================================
var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseRequestLocalization(localizationOptions);
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();