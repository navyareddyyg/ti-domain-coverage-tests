# Domain coverage test fixtures

Synthetic, non-sensitive development controls for strict bare-domain extraction.
The source and configuration files are authored examples, not applications to
build or run. Domain strings reference public services, threat indicators, or deliberate placeholders;
their presence does not assert ownership, maliciousness, reachability, or safety.

The repository contains fixtures and expected roles only. It contains no scanner
implementation, credentials, private source, customer samples, or malware.
Scanning these files must not execute them or contact the named services.

## What to check

| Case | Expected behavior |
| --- | --- |
| Explicit hostname settings and egress lists | Admit complete literal destinations. |
| Exact host equality | Admit the exact host, not a parent suffix. |
| Source property expressions | Reject identifiers as domain literals. |
| Tenancy suffixes, aliases, returned categories | Do not promote labels to destinations. |
| Comments, documentation, templates, private/example names | Do not admit. |
| Existing full URLs | Preserve their independent URL identity and full path. |

`cases.json` records roles by fixture intent. `manifest.json` pins file bytes
with SHA-256 and exact UTF-16 source spans. Wildcard/template values and full
URLs are structural controls, not bare-domain annotations. Full URLs in
`fixtures/settings.yaml`, `fixtures/page.html` and `notes.md` must remain
baseline URLs, including when bare-domain admission excludes Markdown.

For an offline runner accepting `--manifest` and `--out`:

```powershell
dotnet C:\path\to\your\offline-runner.dll --manifest .\manifest.json --out .\strict.report.json
.\Test-CoverageReport.ps1 -ReportPath .\strict.report.json
```

Acceptance: all 11 annotated destination occurrences are admitted, zero annotated
non-destinations are admitted, zero admissions are unannotated, and all three existing
full URL paths remain in the baseline. Duplicate hosts may share a root lookup
identity; they must not lose source occurrences. The checker validates candidate
records against labeled spans and original bytes, not just summary counters.

`fixtures/threat-domains.ini` keeps the two threat indicators as bare hostname
settings. They are extraction controls, not guaranteed-current malicious verdicts.

To regenerate spans/hashes after intentional fixture edits, use PowerShell 7
and generate a new file without overwriting existing evidence:

```powershell
.\New-Manifest.ps1 -OutFile manifest.local.json
```

Review the changed labels and manifest before replacing the committed version.
An offline extraction result is not a reputation verdict. These intentionally
authored controls establish behavior, not independent precision/recall,
natural threat-detection improvement, or production readiness.
