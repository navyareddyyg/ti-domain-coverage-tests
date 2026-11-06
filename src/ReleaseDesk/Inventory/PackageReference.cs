namespace ReleaseDesk.Inventory;

public sealed record PackageReference
{
    public required string Ecosystem { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(Version);
        if (Name is "." or ".." || Version is "." or "..")
        {
            throw new ArgumentException("Package names and versions cannot be URI dot segments.");
        }

        if (Ecosystem is not ("nuget" or "npm"))
        {
            throw new ArgumentException("Package ecosystem must be nuget or npm.");
        }
    }
}
