# [H1][LANGUAGEEXT_API]
>**Dictum:** *LanguageExt owns rails, effects, and algebraic composition.*

<br>

[IMPORTANT] Pin **`LanguageExt.Core` `5.0.0-beta-77`**. Typical workspace imports: `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, and static `LanguageExt.Prelude` when global usings are enabled.

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

| [INDEX] | [SURFACE] | [OWNS] | [TYPICAL_USE] |
| :-----: | --------- | ------ | ------------- |
| [1] | `Option<T>` | Absence without failure. | Optional input after null/sentinel projection. |
| [2] | `Fin<T>` | Synchronous fallible result. | Default local validation and native call admission. |
| [3] | `Validation<Error,T>` | Independent error accumulation. | Parallel requirements and multi-field validation. |
| [4] | `Eff<RT,T>` | Runtime-record effects with typed failure. | Host context, IO, and boundary work. |
| [5] | `IO<T>` | Deferred side-effect and resource description. | Boundary execution before collapse into `Eff` or terminal result. |
| [6] | `Schedule` | Retry, repeat, timeout, backoff algebra. | Composition-root resilience. |
| [7] | `Seq<T>`, `Arr<T>` | Immutable traversal shapes. | Cross-module sequence and strict batch storage. |
| [8] | `K<F,A>` and traits | Effect-polymorphic algorithms. | Advanced compression when one algorithm targets multiple carriers. |
| [9] | `Atom<T>`, `Ref<T>` | Managed reactive state. | UI/session state — no Subscribe API in 5.0.0-beta-77. |
| [10] | `HashMap<K,V>`, `HashSet<T>` | Immutable map/set. | Keyed memoization after key policy is explicit. |

---
## [3][ADVANCED_INDEX]
>**Dictum:** *Operator and combinator detail lives in sibling files.*

<br>

| [INDEX] | [FILE] | [OWNS] |
| :-----: | ------ | ------ |
| [1] | `operators.md` | `\|`, `&`, `+` disambiguation across Option, Schedule, Validation, Error, domain types |
| [2] | `prelude.md` | Global static Prelude: `Some`, `Optional`, `guard`, `toSeq`, `unit`, `identity` |
| [3] | `combinators.md` | `TraverseM` lowering, `Choose`, `BiBind`, `Atom.SwapMaybe`, recovery combinators |
| [4] | `effects.md` | Rail selection, Schedule names, v5 deltas, Atom v5 surface |
| [5] | `rasm.md` | Repo rail policy, boundary patterns, validation error shapes |

---
## [4][BOUNDARIES]
>**Dictum:** *Collapse effects only where hosts require concrete output.*

<br>

- Keep `Match`, `Run*`, and unsafe collapse at host boundaries only.
- Prefer `Flatten()` for nested rails when local XML proves availability.
- Keep host-owned mutable values out of memoized LanguageExt state unless ownership is explicit.
- Use `K<F,A>` only when it removes repeated algorithms, not as decorative abstraction.
- Do not document broader LanguageExt packages unless pinned and consumed.
