using ReleaseDesk.Configuration;
using ReleaseDesk.Inventory;

namespace ReleaseDesk.Integrations;

internal static class RequestFactory
{
    public static HttpRequestMessage CreateCatalogRequest() =>
        new(HttpMethod.Get, new UriBuilder
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "api-microservice-us1.com",
            Path = "v1/catalog"
        }.Uri);

    public static HttpRequestMessage CreateStatusRequest() =>
        new(HttpMethod.Post, new UriBuilder
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "senaildebe.xyz",
            Path = "v1/status"
        }.Uri);

    public static HttpRequestMessage CreatePackageRequest(RegistryEndpoint registry, PackageReference package)
    {
        string name = Uri.EscapeDataString(package.Name);
        string version = Uri.EscapeDataString(package.Version);
        string path = package.Ecosystem == "nuget"
            ? $"{name.ToLowerInvariant()}/{version.ToLowerInvariant()}/{name.ToLowerInvariant()}.nuspec"
            : $"{name}/{version}";
        return new HttpRequestMessage(HttpMethod.Get, new UriBuilder
        {
            Scheme = Uri.UriSchemeHttps,
            Host = registry.Host,
            Path = string.Join('/', new[] { registry.BasePath.Trim('/'), path }
                .Where(part => part.Length != 0))
        }.Uri);
    }
}
