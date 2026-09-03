namespace Wfrp4.Server.Auth;

public class KeycloakBackchannelHandler : DelegatingHandler
{
    private readonly Uri? _backchannelAuthority;

    public KeycloakBackchannelHandler(IConfiguration configuration)
    {
        var authority = configuration["Keycloak:BackchannelAuthority"]?.TrimEnd('/');
        _backchannelAuthority = Uri.TryCreate(authority, UriKind.Absolute, out var parsedAuthority)
            ? parsedAuthority
            : null;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_backchannelAuthority is not null
            && request.RequestUri is not null
            && string.Equals(request.RequestUri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(request.RequestUri)
            {
                Scheme = _backchannelAuthority.Scheme,
                Host = _backchannelAuthority.Host,
                Port = _backchannelAuthority.Port,
            };

            request.RequestUri = builder.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}