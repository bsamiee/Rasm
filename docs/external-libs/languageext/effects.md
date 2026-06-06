# [LANGUAGEEXT_EFFECTS]

Runtime effects use concrete runtime records and `Eff.runtime<RT>()`. Boundary adapters may use statement control flow only with an explicit boundary marker in code.

## [1][RAILS]

| [INDEX] | [RAIL]                | [QUESTION]                         | [USE]                                                     |
| :-----: | --------------------- | ---------------------------------- | --------------------------------------------------------- |
|   [1]   | `Fin<T>`              | Did one operation succeed or fail? | Native calls, value admission, projection.                |
|   [2]   | `Validation<Error,T>` | Which inputs failed?               | Multi-input forms, symbols, requirement sets.             |
|   [3]   | `Eff<RT,T>`           | Which runtime context is needed?   | Runtime context, filesystem, clock, external service.     |
|   [4]   | `IO<T>`               | Which side-effect is deferred?     | Resource, file, process, and host execution descriptions. |

## [2][RUNTIME_RECORD]

Use `Eff.runtime<RT>()` to read host capability records, then project with `Map`, `Bind`, `MapFail`, `BiMap`, or LINQ query syntax. Keep runtime records small and concrete. Do not introduce service locators, decorator containers, or unused package posture when a runtime record owns the dependency.

## [3][SCHEDULE]

Schedule builders: `recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `jitter`, `maxDelay`.
Composition: `Schedule.a | Schedule.b`, `union`, `intersect`, `transformerA + transformerB` — full operator table in the same package doc set under operators §3.
Eff recovery: `Prelude.catch(...)`, `Prelude.retry(schedule, eff)`, `IfFailEff` — not generic `eff1 | eff2`.
IO: `IO<T>.Retry(Schedule)`, `Finally`, `Bracket`.

Pure domain `Fin`/`Validation` transforms avoid blocking delay schedules.

## [4][STATE]

| [INDEX] | [SURFACE]                           | [USE]                                                      |
| :-----: | ----------------------------------- | ---------------------------------------------------------- |
|   [1]   | `Atom<T>`                           | UI/session state with validated transitions.               |
|   [2]   | `AtomHashMap`, `AtomSeq`, `AtomQue` | Host-owned concurrent collections.                         |
|   [3]   | `Ref<T>` and STM                    | Coordinated mutable references under explicit transaction. |

Never use LanguageExt state to hide native object lifetime, host tree mutation, or ordinary domain accumulation.

## [5][RECOVERY]

- Convert native exceptions at boundary adapters into `Error` once.
- Prefer `MapFail`, `BiMap`, fallback alternatives, and verified catch combinators from local XML.
- Keep terminal collapse at command/component/tool edges.
- Preserve original operation, host object, tolerance, and input name in diagnostics.

## [6][CURRENT_SURFACE]

Current effect APIs route async and resource work through `Eff<RT,T>` and `IO<T>`:

| [INDEX] | [SURFACE]          | [PROJECT_USE]                                                               |
| :-----: | ------------------ | --------------------------------------------------------------------------- |
|   [1]   | Async effect leg   | `Eff<RT, T>` + `IO.liftAsync`.                                              |
|   [2]   | Resource lifetime  | `IO<T>.Bracket`/`BracketFail`/`Finally`, `Prelude.use`.                     |
|   [3]   | Runtime dependency | Concrete runtime records consumed via `Eff.runtime<RT>()`.                  |
|   [4]   | Stream-like state  | `Atom<T>` + paint, `Channel<T>`, or process cache.                          |
|   [5]   | Traverse lowering  | `TraverseM` -> `K<F, Seq<B>>`; lower via `>> lower`, unary `+`, or `.As()`. |

**Validation monoid requirement (v5):** `Validation<E, A>` requires `E : Monoid<E>`. `Validation<Error, T>` works when `Error` implements the required monoid; `Validation<string, T>` does NOT compile — use `StringM` or a monoidal error type. Consumer validation error policies belong in the local posture file.
**Atom surface (v5):** `Atom<T>.Swap`/`SwapMaybe`/`SwapIO`/`SwapMaybeIO`. **No `Subscribe`/`OnChange`/`Reset`** — react-to-state patterns use the host paint loop or polling. `Atom<TMetadata, T>` two-arg form threads metadata through swap functions without closure allocation.
**Pattern matching:** Record-case patterns are faster than `.Match` and prefer-able for hot paths: `fin switch { Fin.Succ<int>(var x) => x, Fin.Fail<int>(var e) => 0 }`.
