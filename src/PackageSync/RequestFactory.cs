using System;
using System.Net.Http;

namespace PackageSync;

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
}
