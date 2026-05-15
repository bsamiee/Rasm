# [H1][LANGUAGEEXT_PACKAGE_REGISTRY]
>**Dictum:** *Package posture determines dependency permission.*

<br>

[IMPORTANT] Rasm currently pins `LanguageExt.Core` `5.0.0-beta-77` through central package management. Treat this version as the implementation baseline unless `Directory.Packages.props` changes.

---
## [1][ACTIVE_BASELINE]
>**Dictum:** *Only the pinned package is an implementation dependency today.*

<br>

| [INDEX] | [PACKAGE] | [VERSION] | [POSTURE] | [REPO_USE] |
| :-----: | --------- | --------: | :-------: | ---------- |
| **[1]** | `LanguageExt.Core` | `5.0.0-beta-77` | Use | Core FP surface for `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `Option<T>`, `Seq<T>`, traits, and collections. |

[CRITICAL] Do not add another LanguageExt package unless the target capability is absent from `LanguageExt.Core` and the new package is added through central package management.

---
## [2][OFFICIAL_PACKAGES]
>**Dictum:** *Official sibling packages are optional capability expansions.*

<br>

| [INDEX] | [PACKAGE] | [OBSERVED_VERSION] | [POSTURE] | [CAPABILITY] |
| :-----: | --------- | -----------------: | :-------: | ------------ |
| **[1]** | `LanguageExt.FSharp` | `5.0.0-beta-77` | Defer | F# interop for LanguageExt and F# native types. |
| **[2]** | `LanguageExt.Parsec` | `5.0.0-beta-77` | Defer | Parser combinators. |
| **[3]** | `LanguageExt.Rx` | `5.0.0-beta-77` | Defer | Reactive Extensions integration. |
| **[4]** | `LanguageExt.Streaming` | `5.0.0-beta-77` | Defer | Compositional streaming types. |
| **[5]** | `LanguageExt.Sys` | `5.0.0-beta-77` | Defer | Pure wrappers around common BCL IO behaviors. |
| **[6]** | `LanguageExt.Pipes` | `5.0.0-beta-50` | Ignore | Older prerelease streaming surface; prefer `LanguageExt.Streaming` if streaming is needed. |

---
## [3][LEGACY_PACKAGES]
>**Dictum:** *Legacy packages are not defaults for v5 code.*

<br>

| [INDEX] | [PACKAGE] | [OBSERVED_VERSION] | [POSTURE] | [REASON] |
| :-----: | --------- | -----------------: | :-------: | -------- |
| **[1]** | `LanguageExt.Transformers` | `4.4.8` | Ignore | Legacy transformer helpers; v5 core exposes current transformer and trait surfaces. |
| **[2]** | `LanguageExt.CodeGen` | `4.4.8` | Ignore | Superseded in this repo by Thinktecture source generation. |
| **[3]** | `LanguageExt.SysX` | `4.4.8` | Ignore | Legacy Sys extension package. |

---
## [4][ADJACENT_PACKAGES]
>**Dictum:** *Community packages require explicit need and compatibility proof.*

<br>

| [INDEX] | [PACKAGE] | [OBSERVED_VERSION] | [POSTURE] | [CAPABILITY] |
| :-----: | --------- | -----------------: | :-------: | ------------ |
| **[1]** | `LanguageExt.UnitTesting` | `4.4.11` | Defer | Testing extensions. |
| **[2]** | `LanguageExt.AutoFixture` | `2.0.1` | Defer | AutoFixture integration. |
| **[3]** | `LanguageExt.Bson` | `0.4.0` | Defer | MongoDB BSON serializers. |
| **[4]** | `LanguageExt.Bson.DependencyInjection` | `0.4.0` | Defer | BSON DI registration. |
| **[5]** | `LanguageExt.AspNetCore.NativeTypes` | `0.2.0` | Defer | ASP.NET Core model binding and response handling. |
| **[6]** | `LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson` | `0.1.0` | Defer | Newtonsoft JSON integration. |
| **[7]** | `LanguageExt.JsonSerializer` | `0.0.5` | Ignore | Unused serializer package. |
| **[8]** | `LanguageExt.Json` | `0.1.0` | Ignore | Experimental JSON package. |
| **[9]** | `LanguageExt.Http` | `0.1.0` | Ignore | Experimental HTTP package. |
| **[10]** | `LanguageExt.Net.Http` | `0.1.0-alpha-4` | Ignore | Experimental HTTP package. |
| **[11]** | `LanguageExt.Effects.Database` | `1.0.0-preview9` | Ignore | Old database effect package; prefer repo-owned `Eff<RT,T>` persistence patterns when persistence appears. |

---
## [5][RULES]
>**Dictum:** *Package expansion must reduce code, not add ceremony.*

<br>

- Use `LanguageExt.Core` as the default source for FP primitives.
- Defer official sibling packages until code needs their exact domain.
- Ignore legacy v4 packages unless a migration task explicitly requires them.
- Reject packages that duplicate Thinktecture, RhinoCommon, GH2, or repo-owned abstractions.
