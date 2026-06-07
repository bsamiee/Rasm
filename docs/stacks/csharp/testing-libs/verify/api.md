# [VERIFY_API]

Use `Verify.XunitV3` only with concrete snapshot-worthy tests. Do not use Verify for algebraic laws, numeric behavior, or host-native truth.

## [1][SETTINGS]

| [INDEX] | [SURFACE]                 | [PROJECT_USE]                                                                         |
| :-----: | ------------------------- | ------------------------------------------------------------------------------------- |
|   [1]   | `VerifierSettings`        | Add module-initializer scrubbers only when a snapshot contains machine-specific data. |
|   [2]   | `VerifySettings`          | Per-test path/name tweaks only when the artifact owner requires it.                   |
|   [3]   | `Verifier.DerivePathInfo` | Keep snapshots beside owning specs or under a stable artifact directory.              |
|   [4]   | Scrubbers                 | Remove machine paths, timestamps, generated IDs, and runtime-specific noise.          |
|   [5]   | `Verify.Cli`              | Review/accept received files manually; never auto-accept in scripts.                  |

## [2][PROJECT_SCOPE]

Use Verify for analyzer diagnostics, generated manifests, normalized runtime evidence JSON, or package/config reports. Avoid snapshots for floating numeric values, generated random samples, current implementation output, or anything better described by an algebraic law.

## [3][NATURAL_USE_CASES]

| [INDEX] | [USE_CASE]                           |
| :-----: | ------------------------------------ |
|   [1]   | `tools/cs-analyzer` rule diagnostics |
|   [2]   | SmartEnum catalog enumerations       |
|   [3]   | Runtime evidence JSON files          |
|   [4]   | Generated source files               |
|   [5]   | Package/config reports               |

Why Verify fits:
- [1] Per-rule fixture produces deterministic diagnostic text; drift catches wording changes that break IDE quick-fix UX.
- [2] When a public catalog is API surface, snapshotting the list catches accidental case reorder/rename in code review. Pair with `Spec.SmartEnumKeysUnique`.
- [3] Normalized runtime evidence JSON under the project artifact root after the runtime rail stabilizes.
- [4] Snapshot select Thinktecture generated source to catch source-generator behavior changes when the package graph changes.
- [5] Normalized package/config reports from the owning tool rail.

Anti-uses (Grade F):
- Snapshotting `Matrix.spec.cs` SVD reconstruction output is not a law; the math IS the oracle.
- Snapshotting runtime scenario stdout when it includes wall-clock timing or runtime version strings — scrub or skip.

## [4][SCRUBBER_RAIL]

Central tooling module initializer:

```csharp
[ModuleInitializer]
public static void InitVerify() {
    VerifierSettings.AddScrubber(text => text.Replace(Environment.MachineName, "{machine}"));
    VerifierSettings.AddScrubber(text => Regex.Replace(text.ToString(), @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", "{timestamp}"));
    VerifierSettings.AddScrubber(text => Regex.Replace(text.ToString(), @"Runtime \d+\.\d+\.\d+", "Runtime {version}"));
}
```

Centralize all scrubbers — per-test scrubbers fragment the contract and make snapshot diffs noisy across machines.
