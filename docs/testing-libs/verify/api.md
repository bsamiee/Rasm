# [H1][VERIFY_API]

[IMPORTANT] Rasm uses `Verify.XunitV3` (version pinned in `Directory.Packages.props`) in `tests/csharp/_tooling` only with concrete snapshot-worthy tests. Do not use Verify for algebraic laws, numeric behavior, or Rhino/GH native truth.

## [1][PACKAGE]

| [INDEX] | [PACKAGE]        | [PIN]                      | [USE]                         |
| :-----: | ---------------- | -------------------------- | ----------------------------- |
|   [1]   | `Verify.XunitV3` | `Directory.Packages.props` | xUnit v3 snapshot assertions. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/Verify.XunitV3

## [2][SETTINGS]

| [INDEX] | [SURFACE]                 | [RASM_USE]                                                                            |
| :-----: | ------------------------- | ------------------------------------------------------------------------------------- |
|   [1]   | `VerifierSettings`        | Add module-initializer scrubbers only when a snapshot contains machine-specific data. |
|   [2]   | `VerifySettings`          | Per-test path/name tweaks only when the artifact owner requires it.                   |
|   [3]   | `Verifier.DerivePathInfo` | Keep snapshots beside owning specs or under a stable artifact directory.              |
|   [4]   | Scrubbers                 | Remove machine paths, timestamps, generated IDs, and runtime-specific noise.          |
|   [5]   | `Verify.Cli`              | Review/accept received files manually; never auto-accept in scripts.                  |

## [3][RASM_SCOPE]

Use Verify for analyzer diagnostics, generated manifests, normalized bridge JSON, or package/config reports. Avoid snapshots for floating numeric values, generated random samples, current implementation output, or anything better described by an algebraic law.

## [4][NATURAL_USE_CASES_IN_RASM]

| [INDEX] | [USE_CASE]                           |
| :-----: | ------------------------------------ |
|   [1]   | `tools/cs-analyzer` rule diagnostics |
|   [2]   | SmartEnum catalog enumerations       |
|   [3]   | Bridge JSON evidence files           |
|   [4]   | Generated source files               |
|   [5]   | Package/config reports               |

[WHY_VERIFY_FITS]
- [1] Per-rule fixture produces deterministic diagnostic text; drift catches wording changes that break IDE quick-fix UX.
- [2] When a public catalog is API surface, snapshotting the list catches accidental case reorder/rename in code review. Pair with `Spec.SmartEnumKeysUnique`.
- [3] Normalized `vectors-*-verify.csx` JSON under `.artifacts/rhino/verify/` after bridge stabilizes.
- [4] Snapshot select Thinktecture `*.g.cs` under `obj/` to catch source-generator behavior changes on upgrade.
- [5] Normalized output of `dotnet list package --vulnerable`, `dotnet restore --locked-mode` reports.

Anti-uses (Grade F):
- Snapshotting `Matrix.spec.cs` SVD reconstruction output is not a law; the math IS the oracle.
- Snapshotting bridge scenario stdout when it includes wall-clock timing or RhinoWIP version strings — scrub or skip.

## [5][SCRUBBER_RAIL]

Module initializer in `tests/csharp/_tooling/ModuleInitializers.cs`:

```csharp
[ModuleInitializer]
public static void InitVerify() {
    VerifierSettings.AddScrubber(text => text.Replace(Environment.MachineName, "{machine}"));
    VerifierSettings.AddScrubber(text => Regex.Replace(text.ToString(), @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", "{timestamp}"));
    VerifierSettings.AddScrubber(text => Regex.Replace(text.ToString(), @"RhinoWIP \d+\.\d+\.\d+", "RhinoWIP {version}"));
}
```

Centralize all scrubbers — per-test scrubbers fragment the contract and make snapshot diffs noisy across machines.
