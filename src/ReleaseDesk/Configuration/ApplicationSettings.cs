namespace ReleaseDesk.Configuration;

public sealed record ApplicationSettings
{
    public required string ReleaseName { get; init; }
    public required RegistrySettings Registries { get; init; }
    public required SourceControlSettings SourceControl { get; init; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ReleaseName);
        ArgumentNullException.ThrowIfNull(Registries);
        ArgumentNullException.ThrowIfNull(Registries.NuGet);
        ArgumentNullException.ThrowIfNull(Registries.Npm);
        Registries.NuGet.Validate();
        Registries.Npm.Validate();
        ArgumentNullException.ThrowIfNull(SourceControl);
        SourceControl.Validate();
    }
}

public sealed record RegistrySettings
{
    public required RegistryEndpoint NuGet { get; init; }
    public required RegistryEndpoint Npm { get; init; }
}

public sealed record RegistryEndpoint
{
    public required string Host { get; init; }
    public required string BasePath { get; init; }

    public void Validate()
    {
        if (Uri.CheckHostName(Host) != UriHostNameType.Dns)
        {
            throw new ArgumentException("Registry host must be a DNS hostname without a scheme or path.");
        }

        ArgumentNullException.ThrowIfNull(BasePath);
        if (BasePath.Contains('?', StringComparison.Ordinal)
            || BasePath.Contains('#', StringComparison.Ordinal))
        {
            throw new ArgumentException("Registry base path cannot contain a query or fragment.");
        }
    }
}

public sealed record SourceControlSettings
{
    public required string ApiUrl { get; init; }
    public required string AllowedHostSuffix { get; init; }

    public void Validate()
    {
        if (!Uri.TryCreate(ApiUrl, UriKind.Absolute, out Uri? uri)
            || uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length != 0)
        {
            throw new ArgumentException("Source control requires an absolute HTTPS URL without user info.");
        }

        if (Uri.CheckHostName(AllowedHostSuffix) != UriHostNameType.Dns
            || !(uri.Host.Equals(AllowedHostSuffix, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith("." + AllowedHostSuffix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Source-control URL is outside the configured host suffix.");
        }
    }
}
