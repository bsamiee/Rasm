# [LANGUAGEEXT_API]

[IMPORTANT] Typical workspace imports: `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, and static `LanguageExt.Prelude` when global usings are enabled.

## [1][SURFACE_MAP]

| [INDEX] | [SURFACE]                    | [OWNS]                                     | [TYPICAL_USE]                                         |
| :-----: | ---------------------------- | ------------------------------------------ | ----------------------------------------------------- |
|   [1]   | `Option<T>`                  | Absence without failure.                   | Optional input after null/sentinel projection.        |
|   [2]   | `Fin<T>`                     | Synchronous fallible result.               | Default local validation and native call admission.   |
|   [3]   | `Validation<Error,T>`        | Independent error accumulation.            | Parallel requirements and multi-field validation.     |
|   [4]   | `Eff<RT,T>`                  | Runtime-record effects with typed failure. | Host context, IO, and boundary work.                  |
|   [5]   | `IO<T>`                      | Deferred side-effect/resource description. | Boundary execution before `Eff` or terminal result.   |
|   [6]   | `Schedule`                   | Retry, repeat, timeout, backoff algebra.   | Composition-root resilience.                          |
|   [7]   | `Seq<T>`, `Arr<T>`           | Immutable traversal shapes.                | Cross-module sequence and strict batch storage.       |
|   [8]   | `K<F,A>` and traits          | Effect-polymorphic algorithms.             | One algorithm, multiple carriers.                     |
|   [9]   | `Atom<T>`, `Ref<T>`          | Managed reactive state.                    | UI/session state — no Subscribe API in LanguageExt v5. |
|  [10]   | `HashMap<K,V>`, `HashSet<T>` | Immutable map/set.                         | Keyed memoization after key policy is explicit.       |

## [2][ADVANCED_INDEX]

| [INDEX] | [FILE]           | [OWNS]                                                                                      |
| :-----: | ---------------- | ------------------------------------------------------------------------------------------- |
|   [1]   | `operators.md`   | `\|`, `&`, `+` disambiguation across Schedule, Validation, Error, Eff/Finally, domain types |
|   [2]   | `prelude.md`     | Global static Prelude: `Some`, `Optional`, `guard`, `toSeq`, `unit`, `identity`             |
|   [3]   | `combinators.md` | `TraverseM` lowering, `Choose`, `BiBind`, `Atom.SwapMaybe`, recovery combinators            |
|   [4]   | `effects.md`     | Rail selection, Schedule names, v5 deltas, Atom v5 surface                                  |
|   [5]   | `rasm.md`        | Repo rail policy, boundary patterns, validation error shapes                                |
|   [6]   | `traits.md`      | Effect-polymorphic traits, `K<F,A>` algorithms                                              |
|   [7]   | `collections.md` | `Seq`, `Arr`, immutable collection patterns                                                 |

## [3][BOUNDARIES]

- Keep `Match`, `Run*`, and unsafe collapse at host boundaries only.
- Prefer `Flatten()` for nested rails when local XML proves availability.
- Keep host-owned mutable values out of memoized LanguageExt state unless ownership is explicit.
- Use `K<F,A>` only when it removes repeated algorithms, not as decorative abstraction.
- Do not document broader LanguageExt packages unless package graph state and a consumer make them active.
