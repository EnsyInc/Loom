using EnsyNet.Core.Configurations;

namespace EnsyInc.Loom.Core.Config;

public sealed record EntraConfig : IConfig
{
    public static string ConfigName => "Entra";

    public required string TenantId { get; init; }

    public required string Audience { get; init; }

    /// <summary>
    /// Overrides the computed Entra authority (<c>https://login.microsoftonline.com/{TenantId}/v2.0</c>).
    /// Tests set this to a local mock OIDC issuer instead of pointing at the real Entra tenant.
    /// </summary>
    public string? Authority { get; init; }

    public bool IsValid()
        => !string.IsNullOrWhiteSpace(TenantId) && !string.IsNullOrWhiteSpace(Audience);

    public string GetAuthority()
        => string.IsNullOrWhiteSpace(Authority) ? $"https://login.microsoftonline.com/{TenantId}/v2.0" : Authority;
}
