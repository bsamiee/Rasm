# [H1][COVERLET_API]

[IMPORTANT] Rasm uses `coverlet.MTP` (version pinned in `Directory.Packages.props`) through the .NET 10 Microsoft Testing Platform runner. It is injected only for runnable test projects; `Rasm.TestKit`, benchmarks, fuzz harnesses, and bridge scenarios are not coverage targets.

## [1][PACKAGE]

| [INDEX] | [PACKAGE]      | [PIN]                      | [USE]                                                                    |
| :-----: | -------------- | -------------------------- | ------------------------------------------------------------------------ |
|   [1]   | `coverlet.MTP` | `Directory.Packages.props` | `dotnet test --project ... --coverlet` via `tools.quality test coverage` |

[SOURCE] xUnit MTP coverage docs: https://xunit.net/docs/getting-started/v3/code-coverage-with-mtp

## [2][MTP_SURFACE]

| [INDEX] | [ARGUMENT]                                      | [RASM_POSTURE]                                               |
| :-----: | ----------------------------------------------- | ------------------------------------------------------------ |
|   [1]   | `--coverlet`                                    | Command-time opt-in through `tools.quality test coverage`.   |
|   [2]   | `--coverlet-output-format`                      | `json` and `cobertura` for local and CI-readable output.     |
|   [3]   | `--coverlet-include` / `--coverlet-exclude`     | Managed assemblies only; avoid bridge/runtime fiction.       |
|   [4]   | `--coverlet-exclude-by-file`                    | Generated and obj-path exclusions.                           |
|   [5]   | `--coverlet-exclude-by-attribute`               | Coverage and generated-code attribute exclusions.            |
|   [6]   | `--coverlet-exclude-assemblies-without-sources` | `None`; local proof showed stricter modes can empty reports. |

## [3][RASM_SCOPE]

- `Rasm.Tests` is the first managed coverage target.
- Rhino/GH document, UI, viewport, command, and native validity behavior belongs to bridge scenarios.
- Classify each uncovered line as missing static law, bridge-owned runtime path, generated code, or unreachable defensive guard before adding tests.
- Do not write artificial tests only to raise percentages.

Local command:

```bash
uv run python -m tools.quality test coverage
```

## [4][SURVIVOR_CLASSIFICATION]

| [INDEX] | [CLASS]               | [ACTION]                                                                                                       |
| :-----: | --------------------- | -------------------------------------------------------------------------------------------------------------- |
|   [1]   | Missing static law    | Add a Grade A/B/C oracle to the owning spec.                                                                   |
|   [2]   | Bridge-owned runtime  | Strengthen `*.verify.csx`; coverlet cannot reach line. `[ExcludeFromCodeCoverage]` only after scenario exists. |
|   [3]   | Generated source      | Exclude centrally by file or attribute.                                                                        |
|   [4]   | Defensive unreachable | One-line comment naming unreachability invariant; do not test the arm.                                         |
|   [5]   | Dead code             | Remove.                                                                                                        |

Anti-patterns:
- Writing an `Assert.True(true)` test only to cover the line.
- Bumping thresholds to mask uncovered bridge paths.
- Mutating the test to invoke a private method via reflection.
