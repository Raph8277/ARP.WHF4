using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Wfrp4.Client;
using Wfrp4.Client.Services;
using Wfrp4.Client.Theme;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- OIDC Authentication (Keycloak) ---
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = builder.Configuration["Keycloak:Authority"]!;
    options.ProviderOptions.ClientId = builder.Configuration["Keycloak:ClientId"]!;
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.RedirectUri = builder.HostEnvironment.BaseAddress + "authentication/login-callback";
    options.ProviderOptions.PostLogoutRedirectUri = builder.HostEnvironment.BaseAddress + "authentication/logout-callback";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
});

// --- HTTP Client with auth handler ---
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient<Wfrp4ApiClient>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

// --- HTTP Client anonyme pour l'inscription (pas de jeton requis) ---
builder.Services.AddHttpClient<InscriptionApiClient>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// --- MudBlazor ---
builder.Services.AddMudServices();

// --- Theme ---
builder.Services.AddScoped<UserThemeService>();

await builder.Build().RunAsync();
