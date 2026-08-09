using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Server.Auth;
using Wfrp4.Server.Filters;
using Wfrp4.Server.Services;

var builder = WebApplication.CreateBuilder(args);
var keycloakConfiguration = await LoadKeycloakConfigurationAsync(builder.Configuration);

// --- EF Core + PostgreSQL ---
builder.Services.AddDbContext<Wfrp4DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Wfrp4")));

// --- Authentication (Keycloak JWT) ---
builder.Services.AddTransient<KeycloakBackchannelHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.IncludeErrorDetails = true;
        options.BackchannelHttpHandler = new KeycloakBackchannelHandler(builder.Configuration)
        {
            InnerHandler = new HttpClientHandler(),
        };
        var validIssuer = builder.Configuration["Keycloak:ValidIssuer"]
            ?? builder.Configuration["Keycloak:Authority"];
        if (keycloakConfiguration is not null)
        {
            options.Configuration = keycloakConfiguration;
        }
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogError(context.Exception, "JWT authentication failed.");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogWarning("JWT challenge triggered. Error={Error}; Description={Description}", context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidIssuer = validIssuer,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });

builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

// --- Authorization Policies ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Joueur", p => p.RequireRole("wfrp4-joueur"));
    options.AddPolicy("MaitreJeu", p => p.RequireRole("wfrp4-maitre-jeu"));
    options.AddPolicy("Admin", p => p.RequireRole("wfrp4-admin"));
});

// --- Services ---
builder.Services.AddScoped<PersonnageService>();
builder.Services.AddScoped<XPService>();
builder.Services.AddScoped<PersonnageOwnerFilter>();

// --- Inscription publique (API Admin Keycloak) ---
builder.Services.AddHttpClient("KeycloakAdmin")
    .AddHttpMessageHandler<KeycloakBackchannelHandler>();
builder.Services.AddScoped<KeycloakAdminService>();

builder.Services.AddControllers();

var app = builder.Build();

// --- Auto-migrate database ---
{
    var retries = 10;
    for (var i = 0; i < retries; i++)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Wfrp4DbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Applying database migrations (attempt {Attempt}/{Max})...", i + 1, retries);
            db.Database.Migrate();
            logger.LogInformation("Seeding reference data when required...");
            await Wfrp4DataSeeder.SeedAsync(db);
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex) when (i < retries - 1)
        {
            logger.LogWarning(ex, "Database not ready, retrying in 3s...");
            Thread.Sleep(3000);
        }
    }
}

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

static async Task<OpenIdConnectConfiguration?> LoadKeycloakConfigurationAsync(IConfiguration configuration)
{
    var authority = configuration["Keycloak:Authority"];
    if (string.IsNullOrWhiteSpace(authority) || !Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri))
    {
        return null;
    }

    var backchannelAuthority = configuration["Keycloak:BackchannelAuthority"];
    Uri keyUri;
    if (!string.IsNullOrWhiteSpace(backchannelAuthority) && Uri.TryCreate(backchannelAuthority, UriKind.Absolute, out var backchannelUri))
    {
        keyUri = new Uri($"{backchannelUri.Scheme}://{backchannelUri.Host}:{backchannelUri.Port}{authorityUri.AbsolutePath.TrimEnd('/')}/protocol/openid-connect/certs");
    }
    else
    {
        keyUri = new Uri($"{authority.TrimEnd('/')}/protocol/openid-connect/certs");
    }

    using var httpClient = new HttpClient();
    const int maxAttempts = 30; // 30 × 3s = 90s max pour que Keycloak soit prêt
    for (var attempt = 0; attempt < maxAttempts; attempt++)
    {
        try
        {
            var jwks = await httpClient.GetStringAsync(keyUri);
            var configurationResult = new OpenIdConnectConfiguration
            {
                Issuer = configuration["Keycloak:ValidIssuer"] ?? authority,
                JwksUri = keyUri.ToString(),
            };

            foreach (var key in new JsonWebKeySet(jwks).GetSigningKeys())
            {
                configurationResult.SigningKeys.Add(key);
            }

            return configurationResult;
        }
        catch (Exception ex) when (attempt < maxAttempts - 1)
        {
            Console.WriteLine($"[Keycloak] Tentative {attempt + 1}/{maxAttempts} échouée : {ex.Message}. Nouvelle tentative dans 3s...");
            await Task.Delay(3000);
        }
    }

    return null;
}