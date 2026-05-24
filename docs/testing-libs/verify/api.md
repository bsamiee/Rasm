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
