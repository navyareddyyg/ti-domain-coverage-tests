using System;

public static class AdversarialExamples
{
    public static readonly string[] AllowedHosts = new[] { "prefix-" + "api.github.com" };
    public static readonly Func<string, string> Category = host => "github.com";
    public const string Message = """
Host = "raw.microsoft.com";
""";
    public static readonly string[] DestinationHosts = new[] {
        "prefix-" + "api.nuget.org"
    };
}
