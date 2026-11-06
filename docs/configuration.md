# Configuration

Pass `--settings` to select an environment file. Settings are loaded as a complete
document; there is no implicit merge with local files or environment variables.
Unknown JSON properties and missing required members are errors.

| Setting | Meaning |
| --- | --- |
| `releaseName` | Name recorded in the output plan. |
| `registries.nuGet.host` | NuGet registry hostname, separate from its path. |
| `registries.nuGet.basePath` | Prefix for NuGet metadata requests. |
| `registries.npm.host` | npm registry hostname. |
| `registries.npm.basePath` | Required npm metadata prefix; use `""` for no prefix. |
| `sourceControl.apiUrl` | Full metadata URL, retaining the repository path. |
| `sourceControl.allowedHostSuffix` | Source-control tenancy restriction, not a destination. |

Both registries use HTTPS. Host fields accept DNS names, not URLs or paths.
The source-control URL must use HTTPS and belong to the configured host suffix.
Catalog and status requests are constructed by the integration layer.

Inventory entries require `ecosystem`, `name` and `version`. Supported ecosystems
are `nuget` and `npm`. Each ecosystem/name pair may appear once. Scoped npm
package names and NuGet namespace-style names are package identities, not hosts.
