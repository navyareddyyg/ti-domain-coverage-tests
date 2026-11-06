# Operations

ReleaseDesk is a local planning step. A successful run validates settings and
inventory, creates a new JSON plan and exits. It does not open a network connection.

The plan lists release metadata, integration requests and package metadata
requests. Preserve the full request paths when reviewing it. A hostname alone
does not identify the package or release being requested.

## Repeated runs

Use a new `--output` filename for each run. Input files are never rewritten and
existing plans are never replaced. The process exits nonzero and writes an error
to standard error for invalid JSON, unsupported inventory, invalid settings,
missing files or an existing output.

## Build checks

The workflow builds the solution, runs application checks and creates a plan
using the bundled inventory on Windows and Linux. It does not execute packages,
contact integrations or publish release artifacts.

If a plan fails validation, correct the settings or inventory and rerun. Do not
treat a failed plan as an empty successful export.
