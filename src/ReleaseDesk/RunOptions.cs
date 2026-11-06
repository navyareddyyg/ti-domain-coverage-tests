namespace ReleaseDesk;

public sealed record RunOptions(string SettingsPath, string InventoryPath, string OutputPath)
{
    public static RunOptions Parse(string[] arguments)
    {
        string settings = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        string inventory = Path.Combine(AppContext.BaseDirectory, "packages.json");
        string output = Path.Combine("artifacts", "release-plan.json");
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0; index < arguments.Length; index += 2)
        {
            string key = arguments[index];
            if (index + 1 >= arguments.Length || !seen.Add(key)
                || string.IsNullOrWhiteSpace(arguments[index + 1])
                || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException("Options require one non-empty value and cannot be repeated.");
            }

            switch (key)
            {
                case "--settings": settings = arguments[index + 1]; break;
                case "--inventory": inventory = arguments[index + 1]; break;
                case "--output": output = arguments[index + 1]; break;
                default: throw new ArgumentException($"Unknown option: {key}");
            }
        }

        return new RunOptions(settings, inventory, output);
    }
}
