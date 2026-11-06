# ReleaseDesk

A small dependency-inventory worker for release preparation. It validates a
package inventory and writes a reviewable plan containing registry downloads,
source-control metadata and catalog/status requests.

The worker only prepares the plan. It never sends HTTP requests, executes
packages or downloads artifacts. The included inventory is authored sample data;
this is not a real customer's application.

## Run locally

Use the .NET 10 SDK. No external NuGet packages, accounts or credentials are
required.

```powershell
dotnet build ReleaseDesk.slnx -c Release
dotnet run --project src\ReleaseDesk -c Release --no-build
```

The default inventory and application settings are copied beside the executable.
The output is `artifacts\release-plan.json`. Existing output files are never
overwritten; choose a different output path for another run:

```powershell
dotnet run --project src\ReleaseDesk -c Release --no-build -- --settings config\production.json --inventory data\packages.json --output artifacts\production-plan.json
```

## Repository

| Directory | Purpose |
| --- | --- |
| `src/ReleaseDesk` | Worker, settings, inventory rules and integration requests. |
| `data` | Local package inventory. |
| `config` | Environment-specific settings. |
| `tests/ReleaseDesk.Checks` | Dependency-free application checks. |
| `docs` | Configuration and operations notes. |
| `.github/workflows` | Windows/Linux build and checks. |

```powershell
dotnet run --project tests\ReleaseDesk.Checks -c Release --no-build
dotnet format ReleaseDesk.slnx --verify-no-changes --no-restore
```
