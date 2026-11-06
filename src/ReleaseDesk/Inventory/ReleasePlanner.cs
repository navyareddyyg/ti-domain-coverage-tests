using ReleaseDesk.Configuration;
using ReleaseDesk.Integrations;

namespace ReleaseDesk.Inventory;

public sealed record PlannedRequest(string Purpose, string Method, string Url);

public sealed record ReleasePlan(
    string ReleaseName, IReadOnlyList<PackageReference> Packages, IReadOnlyList<PlannedRequest> Requests);

public static class ReleasePlanner
{
    public static ReleasePlan Create(ApplicationSettings settings, IReadOnlyList<PackageReference> packages)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(packages);
        settings.Validate();

        var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (PackageReference package in packages)
        {
            ArgumentNullException.ThrowIfNull(package);
            package.Validate();
            if (!identities.Add($"{package.Ecosystem}:{package.Name}"))
            {
                throw new ArgumentException("Inventory contains more than one entry for the same package.");
            }
        }

        var requests = new List<PlannedRequest>();
        AddRequest("catalog", RequestFactory.CreateCatalogRequest());
        AddRequest("status", RequestFactory.CreateStatusRequest());
        AddRequest("release-metadata", new HttpRequestMessage(HttpMethod.Get, settings.SourceControl.ApiUrl));
        foreach (PackageReference package in packages)
        {
            RegistryEndpoint registry = package.Ecosystem == "nuget"
                ? settings.Registries.NuGet : settings.Registries.Npm;
            AddRequest("package-metadata", RequestFactory.CreatePackageRequest(registry, package));
        }

        return new ReleasePlan(settings.ReleaseName, packages.ToArray(), requests.ToArray());

        void AddRequest(string purpose, HttpRequestMessage request)
        {
            using (request)
            {
                requests.Add(new PlannedRequest(purpose, request.Method.Method,
                    request.RequestUri!.AbsoluteUri));
            }
        }
    }
}
