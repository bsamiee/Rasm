# [H1][VERIFY_API]
>**Dictum:** *Snapshots compare stable artifacts; they are not domain oracles.*

<br>

[IMPORTANT] Rasm uses `Verify.XunitV3 31.17.0` in `tests/csharp/_tooling` only with concrete snapshot-worthy tests. Do not use Verify for algebraic laws, numeric behavior, or Rhino/GH native truth.

---
## [1][PACKAGE]
>**Dictum:** *Use the xUnit v3 package rail.*

<br>

| [INDEX] | [PACKAGE] | [PIN] | [USE] |
| :-----: | --------- | ----- | ----- |
| [1] | `Verify.XunitV3` | `31.17.0` | xUnit v3 snapshot assertions. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/Verify.XunitV3/31.17.0

---
## [2][SETTINGS]
>**Dictum:** *Determinism must be global and explicit.*

<br>

| [INDEX] | [SURFACE] | [RASM_USE] |
| :-----: | --------- | ---------- |
| [1] | `VerifierSettings` | Add module-initializer scrubbers only when a snapshot contains machine-specific data. |
| [2] | `VerifySettings` | Per-test path/name tweaks only when the artifact owner requires it. |
| [3] | `Verifier.DerivePathInfo` | Keep snapshots beside owning specs or under a stable artifact directory. |
| [4] | Scrubbers | Remove machine paths, timestamps, generated IDs, and runtime-specific noise. |
| [5] | `Verify.Cli` | Review/accept received files manually; never auto-accept in scripts. |

---
## [3][RASM_SCOPE]
>**Dictum:** *A verified file must encode a stable public artifact.*

<br>

Use Verify for analyzer diagnostics, generated manifests, normalized bridge JSON, or package/config reports. Avoid snapshots for floating numeric values, generated random samples, current implementation output, or anything better described by an algebraic law.

---
## [4][NATURAL_USE_CASES_IN_RASM]
>**Dictum:** *Snapshots earn their place by encoding what an algebraic law cannot.*

<br>

| [INDEX] | [USE_CASE] | [WHY_VERIFY_FITS] |
| :-----: | ---------- | ----------------- |
| [1] | `tools/cs-analyzer` rule diagnostics | Per-rule fixture produces a deterministic diagnostic message; Verify pins the exact text including parameter names. Drift catches accidental wording changes that break IDE quick-fix UX. |
| [2] | SmartEnum catalog enumerations | When a public catalog (e.g., `CurveProjection.Items`) is API surface, snapshotting the list catches accidental case reorder/rename in code review. Pair with `Spec.SmartEnumKeysUnique` for runtime contract. |
| [3] | Bridge JSON evidence files | Normalized `vectors-*-verify.csx` JSON output under `.artifacts/rhino/verify/` can be snapshot-asserted in `_tooling` after the bridge has stabilized. |
| [4] | Generated source files | Roslyn source-generators (Thinktecture `[Union]`/`[SmartEnum]`/`[ValueObject]`) produce `*.g.cs` under `obj/`. Snapshotting select generated files catches breaking changes in source-generator behavior between Thinktecture upgrades. |
| [5] | Package/config reports | Normalized output of `dotnet list package --vulnerable`, `dotnet restore --locked-mode` reports. |

Anti-uses (Grade F):
- Snapshotting `Matrix.spec.cs` SVD reconstruction output is not a law; the math IS the oracle.
- Snapshotting bridge scenario stdout when it includes wall-clock timing or RhinoWIP version strings — scrub or skip.

---
## [5][SCRUBBER_RAIL]
>**Dictum:** *Scrubbers are part of the contract, not noise reduction.*

<br>

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
