using System.Text.Json;
using ReleaseDesk;
using ReleaseDesk.Configuration;
using ReleaseDesk.Inventory;

ApplicationSettings settings = new()
{
    ReleaseName = "unit-tests",
    Registries = new RegistrySettings
    {
        NuGet = new RegistryEndpoint { Host = "packages.example.com", BasePath = "v3-flatcontainer" },
        Npm = new RegistryEndpoint { Host = "npm.example.com", BasePath = "" }
    },
    SourceControl = new SourceControlSettings
    {
        ApiUrl = "https://github.com/acme/releases/latest",
        AllowedHostSuffix = "github.com"
    }
};
PackageReference nuget = new() { Ecosystem = "nuget", Name = "Contoso.Tools", Version = "2.4.0" };
PackageReference npm = new() { Ecosystem = "npm", Name = "@contoso/ui", Version = "1.2.0" };

ReleasePlan plan = ReleasePlanner.Create(settings, [nuget, npm]);
Check(plan.Packages.Count == 2 && plan.Requests.Count == 5, "Plan must include packages and integration requests.");
Check(plan.Requests.Any(request => request.Purpose == "package-metadata"
    && request.Url.EndsWith("/v3-flatcontainer/contoso.tools/2.4.0/contoso.tools.nuspec", StringComparison.Ordinal)),
    "NuGet metadata paths must retain the selected package and version.");
Check(plan.Requests.Any(request => request.Purpose == "package-metadata"
    && request.Url.EndsWith("/%40contoso%2Fui/1.2.0", StringComparison.OrdinalIgnoreCase)),
    "Scoped npm names must remain one encoded package component.");
Check(plan.Requests.Single(request => request.Purpose == "release-metadata").Url
    == settings.SourceControl.ApiUrl, "Full source-control paths must be preserved.");
Check(plan.Requests.Single(request => request.Purpose == "status").Method == "POST",
    "The status integration must retain its HTTP method.");

ExpectArgument(() => ReleasePlanner.Create(settings, [nuget, nuget with { Version = "3.0.0" }]),
    "Conflicting versions of the same package must be rejected.");
ExpectArgument(() => ReleasePlanner.Create(settings, [npm with { Ecosystem = "other" }]),
    "Unsupported ecosystems must be rejected.");
ExpectArgument(() => ReleasePlanner.Create(settings, [npm with { Version = " " }]),
    "Missing versions must be rejected.");
foreach (string segment in new[] { ".", ".." })
{
    ExpectArgument(() => ReleasePlanner.Create(settings, [npm with { Name = segment }]),
        "Dot-only package names must be rejected before constructing URLs.");
    ExpectArgument(() => ReleasePlanner.Create(settings, [nuget with { Version = segment }]),
        "Dot-only versions must be rejected before constructing URLs.");
}
ExpectArgument(() => ReleasePlanner.Create(settings with
{
    Registries = settings.Registries with
    {
        NuGet = settings.Registries.NuGet with { Host = "not/a/host" }
    }
}, [nuget]), "Registry host settings cannot contain paths.");
ExpectArgument(() => ReleasePlanner.Create(settings with
{
    SourceControl = settings.SourceControl with { AllowedHostSuffix = "other.example.com" }
}, [nuget]), "Source-control suffix restrictions must be enforced.");
ExpectArgument(() => RunOptions.Parse(["--output", "one.json", "--output", "two.json"]),
    "Duplicate command-line options must be rejected.");
ExpectArgument(() => RunOptions.Parse(["--inventory", "--output"]),
    "An option cannot be consumed as another option's filename.");

PackageReference[] copied = JsonSerializer.Deserialize<PackageReference[]>(
    JsonSerializer.Serialize(plan.Packages, JsonDefaults.Options), JsonDefaults.Options)!;
Check(copied.SequenceEqual(plan.Packages), "Plan package identities must survive JSON serialization.");
Console.WriteLine("PASS: release planning, package paths, validation and command-line checks.");

static void Check(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void ExpectArgument(Action action, string message)
{
    try
    {
        action();
    }
    catch (ArgumentException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}
