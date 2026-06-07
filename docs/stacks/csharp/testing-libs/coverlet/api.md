# [COVERLET_API]

Use `coverlet.MTP` through the Microsoft Testing Platform runner where the project configures it. It belongs only in runnable test projects; testkit, benchmarks, fuzz harnesses, and runtime scenarios are not coverage targets.

## [1][MTP_SURFACE]

| [INDEX] | [ARGUMENT]                                      | [PROJECT_POSTURE]                                                  |
| :-----: | ----------------------------------------------- | ------------------------------------------------------------------ |
|   [1]   | `--coverlet`                                    | Command-time opt-in through the local test runner.                 |
|   [2]   | `--coverlet-output-format`                      | `json` and `cobertura` for local and CI-readable output.           |
|   [3]   | `--coverlet-include` / `--coverlet-exclude`     | Managed assemblies only; avoid bridge/runtime fiction.             |
|   [4]   | `--coverlet-exclude-by-file`                    | Generated and obj-path exclusions.                                 |
|   [5]   | `--coverlet-exclude-by-attribute`               | Coverage and generated-code attribute exclusions.                  |
|   [6]   | `--coverlet-exclude-assemblies-without-sources` | `None`; stricter modes can remove managed assemblies from reports. |

## [2][PROJECT_SCOPE]

- The primary managed test assembly is the first coverage target.
- Host document, UI, viewport, command, and native validity behavior belongs to runtime scenarios.
- Classify each uncovered line as missing static law, bridge-owned runtime path, generated code, or unreachable defensive guard before adding tests.
- Do not write artificial tests only to raise percentages.

Local command:

```bash
<test-runner> coverage
```

## [3][SURVIVOR_CLASSIFICATION]

| [INDEX] | [CLASS]               | [ACTION]                                                                                                             |
| :-----: | --------------------- | -------------------------------------------------------------------------------------------------------------------- |
|   [1]   | Missing static law    | Add a Grade A/B/C oracle to the owning spec.                                                                         |
|   [2]   | Runtime-owned path    | Strengthen the runtime scenario; coverlet cannot reach line. `[ExcludeFromCodeCoverage]` only after scenario exists. |
|   [3]   | Generated source      | Exclude centrally by file or attribute.                                                                              |
|   [4]   | Defensive unreachable | One-line comment naming unreachability invariant; do not test the arm.                                               |
|   [5]   | Dead code             | Remove.                                                                                                              |

Anti-patterns:
- Writing an `Assert.True(true)` test only to cover the line.
- Bumping thresholds to mask uncovered runtime paths.
- Mutating the test to invoke a private method via reflection.
