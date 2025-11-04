namespace PurAnalyzer.Api.Authentication;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Custom Basic Authentication handler for the PurAnalyzer API.
/// Validates credentials provided in the Authorization header
/// against environment or configuration values.
/// </summary>
public sealed class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthHandler"/> class.
    /// </summary>
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    /// <summary>
    /// Attempts to authenticate the incoming request using Basic Authentication.
    /// Credentials are compared with the expected username and password
    /// from environment variables (<c>BASICAUTH_USERNAME</c>, <c>BASICAUTH_PASSWORD</c>).
    /// </summary>
    /// <returns>
    /// An <see cref="AuthenticateResult"/> indicating success or failure.
    /// </returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read expected credentials from configuration/environment
        var cfg = Context.RequestServices.GetRequiredService<IConfiguration>();
        var expectedUser = cfg["BASICAUTH_USERNAME"] ?? string.Empty;
        var expectedPass = cfg["BASICAUTH_PASSWORD"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(expectedPass))
        {
            return Task.FromResult(AuthenticateResult.Fail("BasicAuth credentials not configured."));
        }

        // Safely read header → no nullable warnings
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        // TryParse protects against null/invalid formats
        if (!AuthenticationHeaderValue.TryParse(authValues.ToString(), out var header) ||
            !string.Equals(header.Scheme, "Basic", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));
        }

        try
        {
            var bytes = Convert.FromBase64String(header.Parameter);
            var decoded = Encoding.UTF8.GetString(bytes);
            var parts = decoded.Split(':', 2);
            if (parts.Length != 2)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid credential format."));
            }

            var user = parts[0];
            var pass = parts[1];

            if (!FixedTimeEquals(user, expectedUser) || !FixedTimeEquals(pass, expectedPass))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid credentials."));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, user) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Malformed Authorization header."));
        }
    }

    /// <summary>
    /// Sends the HTTP 401 challenge with a Basic realm when authentication fails.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = @"Basic realm=""PurAnalyzer""";
        return base.HandleChallengeAsync(properties);
    }

    /// <summary>
    /// Performs a constant-time string comparison to protect against timing attacks.
    /// </summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        if (ba.Length != bb.Length) return false;
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
