using System.Text.Json;
using ReleaseDesk;
using ReleaseDesk.Configuration;
using ReleaseDesk.Inventory;

try
{
    if (args is ["--help"])
    {
        Console.WriteLine("ReleaseDesk [--settings <json>] [--inventory <json>] [--output <new-json>]");
        return 0;
    }

    RunOptions options = RunOptions.Parse(args);
    ApplicationSettings settings = JsonSerializer.Deserialize<ApplicationSettings>(
        await File.ReadAllTextAsync(options.SettingsPath), JsonDefaults.Options)
        ?? throw new InvalidDataException("Application settings must be a JSON object.");
    PackageReference[] packages = JsonSerializer.Deserialize<PackageReference[]>(
        await File.ReadAllTextAsync(options.InventoryPath), JsonDefaults.Options)
        ?? throw new InvalidDataException("Package inventory must be a JSON array.");
    ReleasePlan plan = ReleasePlanner.Create(settings, packages);

    string output = Path.GetFullPath(options.OutputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    await using var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write);
    await JsonSerializer.SerializeAsync(stream, plan, JsonDefaults.Options);
    Console.WriteLine($"Prepared {plan.Packages.Count} packages and {plan.Requests.Count} requests.");
    Console.WriteLine($"Plan written to {output}. No requests were sent.");
    return 0;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
    or InvalidDataException or JsonException or ArgumentException)
{
    Console.Error.WriteLine($"ReleaseDesk: {exception.Message}");
    return 1;
}
