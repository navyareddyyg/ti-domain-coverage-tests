using System;

public static class ClientConfiguration
{
    public const string DefaultHost = "login.microsoftonline.com";

    public static bool IsExpectedHost(Uri destination) =>
        destination.Host == "api.github.com";

    public static string ReadHost(dynamic configuration)
    {
        string Host = configuration.ServerAddress;
        return Host;
    }

    public static string DescribeCategory() => "github.com";
    public const string TenancyHost = "ghe.com";
    public const string Message = "Host = 'comment.microsoft.com'";
    // Host = "commented.microsoft.com";
}
