# [H1][LANGUAGEEXT_API]
>**Dictum:** *LanguageExt owns rails, effects, and algebraic composition.*

<br>

[IMPORTANT] Rasm pins `LanguageExt.Core` `5.0.0-beta-77` and imports `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, and static `LanguageExt.Prelude` from `Directory.Build.props`.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Local beta XML is the exact API contract.*

<br>

| [INDEX] | [SOURCE] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Directory.Packages.props` | Confirms `LanguageExt.Core` `5.0.0-beta-77`. |
| [2] | `~/.nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.xml` | Exact public API. |
| [3] | `~/.nuget/packages/languageext.core/5.0.0-beta-77/languageext.core.nuspec` | Package metadata and source commit. |
| [4] | NuGet and upstream docs | Secondary feature context. |

---
## [2][SURFACE_MAP]
>**Dictum:** *Choose the rail by failure and execution semantics.*

<br>

| [INDEX] | [SURFACE] | [OWNS] | [RASM_USE] |
| :-----: | --------- | ------ | ---------- |
| [1] | `Option<T>` | Absence without failure. | Optional host input after native null/sentinel projection. |
| [2] | `Fin<T>` | Synchronous fallible result. | Default local validation and native call admission rail. |
| [3] | `Validation<Error,T>` | Independent error accumulation. | Parallel requirements, symbol sets, and multi-input GH validation. |
| [4] | `Eff<RT,T>` | Runtime-record effects with typed failure. | Host context, IO, and Rhino/GH boundary work. |
| [5] | `IO<T>` | Deferred side-effect and resource description. | Boundary execution before collapse into `Eff` or terminal result. |
| [6] | `Schedule` | Retry, repeat, timeout, backoff algebra. | Composition-root resilience after a concrete boundary need exists. |
| [7] | `Seq<T>`, `Arr<T>` | Immutable traversal shapes. | Cross-module sequence and strict batch storage. |
| [8] | `K<F,A>` and traits | Effect-polymorphic algorithms. | Advanced compression when one algorithm targets multiple carriers. |
| [9] | `Atom<T>`, `Ref<T>` | Managed host state. | UI, bridge, subscription, or session state only. |

---
## [3][BOUNDARIES]
>**Dictum:** *Rasm collapses effects only where hosts require concrete output.*

<br>

- Keep `Match`, `Run*`, and unsafe collapse at Rhino command, GH2 component, CLI, or test boundaries.
- Prefer `Flatten()` for nested rails when local XML proves availability.
- Keep Rhino-owned mutable geometry out of memoized LanguageExt state unless ownership is explicit.
- Use `K<F,A>` only when it removes repeated algorithms, not as decorative abstraction.
- Do not document broader LanguageExt packages unless Rasm pins and consumes them.
